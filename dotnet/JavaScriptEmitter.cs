using System;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EdgeReference
{
  public class JavaScriptEmitter : CodeEmitter
  {
    public const string EdgeTypeName = "edge";

    public const string EdgeModuleName = "edge";

    public const string EdgeReferenceTypeName = "EdgeReference";

    public const string EdgeReferenceModuleName = "edge-reference";

    /**
     * 0 - indent
     * 1 - static
     * 2 - name
     * 3 - additional indent
     * 4 - property body
     */
    private const string GetterTemplate = @"{0}{1}get {2}() {{
{3}
{0}}}";

    /**
     * 0 - indent
     * 1 - static
     * 2 - name
     * 3 - additional indent
     * 4 - property body
     */
    private const string SetterTemplate = @"{0}{1}set {2}(value) {{
{3}
{0}}}";

  private Dictionary<string, bool> appended = new Dictionary<string, bool>();

    /// <summary>
    /// The name of the JavaScript class.
    /// </summary>
    private string javaScriptClassName;

    /// <summary>
    /// The source type's full name with &quot.&quot; replaced with 
    /// &quot-&quot;.  Intended for use as a file name.
    /// </summary>
    private string javaScriptFullName;

    #region .Net Wrapping

    public void AppendReferenceHeader() 
    {
      this.buffer.AppendLine("const Reference = edge(function () { /*");
    }

    public void AppendReferenceFooter()
    {
      this.buffer.AppendLine("*/}));");
    }

    #endregion

    #region Requires

    /// <summary>
    /// Constructs and appends JavaScript require statements for the file.
    /// </summary>
    /// <param name="target">Target.</param>
    public void AppendBasicRequires(Type target) 
    {
      AppendRequire(EdgeTypeName, EdgeModuleName);

      if (target.BaseType != null && target.BaseType != typeof(object)) {
        AppendRequire(target.BaseType);
      }
    }

    public void AppendRequires(IEnumerable<Type> referenceTypes) {

      foreach (Type type in referenceTypes) {
        if (!this.appended.ContainsKey(type.Name)
            && ReflectionUtils.IsReferenceType(type)) {
          this.appended.Add(type.Name, true);
          this.AppendRequire(type);        
        }
      }
    }

    private void AppendRequire(string name, string file)
    {
      buffer.AppendFormat (
        CultureInfo.InvariantCulture,
        "const {0} = require('{1}');",
        name,
        file);

      buffer.AppendLine ();
    }

    private void AppendRequire(Type type) {
      string name = type.Name;

      // TODO: look up name in collection of existing names.
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "const {0} = require('{1}');",
        name,
        ReflectionUtils.ConvertFullName(type.FullName));

      buffer.AppendLine();
    }

    #endregion

    #region Class

    public void AppendClassDefinition(Type target) 
    {
      string name = target.Name;
      const string ClassDefinitionTemplate = @"{0}class {1}{2} {{";

      string extendsStatement = string.Empty;
      // If this class inherits from something, so should the proxy.
      // TODO: Look up type names here
      string baseClass =
        (target.BaseType != typeof(object)) 
        ? target.BaseType.Name
        : EdgeReferenceTypeName;

      extendsStatement = string.Concat(" extends ", baseClass);

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ClassDefinitionTemplate,
        this.CurrentIndent, 
        name,
        extendsStatement);

      this.buffer.AppendLine();
      this.buffer.AppendLine();

      // Add indent for future declarations
      this.Indent();
    }

    public void AppendClassTermination()
    {
      // Outdent
      this.Outdent();

      buffer.Append(this.CurrentIndent);
      buffer.AppendLine("}");
    }

    #endregion Class

    #region Properties

    /// <summary>
    /// Generates and appends a JavaScript property to the ProxyGenerator's 
    /// internal buffer.
    /// </summary>
    /// <param name="source">
    /// Information about the property to be generated.
    /// </param>
    /// <param name="isStatic">
    /// If set to <c>true</c>, the member is static.
    /// </param>
    /// <remarks>
    /// The property info provided could be used to determine whether the 
    /// member is static; however a parameter is more convenient.
    /// </remarks>
    public void AppendProperty(PropertyInfo source, bool isStatic)
    {
      string baseIndent = this.CurrentIndent;
      string staticModifier = isStatic ? "static " : string.Empty;

      string getterBody = GenerateGetterBody(
        source.Name,
        source.PropertyType,
        isStatic);

      string setterBody = GenerateSetterBody(
        source.Name,
        source.PropertyType,
        isStatic);

      Action<string, string> formatAccessor = (formatString, body) => {
        this.buffer.AppendFormat(
          CultureInfo.InvariantCulture,
          formatString,
          baseIndent,
          staticModifier,
          source.Name,
          body);
      };

      // used twice
      MethodInfo setter = source.GetSetMethod();
      MethodInfo getter = source.GetGetMethod();
      bool canWrite = source.CanWrite && setter != null && setter.IsPublic;

      // Note that public properties are defined as properties with a 
      // public getter OR setter - therefore make sure the accessor is 
      // public.
      if (source.CanRead && getter != null && getter.IsPublic) {
        formatAccessor(GetterTemplate, getterBody);

        if (canWrite) {
          this.AppendBreak ();
        }
      }

      if (canWrite) {
        formatAccessor(SetterTemplate, setterBody);
      }
    }

    private string GenerateGetterBody(string name, Type type, bool isStatic) {
      string result;

      if (ReflectionUtils.IsReferenceType(type)) {
        result = string.Format(
          CultureInfo.InvariantCulture,
          @"{0}{1}var returnId = Reference.Get_{2}({3});
{0}{1}return new {4}(returnId);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId",
          this.DetermineJavaScriptTypeName(type));
      } else {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}return Reference.Get_{2}({3});",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId");
      }

      return result;
    }

    private string GenerateSetterBody(string name, Type type, bool isStatic) {
      string result;

      // TODO: Here, only one parameter can be provided.
      if (ReflectionUtils.IsReferenceType(type)) {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}Reference.Set_{2}({3}value._edgeId));",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId, ");
      } else {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}Reference.Set_{2}({3}value));",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "this._referenceId, ");
      }

      return result;
    }

    #endregion Properties

    #region Functions

    public void AppendFunction(MethodInfo source, bool isStatic) 
    {
      // Build argument name references
      ParameterInfo[] arguments = source.GetParameters();
      string[] argumentNames = arguments
        .Select((parameter) => {
          return parameter.Name;
        })
        .ToArray();

      string argumentNameList = string.Join(", ", argumentNames);

      const string SignatureTemplate = 
        "{0}{1}({2}) {{";

      this.buffer.AppendFormat( 
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        source.Name,
        argumentNameList);

      this.buffer.AppendLine();
      // Indent
      this.Indent();

      // Append argument conversions
      this.AppendArgumentConversions(source);

			this.buffer.AppendLine();
			this.buffer.AppendLine();

      // Append call line
      this.buffer.AppendLine(GenerateFunctionCall(
        source,
        isStatic,
        arguments));

      // Append return line
      this.buffer.AppendLine(GenerateReturnLine(source.ReturnType));

      // Outdent
      this.Outdent();

      this.buffer.AppendFormat (
        CultureInfo.InvariantCulture,
        "{0}}}",
        this.CurrentIndent);
    }

    private void AppendArgumentConversions(MethodInfo source)
    {
      foreach (ParameterInfo parameter in source.GetParameters()) {
            AppendArgumentConversion(parameter);
      }
    }

    private void AppendArgumentConversion(ParameterInfo argument) 
    {
      const string ArgumentConversionLineTemplate = 
        "{0}{1} = {1} ? {1}._edgeId : 0;";

      if (ReflectionUtils.IsReferenceType(argument.ParameterType)) {
        this.buffer.AppendFormat(
          CultureInfo.InvariantCulture,
          ArgumentConversionLineTemplate,
				this.CurrentIndent,
          argument.Name);
      }
    }
  
    private string GenerateFunctionCall(
      MethodInfo source,
      bool isStatic,
      ParameterInfo[] arguments)
    {

      const string FunctionCallLineTemplate =
        "{0}var result = Reference.{1}({2});";
      
      string referenceParameter = 
        isStatic ? 
        string.Empty :
        "_referenceId: _referenceId, " + Environment.NewLine;

      string argumentObject = string.Concat(
        "{",
        referenceParameter,
        string.Join(
          "," + Environment.NewLine + this.CurrentIndent,
          arguments.Select(parameter => {
            return string.Concat(
              parameter.Name,
              ": ",
              parameter.Name);
            })
            .ToArray()),
        "}");

      return string.Format(
        CultureInfo.InvariantCulture,
        FunctionCallLineTemplate,
        this.CurrentIndent,
        source.Name,
        argumentObject);
    }

    private string GenerateReturnLine(Type returnType) {
      const string ComplexReturnLineTemplate = "{0}return new {1}(result);";
      const string SimpleReturnLineTemplate = "{0}return result;";

      if (ReflectionUtils.IsReferenceType(returnType)) {
        return string.Format(
          CultureInfo.InvariantCulture,
          ComplexReturnLineTemplate,
          this.CurrentIndent,
          DetermineJavaScriptTypeName(returnType));
      } else {
        return string.Format(
          CultureInfo.InvariantCulture,
          SimpleReturnLineTemplate,
          this.CurrentIndent);
      }
    }

    #endregion Functions

    public void AppendBreak() {
      this.buffer.AppendLine();
      this.buffer.AppendLine();
    }

    public void AppendLine() {
      this.buffer.AppendLine();
    }

    /// <summary>
    /// Looks up the JavaScript type name for the specified type.
    /// </summary>
    private string DetermineJavaScriptTypeName(Type type) {
      // TODO: Look up set of stored names here in case of naming conflict
      return type.Name;
    }
  }
}

