using System;
using EdgeReference;
using NUnit.Framework;
using DotNetTest;
using System.Reflection;

namespace EdgeReference.Test
{
  [TestFixture]
  public class JavaScriptEmitterTest
  {
    public JavaScriptEmitterTest()
    {
    }

    [Test]
    public void AppendBasicRequiresTest() {
      // TODO

    }

    [Test]
    public void AppendFunctionWithComplexResultAndComplexArgumentTest() {
      JavaScriptEmitter emitter = new JavaScriptEmitter ();
      Type t = typeof(TestType1);
      MethodInfo info = t.GetMethod("CreateNewT2");

      emitter.AppendFunction(info, false);

      string output = emitter.ToString();
      string expected = 
        "CreateNewT2(template, description, callback) {" + 
        Environment.NewLine + 
        "    template = template ? template._edgeId : 0;" + 
        Environment.NewLine + 
        Environment.NewLine + 
        "    var result = Reference.CreateNewT2({" +
        Environment.NewLine + 
        "        _referenceId: _referenceId," + 
        Environment.NewLine + 
        "        template: template," +
        Environment.NewLine + 
        "        description: description" + 
        Environment.NewLine + 
        "    }, callback);" + 
        Environment.NewLine + 
        "    return new TestType2(result);" +
        Environment.NewLine + 
        "}";

      Assert.AreEqual(expected, output);
    }
  }
}

