using System;
using EdgeReference;
using NUnit.Framework;
using DotNetTest;
using System.Reflection;

namespace EdgeGenerator.Test
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
        "    template = template ? template._referenceId : 0;" + 
        Environment.NewLine + 
        Environment.NewLine + 
        "    return EdgeReference.callbackOrReturn(" +
        Environment.NewLine +
        "        CreateNewT2," + Environment.NewLine + 
        "        {" + Environment.NewLine +
        "            _referenceId: this._referenceId," + 
        Environment.NewLine + 
        "            template: template," +
        Environment.NewLine + 
        "            description: description" + 
        Environment.NewLine + 
        "        }," + 
        Environment.NewLine + 
        "        TestType2," + Environment.NewLine + 
        "        callback);" + Environment.NewLine + 
        "}";

      Assert.AreEqual(expected, output);
    }
  }
}

