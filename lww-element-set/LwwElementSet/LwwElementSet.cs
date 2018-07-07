using System;
using COLG = System.Collections.Generic;

namespace LwwElementSet
{
  /// <summary>
  /// An implementation of a LWW-Element-Set (Last-Write-Wins) 
  /// CRDT (conflict-free replicated data type).
  /// </summary>
  public class LwwElementSet<T>
  {
    private COLG.IDictionary<T, long> m_addSet;
    private COLG.IDictionary<T, long> m_removeSet;
    private bool m_biasAdd;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public LwwElementSet()
    {
      m_addSet = new COLG.Dictionary<T, long>();
      m_removeSet = new COLG.Dictionary<T, long>();
      m_biasAdd = true;
    }

    #region Public Methods

    /// <summary>
    /// Adds an element to the set.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <param name="timestamp">The number of ticks representing the time of 
    /// this operation.</param>
    public void Add(
      T element,
      long timestamp)
    {
      m_addSet.TryGetValue(element, out long prevTimestamp);
      m_addSet[element] = Math.Max(prevTimestamp, timestamp);
    }

    /// <summary>
    /// Removes an element from the set.
    /// </summary>
    /// <param name="element">The element to remove.</param>
    /// <param name="timestamp">The number of ticks representing the time of 
    /// this operation.</param>
    public void Remove(
      T element,
      long timestamp)
    {
      m_removeSet.TryGetValue(element, out long prevTimestamp);
      m_removeSet[element] = Math.Max(prevTimestamp, timestamp);
    }

    /// <summary>
    /// Looks up an element in the set.
    /// </summary>
    /// <param name="element">The element to lookup.</param>
    /// <returns>True if the element is in the set; false if not.</returns>
    public bool Lookup(T element)
    {
      // If the element is in the add set, check the remove set.
      if (m_addSet.TryGetValue(element, out long addTimestamp))
      {
        // If the element is in the remove set, compare their timestamps.
        if (m_removeSet.TryGetValue(element, out long removeTimestamp))
        {
          return IsInReplica(addTimestamp, removeTimestamp);
        }
        else
        {
          // Element is in add set, but not in remove set.
          return true;
        }
      }

      // Element is not in the add set, which means it was never added
      // to this replica.
      return false;
    }

    /// <summary>
    /// Determines if this replica doesn't contain any elements.
    /// </summary>
    /// <returns>True if this replica is empty; false if not.</returns>
    public bool IsEmpty()
    {
      // Go through every element in the add set and check if it is also 
      // in the remove set.
      foreach (COLG.KeyValuePair<T, long> kvp in m_addSet)
      {
        T element = kvp.Key;
        long addTimestamp = kvp.Value;

        if (m_removeSet.TryGetValue(element, out long removeTimestamp))
        {
          if (IsInReplica(addTimestamp, removeTimestamp))
          {
            return false;
          }
        }
        else
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Sets the bias for equal timestamps towards add.
    /// </summary>
    public void SetBiasAdd()
    {
      m_biasAdd = true;
    }

    /// <summary>
    /// Sets the bias for equal timestamps towards remove.
    /// </summary>
    public void SetBiasRemove()
    {
      m_biasAdd = false;
    }

    /// <summary>
    /// Returns the merged result of all the <paramref name="replicas"/>.
    /// </summary>
    /// <param name="biasAdd">True to have equal timestamps bias towards add; 
    /// false to have equal timestamps bias towards remove.</param>
    /// <param name="replicas">The list of replicas to merge.</param>
    /// <returns>The merged replica of all the <paramref name="replicas"/>.
    /// </returns>
    static public LwwElementSet<T> Merge(
      bool biasAdd,
      params LwwElementSet<T>[] replicas)
    {
      LwwElementSet<T> mergedReplica = new LwwElementSet<T>();
      mergedReplica.m_biasAdd = biasAdd;

      // Collect all the unique elements from all the replicas.
      COLG.HashSet<T> allElements = new COLG.HashSet<T>();

      for (int i = 0; i < replicas.Length; i++)
      {
        allElements.UnionWith(replicas[i].m_addSet.Keys);
        allElements.UnionWith(replicas[i].m_removeSet.Keys);
      }

      // Go through all the unique elements and get the latest timestamp for 
      // its add and/or remove operation.
      foreach (T element in allElements)
      {
        long maxAddTimestamp = DateTime.MinValue.Ticks;
        long maxRemoveTimestamp = DateTime.MinValue.Ticks;

        for (int i = 0; i < replicas.Length; i++)
        {
          if (replicas[i].m_addSet.TryGetValue(
            element, out long addTimestamp))
          {
            maxAddTimestamp = Math.Max(maxAddTimestamp, addTimestamp);
          }

          if (replicas[i].m_removeSet.TryGetValue(
            element, out long removeTimestamp))
          {
            maxRemoveTimestamp = Math.Max(maxRemoveTimestamp, removeTimestamp);
          }
        }

        if (maxAddTimestamp != DateTime.MinValue.Ticks)
        {
          mergedReplica.m_addSet[element] = maxAddTimestamp;
        }

        if (maxRemoveTimestamp != DateTime.MinValue.Ticks)
        {
          mergedReplica.m_removeSet[element] = maxRemoveTimestamp;
        }
      }

      return mergedReplica;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Compares the given timestamps to determine if the element exists 
    /// in the replica or not.
    /// </summary>
    /// <param name="addTimestamp">The timestamp of the add operation.</param>
    /// <param name="removeTimestamp">The timestamp of the remove operation.</param>
    /// <returns>True if <paramref name="addTimestamp"/> is greater than 
    /// <paramref name="removeTimestamp"/>. False if it is less than. If they 
    /// are equal, then it is based on the bias.</returns>
    private bool IsInReplica(
      long addTimestamp,
      long removeTimestamp)
    {
      if (addTimestamp > removeTimestamp)
      {
        return true;
      }
      else if (removeTimestamp > addTimestamp)
      {
        return false;
      }
      else
      {
        // The element has the same add and remove timestamp.
        return m_biasAdd;
      }
    }

    #endregion  
  }
}
