/**
 * DotNetEmitter class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.IO;
using EdgeReference;

namespace EdgeGenerator
{
  /// <summary>
  /// This class emits C# code used in the construction of a JavaScript proxy 
  /// for a .NET type.
  /// </summary>
  public class DotNetEmitter : CodeEmitter
  {
    /// <summary>
    /// The type being used to build the proxy.
    /// </summary>
    private Type source;

    /// <summary>
    /// Assemblies that have already been referenced in emitted code.
    /// </summary>
    private Dictionary<string, bool> referencedAssemblies;

    /// <summary>
    /// Namespaces that have already been referenced in emitted code.
    /// </summary>
    private Dictionary<string, bool> referencedNamespaces;

    /// <summary>
    /// Creates and initializes a new instance of the DotNetEmitter.
    /// </summary>
    /// <param name="source">
    /// The type for which proxy code will be emitted.
    /// </param>
    /// <param name="existingBuffer">
    /// If .NET code is being emitted into an existing file, this buffer can 
    /// be supplied.  Otherwise, exclude this value and a new buffer will be 
    /// created.
    /// </param>
    public DotNetEmitter(Type source, StringBuilder existingBuffer = null)
    {
      this.referencedAssemblies = new Dictionary<string, bool>();
      this.referencedNamespaces = new Dictionary<string, bool>();
      this.source = source;

      // Can optionally supply a buffer that has already been initialized.
      if (existingBuffer == null) {
        this.buffer = new StringBuilder();
      } else {
        this.buffer = existingBuffer;
      }
    }

    /// <summary>
    /// Resets formatting and references for this builder.  Used to clear 
    /// generation context for a separate, unrelated block of code.
    /// </summary>
    public void Reset()
    {
      this.referencedAssemblies.Clear();
      this.referencedNamespaces.Clear();
      this.currentIndentWidth = 0;
    }

    #region References

    /// <summary>
    /// Appends a reference for the specified type's assembly file, if one 
    /// is not defined already.
    /// </summary>
    public void AppendReferenceFor(Type type, string deploymentDirectory = null)
    {
      string belongsTo = type.Assembly.FullName;

      // Already referenced; don't add again
      if (this.referencedAssemblies.ContainsKey(belongsTo)) {
        return;
      }

      this.referencedAssemblies.Add(belongsTo, true);

      // Determine file name of assembly.
      string assemblyFileName = type.Assembly.Location;
      assemblyFileName = Path.GetFileName(assemblyFileName);

      if (deploymentDirectory != null) {
        assemblyFileName = Path.Combine(deploymentDirectory, assemblyFileName);
      }

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "#r \"{0}\"",
        assemblyFileName);

      this.buffer.AppendLine();
    }

    /// <summary>
    /// Appends a using statement for the specified type's namespace, if one 
    /// is not defined already.
    /// </summary>
    public void AppendUsingFor(Type type)
    {
      string ns = type.Namespace;

      if (this.referencedNamespaces.ContainsKey(ns)) {
        return;
      }

      this.referencedNamespaces.Add(ns, true);

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "using {0};",
        ns);

      this.buffer.AppendLine();
    }

    #endregion

    #region Namespace

    /// <summary>
    /// Appends the opening of a namespace corresponding to the type's 
    /// namespace.
    /// </summary>
    public void AppendNamespace()
    {
      const string NamespaceTemplate = "namespace EdgeReference.{0}";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        NamespaceTemplate,
        this.source.Namespace);

      this.buffer.AppendLine();
      this.buffer.AppendLine("{");

      this.Indent();
    }

    /// <summary>
    /// Appends the close of a namespace corresponding to the type's namespace.
    /// </summary>
    public void AppendNamespaceEnd() 
    {
      this.Outdent();
      this.buffer.AppendLine("}");
    }

    #endregion

    #region Class

    /// <summary>
    /// Appends the opening of the proxy class.
    /// </summary>
    /// <param name="className">
    /// The name of the class to be emitted; if not included, uses the name of 
    /// the source type, plus the string "Proxy".
    /// </param>
    public void AppendClassStart(string className = null) 
    {
      className = className ?? (this.source.Name + "Proxy");

      const string ClassHeaderTemplate = "{0}public class {1}";
      
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ClassHeaderTemplate,
        this.CurrentIndent,
        className);

      this.buffer.AppendLine();

      this.BlockStart();
    }

    /// <summary>
    /// Appends the closing of a class.
    /// </summary>
    public void AppendClassEnd()
    {
      this.BlockEnd();
    }

    #endregion

    #region Edge functions

    /// <summary>
    /// Appends the opening of an edge.js standard function.
    /// </summary>
    /// <param name="name">
    /// The name of the function in JavaScript code.
    /// </param>
    public void AppendEdgeFuncStart(string name) {
      const string EdgeFuncTemplate = 
        "var {0} = edge.func({{ source: () => {{/*";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        EdgeFuncTemplate,
        name);

      this.buffer.AppendLine();
    }

    /// <summary>
    /// Appends the closing of an edge.js standard function.
    /// </summary>
    /// <param name="methodName">
    /// The name of the function in C# code.
    /// </param>
    public void AppendEdgeFuncEnd(string methodName) {
      if (string.IsNullOrWhiteSpace(methodName)) {
        this.buffer.AppendLine("*/}});");
        this.buffer.AppendLine();
      } else {
        this.buffer.AppendLine("*/");
        this.buffer.AppendLine("}, ");
        this.Indent();
        this.buffer.Append(this.CurrentIndent);
        this.buffer.Append("methodName: '");
        this.buffer.Append(methodName);
        this.buffer.AppendLine("'");
        this.Outdent();
        this.buffer.AppendLine("});");
        this.buffer.AppendLine();
      }

      this.Reset();
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Appends the closing of an edge.js standard function.
    /// </summary>
    /// <param name="methodName">
    /// The name of the function in C# code.
    /// </param>
    public void AppendStandaloneConstructors(ConstructorInfo[] constructors)
    {
      for (var i=0; i<constructors.Length; i++) {
        this.AppendStandaloneConstructor(constructors[i]);
      }
    }

    private void AppendStandaloneConstructor(ConstructorInfo info)
    {
      this.AppendDefaultStandaloneStart(
        "Constructor",
        new Type[] { this.source });

      this.AppendConstructor(info);
      this.AppendDefaultStandaloneEnd("ConstructInstance");
    }

    public void AppendConstructor(ConstructorInfo info)
    {
      const string SignatureTemplate = 
        "{0}public async Task<object> ConstructInstance(dynamic input)";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent);
      
      this.buffer.AppendLine();

      this.BlockStart();

      this.AppendConstructionLine(info);
      this.AppendConvertAndReturn(this.source);

      this.BlockEnd();
    }

    private void AppendConstructionLine(ConstructorInfo info)
    {
      const string ConstructionTemplate = 
        "{0}{1} _result = new {1}({2});";

      // TODO: Arguments
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ConstructionTemplate,
        this.CurrentIndent,
        this.source.FullName,
        string.Empty);

      this.buffer.AppendLine();
    }

    #endregion

    #region Properties

    public void AppendGetter(PropertyInfo info, bool isStatic)
    {
      MethodInfo getter = info.GetGetMethod();

      if (getter == null) {
        return;
      }

      const string SignatureTemplate = 
        "{0}public async Task<object> Get_{1}({2})";

      string instanceReference = 
        isStatic
          ? "object unused" 
          : "object _referenceId";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        info.Name,
        instanceReference);

      this.buffer.AppendLine();

      this.BlockStart();

      if (!isStatic) {
        this.AppendReferenceIdConversion("_referenceId");
        this.AppendParentReferenceFetch("_refId");
      }

      this.AppendGetterPropertyRetrieval(info.Name, getter);

      this.AppendConvertAndReturn(info.PropertyType);

      this.BlockEnd();
    }

    private void AppendParentReferenceFetch(string referenceParameterName)
    {
      const string ReferenceFetchTemplate = "{0}{1} _parent = ({1}){2};";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ReferenceFetchTemplate,
        this.CurrentIndent,
        this.source.FullName,
        GenerateReferenceRetrieval(referenceParameterName));

      this.buffer.AppendLine();
    }

    public void AppendStandaloneProperties(
      PropertyInfo[] properties,
      bool isStatic)
    {
      for (var i=0; i<properties.Length; i++) {
        AppendStandaloneProperty(properties[i], isStatic);
      }
    }

    private void AppendStandaloneProperty(PropertyInfo property, bool isStatic)
    {
      this.AppendStandaloneGetter(property, isStatic);
      this.AppendStandaloneSetter(property, isStatic);
    }

    private void AppendStandaloneGetter(PropertyInfo property, bool isStatic)
    {
      if (property.GetGetMethod() == null) {
        return;
      }

      string getterName = "Get_" + property.Name;
      this.AppendDefaultStandaloneStart(
        getterName,
        new Type[] { property.PropertyType });

      // Actual getter
      this.AppendGetter(property, isStatic);

      this.AppendDefaultStandaloneEnd(getterName);
    }

    private void AppendStandaloneSetter(PropertyInfo property, bool isStatic)
    {
      if (property.GetSetMethod() == null) {
        return;
      }

      string setterName = "Set_" + property.Name;
      this.AppendDefaultStandaloneStart(
        setterName, 
        new Type[] { property.PropertyType });

      this.AppendSetter(property, isStatic);

      this.AppendDefaultStandaloneEnd(setterName);
    }

    private void AppendDefaultStandaloneStart(
      string funcName,
      Type[] referencedTypes)
    {
      this.AppendEdgeFuncStart(funcName);

      this.AppendReferenceFor(
        typeof(ReferenceManager),
        "./node_modules/edge-reference/bin/");

      this.AppendReferenceFor(this.source);

      // Return and parameter type references
      foreach (Type referencedType in referencedTypes) 
      {
        this.AppendReferenceFor(referencedType);
      }

      this.buffer.AppendLine();

      // usings
      this.AppendUsingFor(typeof(System.Threading.Tasks.Task));
      this.AppendUsingFor(typeof(EdgeReference.ReferenceManager));

      this.buffer.AppendLine();

      // class start
      this.AppendClassStart("Startup");
    }

    private void AppendDefaultStandaloneEnd(string methodName)
    {
      this.AppendClassEnd();
      this.AppendEdgeFuncEnd(methodName);
    }

    private void AppendGetterPropertyRetrieval(
      string propertyName,
      MethodInfo getter)
    {
      const string GetTemplate = "{0}{1} _result = {2}.{3};";      

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        GetTemplate,
        this.CurrentIndent,
        getter.ReturnType.FullName,
        getter.IsStatic ? this.source.FullName : "_parent",
        propertyName);

      this.buffer.AppendLine();
    }

    public void AppendSetter(PropertyInfo info, bool isStatic)
    {
      MethodInfo setter = info.GetSetMethod();
      if (setter == null) {
        return;
      }

      const string SignatureTemplate = 
        "{0}public async Task Set_{1}(dynamic parameters)";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        info.Name);

      const string setLineTemplate = "{0}.{1} = parameters.value;";
      string setLine = string.Format(
        CultureInfo.InvariantCulture,
        setLineTemplate,
        setter.IsStatic ? this.source.FullName : "_parent",
        info.Name);

      this.AppendFunctionBody(setter, setLine, false);
    }

    private void AppendParameterConversion(
      ParameterInfo parameter,
      string parameterReference)
    {
      if (ReflectionUtils.IsReferenceType(parameter.ParameterType)) {
        this.buffer.AppendLine(
          GenerateArgumentConversion(
            parameter.Name,
            parameter.Name));
      }
    }

    #endregion

    #region Functions

    public void AppendStandaloneFunctions(MethodInfo[] methods)
    {
      for (var i=0; i<methods.Length; i++) {
        this.AppendStandaloneFunction(methods[i]);
      }
    }

    private void AppendStandaloneFunction(MethodInfo method)
    {
      Type[] referenceTypes = method
        .GetParameters()
        .Select(param => param.ParameterType)
        .Concat(new Type[] { method.ReturnType })
        .ToArray();

      this.AppendDefaultStandaloneStart(method.Name, referenceTypes);
      this.AppendFunction(method);
      this.AppendDefaultStandaloneEnd(method.Name);
    }

    public void AppendFunction(MethodInfo info)
    {
      ParameterInfo[] parameters = info.GetParameters();
      this.buffer.Append(GenerateFunctionSignature(info));
      string callLineTemplate = this.GenerateCallLine(info);
      string callArguments = this.GenerateCallArgumentsList(parameters, info.IsStatic);

      string callLine = string.Format(
        CultureInfo.InvariantCulture,
        callLineTemplate,
        callArguments);

      AppendFunctionBody(info, callLine, true);
    }

    private void AppendFunctionBody(
      MethodInfo info,
      string callLine,
      bool isAsync)
    {
      ParameterInfo[] parameters = info.GetParameters();

      this.buffer.AppendLine();
      this.BlockStart();

      if (isAsync) {
        this.AppendTaskStart(info.ReturnType);
      }

      if (!info.IsStatic) 
      {
        this.AppendReferenceIdConversion("parameters._referenceId");
        this.AppendParentReferenceFetch("_refId");
      }

      bool addLine = false;

      foreach (ParameterInfo parameter in parameters)
      {
        addLine |= this.AppendArgumentConversion(parameter);
      }

      if (addLine) {
        this.buffer.AppendLine();
      }

      this.buffer.Append(this.CurrentIndent);
      this.buffer.AppendLine(callLine);

      // Check for void returns
      this.AppendConvertAndReturn(info.ReturnType);

      if (isAsync) {
        this.AppendTaskEnd();
      }

      this.BlockEnd();
    }

    private string GenerateFunctionSignature(MethodInfo info)
    {
      const string SignatureTemplate = 
        "{0}public async Task{1} {2}(dynamic parameters)";

      string result = string.Format(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        (info.ReturnType == typeof(void)) ? string.Empty : "<object>",
        info.Name);

      return result;
    }

    private string GenerateSignatureArgumentsList(ParameterInfo[] parameters)
    {
      if (parameters.Length == 0) {
        return String.Empty;
      }

      return "dynamic parameters";
    }

    private string GenerateCallArgumentsList(
      ParameterInfo[] parameters,
      bool isStatic)
    {
      string result = String.Empty;

      if (!isStatic) {
        result = "parameters._referenceId";

        if (parameters.Length > 0) {
          result += ", ";
        }
      }

      result = string.Join(
        ", ",
        parameters.Select(info => string.Concat("parameters.", info.Name)));

      return result;
    }

    private bool AppendArgumentConversion(ParameterInfo info)
    {
      if (!ReflectionUtils.IsReferenceType(info.ParameterType)) {
        return false;
      }

      string conversion = GenerateArgumentConversion(
        info.Name,
        info.Name);

      this.buffer.AppendLine(conversion);

      return true;
    }

    private string GenerateArgumentConversion(
      string convertedName,
      string sourceName)
    {
      const string ConversionTemplate = 
        "{0}parameters.{1} = {2};";

      string retrieval = GenerateReferenceRetrieval("parameters." + sourceName);
      
      return string.Format(
        CultureInfo.InvariantCulture,
        ConversionTemplate,
        this.CurrentIndent,
        convertedName,
        retrieval);
    }

    private string GenerateCallLine(MethodInfo info)
    {
      const string ReturnCallTemplate = 
        "{0} _result = {1}.{2}({{0}});";

      const string VoidCallTemplate = "{0}.{1}({{0}});";

      if (info.ReturnType == typeof(void)) {
        return string.Format (
          CultureInfo.InvariantCulture,
          VoidCallTemplate,
          info.IsStatic ? this.source.FullName : "_parent",
          info.Name);
      }
      else
      {
        return string.Format(
          CultureInfo.InvariantCulture,
          ReturnCallTemplate,
          info.ReturnType.FullName,
          info.IsStatic ? this.source.FullName : "_parent",
          info.Name);
      }
    }

    #endregion

    private string JavaScriptFriendlyType(Type type)
    {
      if (ReflectionUtils.IsReferenceType(type)) {
        return "long";
      } else {
        return type.FullName;
      }
    }

    private string GenerateReferenceRetrieval(string referenceArgumentName)
    {
      const string RetrievalTemplate = 
        "ReferenceManager.Instance.PullReference({0})";

      return string.Format(
        CultureInfo.InvariantCulture,
        RetrievalTemplate,
        referenceArgumentName);
    }
  
    private void AppendConvertAndReturn(Type returnType)
    {
      if (returnType == typeof(void)) {
        return;
      }

      if (ReflectionUtils.IsReferenceType(returnType)) {
        this.buffer.AppendFormat(
          CultureInfo.InvariantCulture,
          "{0}return ReferenceManager.Instance.EnsureReference(_result);{1}",
          this.CurrentIndent,
          Environment.NewLine);
      } 
      else 
      {
        this.buffer.AppendFormat(
          CultureInfo.InvariantCulture,
          "{0}return _result;{1}",
          this.CurrentIndent,
          Environment.NewLine);
      }
    }

    private void AppendTaskStart(Type returnType) 
    {
      this.buffer.Append(this.CurrentIndent);
      if (returnType == typeof(void)) {
        this.buffer.AppendLine("await Task.Factory.StartNew(() => {");
      }
      else
      {
        this.buffer.AppendLine(
          "return await Task<object>.Factory.StartNew(() => {");
      }

      this.Indent();
    }

    private void AppendTaskEnd()
    {
      this.Outdent();
      this.buffer.Append(this.CurrentIndent);
      this.buffer.AppendLine("});");
    }

    private void AppendReferenceIdConversion(string referenceIdName)
    {
      // This weird conversion happens because the value may be passed as 
      // int OR long
      const string ConversionTemplate = 
        "{0}long _refId = {1} is long ? (long){1} : (long)(int){1};{2}{2}";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ConversionTemplate,
        this.CurrentIndent,
        referenceIdName,
        Environment.NewLine);
    }
  }
}

