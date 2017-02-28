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

// TODO: Potentially inconsistent line endings here
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
      AppendRequire(EdgeReferenceTypeName, EdgeReferenceModuleName);

      if (target.BaseType != null && target.BaseType != typeof(object)) {
        AppendRelativeRequire(target.BaseType);
      }
    }

    public void AppendRequires(IEnumerable<Type> referenceTypes) {

      foreach (Type type in referenceTypes) {
        if (!this.appended.ContainsKey(type.Name)
            && ReflectionUtils.IsReferenceType(type)) {
          this.appended.Add(type.Name, true);
          this.AppendRelativeRequire(type);        
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

    private void AppendRelativeRequire(Type type) {
      string name = type.Name;

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "const {0} = require('./{1}.js');",
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

    #region Exports

    public void AppendExport(Type target)
    {
      this.buffer.AppendLine();
      this.buffer.Append("module.exports = ");
      this.buffer.AppendLine(target.Name);
    }

    #endregion

    #region Constructors

    public void AppendConstructor(ConstructorInfo source)
    {
      const string ConstructorTemplate = 
        "{0}constructor(referenceId, args) {{";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ConstructorTemplate,
        this.CurrentIndent);
      
      this.BlockStart();

      // TODO: allow multiple constructors with args passed in
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}super(referenceId, Constructor, args);",
        this.CurrentIndent);

      this.buffer.AppendLine();

      this.BlockEnd();
    }

    #endregion

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
          @"{0}{1}var returnId = Get_{2}({3}, true);
{0}{1}return (returnId ? new {4}(returnId) : null);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? "null" : "this._referenceId",
          this.DetermineJavaScriptTypeName(type));
      } else {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}return Get_{2}({3}, true);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? "null" : "this._referenceId");
      }

      return result;
    }

    private string GenerateSetterBody(string name, Type type, bool isStatic) {
      string result;

      if (ReflectionUtils.IsReferenceType(type)) {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}Set_{2}({{ {3}value: value._referenceId }}, true);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "_referenceId: this._referenceId, ");
      } else {
        result = string.Format(
          CultureInfo.InvariantCulture,
          "{0}{1}Set_{2}({{ {3}value: value }}, true);",
          this.CurrentIndent,
          this.incrementalIndent,
          name,
          isStatic ? string.Empty : "_referenceId: this._referenceId, ");
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

      if (argumentNameList.Length > 0) {
        argumentNameList += ", ";
      }

      const string SignatureTemplate = 
        "{0}{1}{2}({3}callback) {{";

      this.buffer.AppendFormat( 
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        isStatic ? "static " : string.Empty,
        source.Name,
        argumentNameList);

      this.buffer.AppendLine();

      // Indent
      this.Indent();

      // Append argument conversions
      if (this.AppendArgumentConversions(source))
      {
        this.buffer.AppendLine();
        this.buffer.AppendLine();
      }

      // Append call line
      this.buffer.AppendLine(GenerateFunctionCall(
        source,
        isStatic,
        arguments));

      // Outdent
      this.Outdent();

      this.buffer.AppendFormat (
        CultureInfo.InvariantCulture,
        "{0}}}",
        this.CurrentIndent);
    }

    private bool AppendArgumentConversions(MethodInfo source)
    {
      bool result = false;
      foreach (ParameterInfo parameter in source.GetParameters()) {
        result |= AppendArgumentConversion(parameter);
      }

      return result;
    }

    private bool AppendArgumentConversion(ParameterInfo argument) 
    {
      const string ArgumentConversionLineTemplate = 
        "{0}{1} = {1} ? {1}._referenceId : 0;";

      if (ReflectionUtils.IsReferenceType(argument.ParameterType)) {
        this.buffer.AppendFormat(
          CultureInfo.InvariantCulture,
          ArgumentConversionLineTemplate,
          this.CurrentIndent,
          argument.Name);

        return true;
      }

      return false;
    }

    private string GenerateFunctionCall(
      MethodInfo source,
      bool isStatic,
      ParameterInfo[] arguments)
    {
      const string FunctionCallLineTemplate =
        "{0}{1}EdgeReference.callbackOrReturn({2}{3},{2}{4},{2}{5},{2}callback);";

      string referenceParameter;
      IEnumerable<string> argumentContent = arguments.Select(parameter => {
        return string.Concat(
          parameter.Name,
          ": ",
          parameter.Name);
        });

      if (!isStatic)
      {
        argumentContent = (new string[] { 
          "_referenceId: this._referenceId"
        }).Concat(argumentContent);
      }

      string divider = string.Concat(
          ",",
          Environment.NewLine,
          this.CurrentIndent,
          this.incrementalIndent,
          this.incrementalIndent
      );

      string argumentObject;

      if (string.IsNullOrWhiteSpace(divider)) {
        argumentObject = "{}";
      }
      else
      {
        argumentObject = string.Concat(
          "{",
          Environment.NewLine,
          this.CurrentIndent,
          this.incrementalIndent,
          this.incrementalIndent,
          string.Join(divider, argumentContent.ToArray()),
          Environment.NewLine,
          this.CurrentIndent,
          this.incrementalIndent,
          "}");
      }

      string returnStatement;
      string wrapperType;

      if (source.ReturnType == typeof(void)) {
        returnStatement = string.Empty;
        wrapperType = "null";
      }
      else
      {
        returnStatement = "return ";
        wrapperType = source.ReturnType.Name;
      }

      return string.Format(
        CultureInfo.InvariantCulture,
        FunctionCallLineTemplate,
        this.CurrentIndent,
        returnStatement,
        Environment.NewLine + this.CurrentIndent + this.incrementalIndent,
        source.Name,
        argumentObject,
        wrapperType);
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

    protected override void BlockStart()
    {
      this.buffer.AppendLine();
      this.Indent();
    }

  }
}

