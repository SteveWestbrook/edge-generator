using System;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

namespace EdgeReference
{
  public class ReferenceManager : MarshalByRefObject
  {
    public Dictionary<long, object> referencesById { get; private set; }

    public Dictionary<object, long> idsByReference { get; private set; }

    public Dictionary<long, long> referenceCount { get; private set; }

    private static ReferenceManager instance;

    private static object instanceLock = new object();

    private long nextTemplateId = 0;

    protected ReferenceManager()
    {
      this.referencesById = new Dictionary<long, object>();
      this.idsByReference = new Dictionary<object, long>();
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

    // TODO: All dictionary modifications should be thread-safe

    public long EnsureReference(object reference)
    {
      if (reference == null) {
        return 0;
      }

      long id;

      if (!this.idsByReference.TryGetValue(reference, out id)) {
        // TODO: The id here can eventually get exhausted.  Should either use a guid or reuse ids
        id = Interlocked.Increment(ref this.nextTemplateId);
        this.referencesById.Add(id, reference);
        this.idsByReference.Add(reference, id);
        this.referenceCount.Add(id, 1);
      } else {
        this.referenceCount[id]++;                
      }

      return id;
    }

    public object PullReference(long id)
    {
      object reference;
      if (this.referencesById.TryGetValue(id, out reference)) {
        return reference;
      }

      return null;
    }

    public bool RemoveReference(long id)
    {
      object reference;

      if (!this.referencesById.TryGetValue(id, out reference)) {
        return false;
      }

      long references = --this.referenceCount[id];                

      if (references <= 0) {
        this.referencesById.Remove(id);
        this.idsByReference.Remove(reference);
        this.referenceCount.Remove(id);
      }

      return true;
    }

  }
}

