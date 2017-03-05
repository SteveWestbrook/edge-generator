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

    /// <summary>
    /// This contains the highest value currently used as an ID for a reference.
    /// </summary>
    private long templateId = 0;

    /// <summary>
    /// Protected constructor for ReferenceManager.
    /// </summary>
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

    /// <summary>
    /// The number of references contained in this instance.
    /// </summary>
    public long Count
    {
      get
      {
        return this.referencesById.Count;
      }
    }

    /// <summary>
    /// Tracks a reference of an object.  This should be called once for each 
    /// proxy created to track the object defined by reference.
    /// </summary>
    /// <param name="reference">
    /// The object to track.
    /// </param>
    /// <remarks>
    /// Internally, this uses reference counting as a means to determine when 
    /// a particular object is no longer in use.
    /// </remarks>
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
          // Specifically, if a program were to create a billion references 
          // per second for 292 years, identifier space would be exhausted.
          // You have been warned.
          id = Interlocked.Increment(ref this.templateId);
          this.referencesById.Add(id, reference);
          this.idsByReference.Add(reference, id);
          this.referenceCount.Add(id, 1);
        } else {
          this.referenceCount[id]++;                
        }
      }

      return id;
    }

    /// <summary>
    /// Retrieves an object reference matching the supplied ID from the 
    /// internal collection and returns it.
    /// </summary>
    /// <param name="id">
    /// The ID of the desired reference.
    /// </param>
    /// <returns>
    /// The reference corresponding to the specified ID, or null if no 
    /// match is found.
    /// </returns>
    public object PullReference(long id)
    {
      object reference;

      lock (this.accessLock)
      {
        this.referencesById.TryGetValue(id, out reference);
      }

      return reference;
    }

    /// <summary>
    /// Removes an object reference matching the supplied ID from the internal 
    /// collection.
    /// </summary>
    /// <param name="id">
    /// The ID of the reference to be removed.
    /// </param>
    /// <returns>
    /// If a reference is found and removed, returns true.  If the ID does not 
    /// match a reference, returns false.
    /// </returns>
    public bool RemoveReference(long id)
    {
      object reference;

      lock (this.accessLock)
      {
        // If no match is found, stop
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

