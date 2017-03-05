/**
 * ReferenceManager class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace EdgeReference
{
  /// <summary>
  /// Controls the lifecycle of references by holding on to them as long as 
  /// they are not explicitly disposed.
  /// </summary>
  public class ReferenceManager : MarshalByRefObject
  {
    /// <summary>
    /// All living references, corresponding to a particular ID.
    /// </summary>
    public Dictionary<long, object> referencesById { get; private set; }

    /// <summary>
    /// Contains a count of object IDs, keyed by the object itself.
    /// </summary>
    public Dictionary<object, long> idsByReference { get; private set; }

    /// <summary>
    /// Contains a count of referenced objects by their ID.  Once this count 
    /// reaches 0, the object can be reclaimed.
    /// </summary>
    public Dictionary<long, long> referenceCount { get; private set; }

    /// <summary>
    /// The single publically-accessible ReferenceManager instance.
    /// </summary>
    private static ReferenceManager instance;

    /// <summary>
    /// Synchronizes access to the single EdgeReference instance.
    /// </summary>
    private static object instanceLock = new object();

    /// <summary>
    /// Synchronizes access to the data structures that control memory life 
    /// cycle in .NET.
    /// </summary>
    private object accessLock = new object();

    private long nextTemplateId = 0;

    protected ReferenceManager()
    {
      this.referencesById = new Dictionary<long, object>();
      this.idsByReference = new Dictionary<object, long>(new ReferenceComparer<object>());
      this.referenceCount = new Dictionary<long, long>();
    }

    /// <summary>
    /// Gets or sets the single instance of wrapper in this AppDomain.
    /// </summary>
    /// <value>The single instance of wrapper in this AppDomain.</value>
    public static ReferenceManager Instance {
      get 
      {
        if (instance == null) 
        {
          lock (instanceLock) 
          {
            if (instance == null) 
            {
              instance = new ReferenceManager();
            }
          }
        }

        return instance;
      }

      set 
      {
        instance = value;
      }
    }

    /// <summary>
    /// Initializes the lifetime service.
    /// </summary>
    /// <returns>The lifetime service.</returns>
    public override object InitializeLifetimeService()
    {
      return null;
    }

    public long Count
    {
      get
      {
        return this.referencesById.Count;
      }
    }

    public long EnsureReference(object reference)
    {
      if (reference == null) {
        return 0;
      }

      // TODO: Reference types are fine, but value types - possibly with methods 
      // - are always copied.  Look into the implications of this.

      long id;

      lock (this.accessLock)
      {
        if (!this.idsByReference.TryGetValue(reference, out id)) {
          // NOTE: The id here can eventually get exhausted.  If your 
          // application creates more than 8 * 10^18 object instances, you 
          // may run out of IDs.  There is a performance hit for dealing 
          // with this, so for the present time it is recommended that you 
          // not use this for extremely busy, long-running programs.
          id = Interlocked.Increment(ref this.nextTemplateId);
          this.referencesById.Add(id, reference);
          this.idsByReference.Add(reference, id);
          this.referenceCount.Add(id, 1);
        } else {
          this.referenceCount[id]++;                
        }
      }

      return id;
    }

    public object PullReference(long id)
    {
      object reference;

      lock (this.accessLock)
      {
        this.referencesById.TryGetValue(id, out reference);
      }

      return reference;
    }

    public bool RemoveReference(long id)
    {
      object reference;

      lock (this.accessLock)
      {
        if (!this.referencesById.TryGetValue(id, out reference)) {
          return false;
        }

        long references = --this.referenceCount[id];                

        if (references <= 0) {
          this.referencesById.Remove(id);
          this.idsByReference.Remove(reference);
          this.referenceCount.Remove(id);
        }
      }
      
      return true;
    }

  }
}

