using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace EdgeReference
{
  public class DotNetEmitter : CodeEmitter
  {
    private Type source;

    private Dictionary<string, bool> referencedTypes;

    private Dictionary<string, bool> referencedNamespaces;

    public DotNetEmitter(Type source, StringBuilder existingBuffer = null)
    {
      this.referencedTypes = new Dictionary<string, bool>();
      this.source = source;

      // Can optionally supply a buffer that has already been initialized.
      if (existingBuffer != null) {
        this.buffer = existingBuffer;
      }
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
      if (this.referencedTypes.ContainsKey(belongsTo)) {
        return;
      }

      this.referencedTypes.Add(belongsTo, true);

      // Determine file name of assembly.
      string assemblyFileName = type.Assembly.Location;
      assemblyFileName = Path.GetFileName(assemblyFileName);

      if (deploymentDirectory != null) {
        assemblyFileName = Path.Combine(deploymentDirectory, assemblyFileName);
      }

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "#r \"{0}\";",
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

    public void AppendNamespaceEnd() 
    {
      this.Outdent();
      this.buffer.AppendLine("}");
    }

    #endregion

    #region Class

    public void AppendClassStart() 
    {
      const string ClassHeaderTemplate = "{0}public static class {1}Proxy";
      
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        ClassHeaderTemplate,
        this.CurrentIndent,
        this.source.Name);

      this.buffer.AppendLine();

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}{{",
        this.CurrentIndent);

      this.buffer.AppendLine();

      this.Indent();
    }

    public void AppendClassEnd()
    {
      this.Outdent();
      
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}}}",
        this.CurrentIndent);

      this.buffer.AppendLine();
    }

    #endregion

    #region Properties
    
    public void AppendProperty(PropertyInfo info, bool isStatic)
    {
      AppendGetter(info, isStatic);
      AppendSetter(info, isStatic);
    }

    public void AppendGetter(PropertyInfo info, bool isStatic)
    {
      MethodInfo getter = info.GetGetMethod();

      if (getter == null) {
        return;
      }

      const string SignatureTemplate = 
        "{0}public static async Task<object> Get_{1}({2})";

      string instanceReference = isStatic ? string.Empty : "object _referenceId";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        info.Name,
        instanceReference);

      this.buffer.AppendLine();

      this.buffer.AppendLine("{");

      this.Indent();

      this.AppendTaskStart(info.PropertyType);

      if (!isStatic) {
        this.AppendParentReferenceFetch("_referenceId");
      }

      this.AppendGetterPropertyRetrieval(info.Name, getter);

      this.AppendConvertAndReturn(info.PropertyType);

      this.AppendTaskEnd();

      this.Outdent();
      this.buffer.AppendLine("}");
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
        "{0}public static async Task Set_{1}(dynamic parameters){2}";

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
        info.Name,
        Environment.NewLine);

      const string setLineTemplate = "_parent.{0} = parameters.value;";
      string setLine = string.Format(
        CultureInfo.InvariantCulture,
        setLineTemplate,
        info.Name);

      this.AppendFunctionBody(setter, setLine);
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

      AppendFunctionBody(info, callLine);
    }

    private void AppendFunctionBody(MethodInfo info, string callLine)
    {
      ParameterInfo[] parameters = info.GetParameters();

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}{",
        this.CurrentIndent);

      this.buffer.AppendLine();

      this.Indent();

      this.AppendTaskStart(info.ReturnType);

      if (!info.IsStatic) 
      {
        this.buffer.AppendLine(
            GenerateArgumentConversion(
              "_parent",
              "_referenceId"));
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
      this.buffer.Append(callLine);

      // TODO: Check for void returns
      this.AppendConvertAndReturn(info.ReturnType);

      this.AppendTaskEnd();

      this.Outdent();

      this.buffer.Append(this.CurrentIndent);
      this.buffer.AppendLine("}");
    
    }

    private string GenerateFunctionSignature(MethodInfo info)
    {
      const string SignatureTemplate = 
        "{0}public static async Task<object> {1}({{0}})";

      string result = string.Format(
        CultureInfo.InvariantCulture,
        SignatureTemplate,
        this.CurrentIndent,
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

      string retrieval = GenerateReferenceRetrieval(sourceName);
      
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
        "{0} _result = _parent.{1}({{0}});";

      const string VoidCallTemplate = "_parent.{1}({{0}});";

      if (info.ReturnType == typeof(void)) {
        return string.Format (
          CultureInfo.InvariantCulture,
          VoidCallTemplate,
          info.Name);
      }
      else
      {
        return string.Format(
          CultureInfo.InvariantCulture,
          ReturnCallTemplate,
          info.ReturnType.FullName,
          info.Name);
      }
    }

    #endregion

    private string JavaScriptFriendlyType(Type type)
    {
      if (ReflectionUtils.IsReferenceType(type)) {
        return "int";
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
          "{0}return ReferenceManager.EnsureReference(_result);{1}",
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

      if (returnType != typeof(void)) {
        this.buffer.Append("return ");
      }

      this.buffer.AppendLine("TaskFactory.StartNew(() => {");
      this.Indent();
    }

    private void AppendTaskEnd()
    {
      this.Outdent();
      this.buffer.Append(this.CurrentIndent);
      this.buffer.AppendLine("});");
    }
  }
}

