/**
 * DotNetEmitter test class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using EdgeReference;
using NUnit.Framework;
using DotNetTest;
using System.Reflection;
using EdgeGenerator;

namespace EdgeGenerator.Test
{
	[TestFixture]
	public class DotNetEmitterTest
	{
		public DotNetEmitterTest()
		{
		}

		[Test]
		public void AppendBasicRequiresTest() {
			// TODO

		}

		[Test]
		public void AppendClassStartTest() {
			string expected = 
				"public class TestType1Proxy" +
				Environment.NewLine +
				"{" +
				Environment.NewLine;
			
			DotNetEmitter emitter = new DotNetEmitter(typeof(TestType1));

			emitter.AppendClassStart();
			
			Assert.AreEqual(expected, emitter.ToString());
		}

		[Test]
		public void AppendClassEndTest() {
			string expected = "}" + Environment.NewLine;

			DotNetEmitter emitter = new DotNetEmitter(typeof(TestType1));

			// Make sure the indent will be removed before appending.
			emitter.Indent();
			emitter.AppendClassEnd();

			Assert.AreEqual(expected, emitter.ToString());
		}

		[Test]
		public void AppendInstanceSimpleGetterTest() {
			Type target = typeof(TestType1);
			DotNetEmitter emitter = new DotNetEmitter(target);

			string expected = 
				"public async Task<object> Get_Name(object _referenceId)" +
				Environment.NewLine +
				"{" + Environment.NewLine + 
				"    long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;" +
				Environment.NewLine + Environment.NewLine + 
				"    DotNetTest.TestType1 _parent = " + 
				"(DotNetTest.TestType1)" + 
				"ReferenceManager.Instance.PullReference(_refId);" +
				Environment.NewLine + 
				"    System.String _result = _parent.Name;" +
				Environment.NewLine +
				"    return _result;" + Environment.NewLine +
				"}" + Environment.NewLine;

			PropertyInfo info = target.GetProperty("Name");

			emitter.AppendGetter(info, false);

			Assert.AreEqual(
				expected,
				emitter.ToString());
		}

		[Test]
		public void AppendInstanceComplexGetterTest() {
			Type target = typeof(TestType1);
			DotNetEmitter emitter = new DotNetEmitter(target);

			string expected = 
				"public async Task<object> Get_Child(object _referenceId)" +
				Environment.NewLine +
				"{" + Environment.NewLine + 
				"    long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;" +
				Environment.NewLine + Environment.NewLine + 
				"    DotNetTest.TestType1 _parent = " + 
				"(DotNetTest.TestType1)" + 
				"ReferenceManager.Instance.PullReference(_refId);" +
				Environment.NewLine + 
				"    DotNetTest.TestType2 _result = _parent.Child;" +
				Environment.NewLine +
				"    return ReferenceManager.Instance.EnsureReference(_result);" + 
				Environment.NewLine +
				"}" + Environment.NewLine;

			PropertyInfo info = target.GetProperty("Child");

			emitter.AppendGetter(info, false);

			Assert.AreEqual(
				expected,
				emitter.ToString());
		}

		[Test]
		public void AppendStaticSimpleGetterTest() {
			Type target = typeof(TestType1);
			DotNetEmitter emitter = new DotNetEmitter(target);

			string expected = 
				"public async Task<object> Get_SharedData(object unused)" +
				Environment.NewLine +
				"{" + Environment.NewLine + 
				"    System.String _result = DotNetTest.TestType1.SharedData;" +
				Environment.NewLine +
				"    return _result;" + Environment.NewLine +
				"}" + Environment.NewLine;

			PropertyInfo info = target.GetProperty("SharedData");

			emitter.AppendGetter(info, true);

			Assert.AreEqual(
				expected,
				emitter.ToString());
		}

		[Test]
		public void AppendInstanceSimpleSetterTest() {
			Type target = typeof(TestType1);
			DotNetEmitter emitter = new DotNetEmitter(target);

			string expected = 
				"public async Task Set_Name(dynamic parameters)" +
				Environment.NewLine +
				"{" + Environment.NewLine +
				"    long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;" +
				Environment.NewLine + Environment.NewLine + 
				"    DotNetTest.TestType1 _parent = " + 
				"(DotNetTest.TestType1)" + 
				"ReferenceManager.Instance.PullReference(_refId);" +
				Environment.NewLine + 
				"    _parent.Name = parameters.value;" +
				Environment.NewLine +
				"}" + Environment.NewLine;

			PropertyInfo info = target.GetProperty("Name");

			emitter.AppendSetter(info, false);

			Assert.AreEqual(
				expected,
				emitter.ToString());
		}

		[Test]
		public void AppendInstanceComplexSetterTest() {
			
		}

		[Test]
		public void AppendInstancePrivateSetterTest() {
			
		}

		[Test]
		public void AppendStaticSimpleSetterTest() {
			
		}
	}
}

