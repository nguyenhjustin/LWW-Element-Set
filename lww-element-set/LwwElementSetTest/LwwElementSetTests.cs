using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LwwElementSet
{
  /// <summary>
  /// Tests for the LwwElementSet implementation.
  /// </summary>
  [TestClass]
  public class LwwElementSetTests
  {
    private Random m_random;
    private ReliableTime m_time;

    /// <summary>
    /// A class implementation which guarantees that getting the current time 
    /// is greater than a previous current time call.
    /// </summary>
    class ReliableTime
    {
      private long m_previousTicks;

      /// <summary>
      /// Default constructor.
      /// </summary>
      public ReliableTime()
      {
        m_previousTicks = DateTime.MinValue.Ticks;
      }

      /// <summary>
      /// Gets the current timestamp in ticks, which is guaranteed to be 
      /// greater than any previous calls to this method.
      /// </summary>
      /// <returns>The current timestamp in ticks.</returns>
      public long GetDateTimeNow()
      {
        long nowTicks = DateTime.Now.Ticks;

        if (nowTicks <= m_previousTicks)
        {
          nowTicks = m_previousTicks + 1;
        }

        m_previousTicks = nowTicks;
        return nowTicks;
      }
    }

    /// <summary>
    /// Method to call before each test is ran.
    /// </summary>
    [TestInitialize]
    public void InitializeTest()
    {
      m_random = new Random();
      m_time = new ReliableTime();
    }

    /// <summary>
    /// Tests that a newly created replica is empty.
    /// </summary>
    [TestMethod]
    public void EmptyReplicaTest()
    {
      LwwElementSet<char> replica = new LwwElementSet<char>();
      Assert.IsTrue(replica.IsEmpty());
    }

    /// <summary>
    /// Tests that elements exist in replica after adding them.
    /// </summary>
    [TestMethod]
    public void AddTest()
    {
      LwwElementSet<int> replica = new LwwElementSet<int>();
      int[] elements = { 1, 2 };

      for (int i = 0; i < elements.Length; i++)
      {
        replica.Add(elements[i], m_time.GetDateTimeNow());
      }

      Assert.IsFalse(replica.IsEmpty());

      for (int i = 0; i < elements.Length; i++)
      {
        Assert.IsTrue(replica.Lookup(elements[i]));
      }
    }

    /// <summary>
    /// Tests that elements don't exist in replica after removing them.
    /// </summary>
    [TestMethod]
    public void RemoveTest()
    {
      LwwElementSet<int> replica = new LwwElementSet<int>();
      int[] elements = { 1, 2 };

      for (int i = 0; i < elements.Length; i++)
      {
        replica.Remove(elements[i], m_time.GetDateTimeNow());
      }

      Assert.IsTrue(replica.IsEmpty());

      for (int i = 0; i < elements.Length; i++)
      {
        Assert.IsFalse(replica.Lookup(elements[i]));
      }
    }

    /// <summary>
    /// Tests that elements don't exist in replica after adding them and 
    /// then removing them.
    /// </summary>
    [TestMethod]
    public void AddRemoveTest()
    {
      LwwElementSet<int> replica = new LwwElementSet<int>();
      int[] elements = { 1, 2 };

      for (int i = 0; i < elements.Length; i++)
      {
        replica.Add(elements[i], m_time.GetDateTimeNow());
      }

      for (int i = 0; i < elements.Length; i++)
      {
        replica.Remove(elements[i], m_time.GetDateTimeNow());
      }

      Assert.IsTrue(replica.IsEmpty());

      for (int i = 0; i < elements.Length; i++)
      {
        Assert.IsFalse(replica.Lookup(elements[i]));
      }
    }

    /// <summary>
    /// Tests that an element added and removed with the same timestamp biases 
    /// towards adding it.
    /// </summary>
    [TestMethod]
    public void BiasAddTest()
    {
      int element = 1;
      long time = m_time.GetDateTimeNow();

      LwwElementSet<int> replica = new LwwElementSet<int>();
      replica.Add(element, time);
      replica.Remove(element, time);

      replica.SetBiasAdd();
      Assert.IsFalse(replica.IsEmpty());
      Assert.IsTrue(replica.Lookup(element));
    }

    /// <summary>
    /// Tests that an element added and removed with the same timestamp biases 
    /// towards removing it.
    /// </summary>
    [TestMethod]
    public void BiasRemoveTest()
    {
      int element = 1;
      long time = m_time.GetDateTimeNow();

      LwwElementSet<int> replica = new LwwElementSet<int>();
      replica.Add(element, time);
      replica.Remove(element, time);

      replica.SetBiasRemove();
      Assert.IsTrue(replica.IsEmpty());
      Assert.IsFalse(replica.Lookup(element));
    }

    /// <summary>
    /// Tests that the merged replica contains the added elements from all 
    /// the replicas.
    /// </summary>
    [TestMethod]
    public void MergeReplicasAddTest()
    {
      LwwElementSet<int> replica1 = new LwwElementSet<int>();
      replica1.Add(1, m_time.GetDateTimeNow());

      LwwElementSet<int> replica2 = new LwwElementSet<int>();
      replica2.Add(2, m_time.GetDateTimeNow());

      LwwElementSet<int> mergedReplica = 
        LwwElementSet<int>.Merge(true, replica1, replica2);

      Assert.IsFalse(mergedReplica.IsEmpty());
      Assert.IsTrue(mergedReplica.Lookup(1));
      Assert.IsTrue(mergedReplica.Lookup(2));
    }

    /// <summary>
    /// Tests that the merged replica doesn't contain an element that was added 
    /// and removed from a replica.
    /// </summary>
    [TestMethod]
    public void MergeReplicasAddRemoveTest()
    {
      int element = 1;

      LwwElementSet<int> replica1 = new LwwElementSet<int>();
      replica1.Add(element, m_time.GetDateTimeNow());

      LwwElementSet<int> replica2 = new LwwElementSet<int>();
      replica2.Add(element, m_time.GetDateTimeNow());
      replica2.Remove(element, m_time.GetDateTimeNow());

      LwwElementSet<int> mergedReplica =
        LwwElementSet<int>.Merge(true, replica1, replica2);

      Assert.IsTrue(mergedReplica.IsEmpty());
      Assert.IsFalse(mergedReplica.Lookup(element));
    }

    /// <summary>
    /// Tests that a merged replica with an add and remove of the same element 
    /// with the same timestamp biases towards adding it.
    /// </summary>
    [TestMethod]
    public void MergeReplicasBiasAddTest()
    {
      int element = 1;
      long time = m_time.GetDateTimeNow();

      LwwElementSet<int> replica1 = new LwwElementSet<int>();
      replica1.Add(element, time);

      LwwElementSet<int> replica2 = new LwwElementSet<int>();
      replica2.Add(element, time);
      replica2.Remove(element, time);

      LwwElementSet<int> mergedReplica =
        LwwElementSet<int>.Merge(true, replica1, replica2);

      Assert.IsFalse(mergedReplica.IsEmpty());
      Assert.IsTrue(mergedReplica.Lookup(element));
    }

    /// <summary>
    /// Tests that a merged replica with an add and remove of the same element 
    /// with the same timestamp biases towards removing it.
    /// </summary>
    [TestMethod]
    public void MergeReplicasBiasRemoveTest()
    {
      int element = 1;
      long time = m_time.GetDateTimeNow();

      LwwElementSet<int> replica1 = new LwwElementSet<int>();
      replica1.Add(element, time);

      LwwElementSet<int> replica2 = new LwwElementSet<int>();
      replica2.Add(element, time);
      replica2.Remove(element, time);

      LwwElementSet<int> mergedReplica =
        LwwElementSet<int>.Merge(false, replica1, replica2);

      Assert.IsTrue(mergedReplica.IsEmpty());
      Assert.IsFalse(mergedReplica.Lookup(element));
    }
  }
}
