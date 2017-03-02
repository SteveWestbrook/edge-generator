/**
 * ProxyGenerator class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Core;

namespace EdgeReference
{
  public class ProxyGenerator
  {
    private static ConcurrentDictionary<string, string> generatedProxies = new ConcurrentDictionary<string, string>();

    private Dictionary<string, List<MemberInfo>> generatedMembers;

    private Action<string, string> classGenerated;

    private JavaScriptEmitter emitter;

    private DotNetEmitter dotNetEmitter;

    protected ProxyGenerator()
    {
    }

    /// <summary>
    /// Generates a contract.
    /// </summary>
    /// <param name="typeNameWithNamespace">Type name with namespace.</param>
    /// <param name="assemblyPath">Assembly path.</param>
    public static void Generate(
      string typeNameWithNamespace,
      string assemblyPath,
      Action<string, string> classGeneratedCallback)
    {

      try 
      {
        if (generatedProxies.ContainsKey(typeNameWithNamespace)) {
          return;
        }

        generatedProxies.TryAdd(typeNameWithNamespace, null);

        ProxyGenerator generator = new ProxyGenerator();
        Assembly owningAssembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
        Type type = owningAssembly.GetType(typeNameWithNamespace);

        generator.classGenerated = classGeneratedCallback;
        generator.Generate(type);
      } catch (Exception ex) {
        Console.WriteLine("Error during generation: ");
        Console.WriteLine(ex.ToString());
        throw;
      }
    }

    private void Generate(Type target)
    {
      this.emitter = new JavaScriptEmitter();
      this.dotNetEmitter = new DotNetEmitter(target, this.emitter.Buffer);

      ConstructorInfo[] constructors = target.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

      PropertyInfo[] staticProperties = RetrieveProperties(target, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

      PropertyInfo[] instanceProperties = RetrieveProperties(target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

      MethodInfo[] staticMethods = this.RetrieveMethods(
          target,
          BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(info => !info.IsSpecialName)
        .ToArray();

      MethodInfo[] instanceMethods = this.RetrieveMethods(
        target,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(info => !info.IsSpecialName)
        .ToArray();

      this.emitter.AppendBasicRequires(target);

      // Get all non-value, non-string property types.
      IEnumerable<Type> requireProperties = 
        staticProperties.Concat(instanceProperties)
        .Distinct()
        .Where(info => ReflectionUtils.IsReferenceType(info.PropertyType))
        .Where(info => info.PropertyType.FullName != target.FullName)
        .Select((info) => {
            return info.PropertyType;
        });

      // Get all non-value, non-string types used by methods.
      IEnumerable<Type> requireMethods = 
        staticMethods.Concat(instanceMethods)
        .Distinct()
        .SelectMany((info) => {
          IEnumerable<Type> result = info.GetParameters()

          .Where((param) => {
            return ReflectionUtils.IsReferenceType(param.ParameterType);
          })
          .Select((param) => {
            return param.ParameterType;
          });

            if (ReflectionUtils.IsReferenceType(info.ReturnType)) {
            result = result.Concat(new Type[] { info.ReturnType });
          }

          return result;
        });

      requireMethods = requireMethods.Where(type => type.FullName != target.FullName);

      this.emitter.AppendRequires(requireProperties);
      this.emitter.AppendRequires(requireMethods);
      this.emitter.AppendLine();

      // Proxies to dot net objects.
      this.dotNetEmitter.AppendStandaloneConstructors(constructors);
      this.dotNetEmitter.AppendStandaloneProperties(staticProperties, true);
      this.dotNetEmitter.AppendStandaloneProperties(instanceProperties, false);
      this.dotNetEmitter.AppendStandaloneFunctions(staticMethods);
      this.dotNetEmitter.AppendStandaloneFunctions(instanceMethods);

      this.emitter.AppendClassDefinition(target);

      foreach (ConstructorInfo info in constructors) {
        this.emitter.AppendConstructor(info);
        this.emitter.AppendBreak();
      }

      // TODO: Constructors - call super, pass params  
      foreach (PropertyInfo info in staticProperties) { 
        this.emitter.AppendProperty(info, true);
        this.emitter.AppendBreak();
      };

      foreach (PropertyInfo info in instanceProperties) {
        this.emitter.AppendProperty(info, false);
        this.emitter.AppendBreak();
      };
                  
      foreach (MethodInfo info in staticMethods) {
        this.emitter.AppendFunction(info, true);
        this.emitter.AppendBreak();
      };
                  
      foreach (MethodInfo info in instanceMethods) {
        this.emitter.AppendFunction(info, false);
        this.emitter.AppendBreak();
      };

      this.emitter.AppendClassTermination();
      this.emitter.AppendExport(target);

      this.OnClassGenerated(target, this.emitter.ToString());

      // After generation, look at base classes
      if (target.BaseType != typeof(object)) {
        this.GenerateProxy(target.BaseType);
      }

      this.GenerateProxiesForProperties(staticProperties);
      this.GenerateProxiesForProperties(instanceProperties);
      this.GenerateProxiesForFunctions(staticMethods);
      this.GenerateProxiesForFunctions(instanceMethods);
    }

    private void GenerateProxiesForProperties(PropertyInfo[] properties)
    {
      for (var i=0; i<properties.Length; i++) {
        this.GenerateProxy(properties[i].PropertyType);
      }
    }

    private void GenerateProxiesForFunctions(MethodInfo[] methods)
    {
      for (var i=0; i<methods.Length; i++) {
        this.GenerateProxy(methods[i].ReturnType);

        ParameterInfo[] parameters = methods[i].GetParameters();
        for (var j=0; j<parameters.Length; j++) {
          GenerateProxy(parameters[j].ParameterType);
        }
      }
    }

    private void GenerateProxy(Type type)
    {
      if (!ReflectionUtils.IsReferenceType(type)) {
        return;
      }

      ProxyGenerator.Generate(
        type.FullName,
        type.Assembly.Location,
        this.classGenerated);
    }

    private PropertyInfo[] RetrieveProperties(Type target, BindingFlags flags)
    {
      PropertyInfo[] result = target.GetProperties(flags);

      // Alphabetical order
      result = result.OrderBy((member) => member.Name).ToArray();
      return result;
    }

    private MethodInfo[] RetrieveMethods(Type target, BindingFlags flags)
    {
      MethodInfo[] result = target.GetMethods(flags);

      // Alphabetical order
      result = result.OrderBy((method) => method.Name).ToArray();
      return result;
    }

    private void OnClassGenerated(Type classType, string generatedJavaScript)
    {
      string name = classType.FullName;

      generatedProxies.AddOrUpdate(name, generatedJavaScript, (a,b) => generatedJavaScript);

      if (this.classGenerated != null) {
        this.classGenerated(name, generatedJavaScript);
      }
    }
  }
}


