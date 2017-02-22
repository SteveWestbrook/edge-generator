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

			Assert.AreEqual (
				"CreateNewT2(template, description) {\n    template = template ? template._edgeId : 0;\n\n    var result = Reference.CreateNewT2(this._referenceId, template, description);\n    return new TestType2(result);\n}",
				output);
		}
	}
}

