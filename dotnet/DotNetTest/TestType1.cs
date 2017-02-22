using System;

namespace DotNetTest
{
	public class TestType1
	{
		private static int innerCount = 0;

		public TestType1()
		{
		}

		public static string SharedData { get; set; }

		public string Name { get; set; }

		public TestType2 Child { get; private set; }

		public TestType1 Sibling { get; set; }

		public void AssignChild(TestType2 newValue) 
		{
			this.Child = newValue;
		}

		public TestType2 CreateNewT2(TestType2 template, string description)
		{
			TestType2 result = new TestType2 ();
			result.Description = description;
			if (template != null) {
				result.Status = template.Status;
			}

			return result;
		}

		public static TestType2 CreateT2StaticTemplate(TestType2 template)
		{
			TestType2 result = new TestType2();
			result.Status = template.Status;
			return result;
		}

		public static TestType2 CreateT2Static()
		{
			return new TestType2();
		}

		public static void UpdateName(TestType1 target, string name)
		{
			target.Name = name;			
		}

		public static void IncreaseCount()
		{
			innerCount++;
		}

		public void AssignT2Description(TestType2 target, string description)
		{
			target.Description = description;
		}

		public void AssignName(string name)
		{
			this.Name = name;
		}

		public void DuplicateName()
		{
			this.Name = string.Concat(this.Name, this.Name);
		}

		public TestType1 ReturnSelf()
		{
			return this;
		}

		public TestType2 ReturnTarget(TestType2 target)
		{
			return target;
		}
	}
}

