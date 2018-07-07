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
    private COLG.IDictionary<T, DateTime> m_addSet;
    private COLG.IDictionary<T, DateTime> m_removeSet;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public LwwElementSet()
    {
      m_addSet = new COLG.Dictionary<T, DateTime>();
      m_removeSet = new COLG.Dictionary<T, DateTime>();
    }

    /// <summary>
    /// Adds an element to the set.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <param name="timestamp">The time of this operation.</param>
    public void Add(
      T element,
      DateTime timestamp)
    {

    }

    /// <summary>
    /// Removes an element from the set.
    /// </summary>
    /// <param name="element">The element to remove.</param>
    /// <param name="timestamp">The time of this operation.</param>
    public void Remove(
      T element,
      DateTime timestamp)
    {

    }

    /// <summary>
    /// Looks up an element in the set.
    /// </summary>
    /// <param name="element">The element to lookup.</param>
    /// <returns>True if the element is in the set; false if not.</returns>
    public bool Lookup(T element)
    {
      return false;
    }

    /// <summary>
    /// Returns the merged result of all the <paramref name="replicas"/>.
    /// </summary>
    /// <param name="replicas">The list of replicas to merge.</param>
    /// <returns>The merged replica of all the <paramref name="replicas"/>.
    /// </returns>
    static public LwwElementSet<T> Merge(params LwwElementSet<T>[] replicas)
    {
      return null;
    }
  }
}
