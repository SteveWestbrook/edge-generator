/**
 * ReferenceComparer class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EdgeReference
{
  /// <summary>
  /// This class compares two objects by reference, and surprisingly does not 
  /// exist in the framework.
  /// </summary>
  public class ReferenceComparer<T> : IEqualityComparer<T>
  {
    public bool Equals(T x, T y)
    {
      return object.ReferenceEquals(x, y);
    }

    public int GetHashCode(T target)
    {
      return RuntimeHelpers.GetHashCode(target);
    }
  }
}
