/**
 * ReflectionUtils class
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

namespace EdgeGenerator
{
	/// <summary>
	/// Contains tools that describe .NET code metadata.
	/// </summary>
	public class ReflectionUtils
	{
    public static bool IsReferenceType(Type type) {
      return !type.IsValueType && type != typeof(string);
    }

    public static string ConvertFullName(string fullName) {
      return fullName.Replace('.', '-');
    }
  }
}

