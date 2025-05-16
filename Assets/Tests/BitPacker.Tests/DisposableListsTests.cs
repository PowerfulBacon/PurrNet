using NUnit.Framework;
using PurrNet;
using PurrNet.Packing;
using PurrNet.Pooling;
using PurrNet.Utils;
using UnityEngine;

public class DisposableListsTests
{
    private BitPacker packer;

    [SetUp]
    public void Setup()
    {
        Hasher.ClearState();
        NetworkManager.CallAllRegisters();

        packer = BitPackerPool.Get();
    }

    [TearDown]
    public void Teardown()
    {
        packer?.Dispose();
    }

    [Test]
    public void TestDeltaDisposableList()
    {
        var oldList = new DisposableList<int>(5);
        var newList = new DisposableList<int>(5);

        for (int i = 0; i < 5; i++)
        {
            oldList.Add(i);
            newList.Add(i);
        }

        // Test with the same list
        bool hasChanged = packer.WriteDisposableDeltaList(oldList, newList);
        Assert.IsFalse(hasChanged, "Lists should be equal");

        // Modify the new list
        newList[0] = 10;

        // Test with different lists
        hasChanged = packer.WriteDisposableDeltaList(oldList, newList);
        Assert.IsTrue(hasChanged, "Lists should not be equal");
    }

    [Test]
    public void TestDeltaSameLength()
    {
        var old = new DisposableList<int>(5);
        var @new = new DisposableList<int>(5);

        for (int i = 0; i < 5; i++)
        {
            old.Add(i);
            @new.Add(i * 2);
        }

        bool hasChanged = packer.WriteDisposableDeltaList(old, @new);
        Assert.IsTrue(hasChanged, "Lists should not be equal");

        Debug.Log("Written bits: " + packer.positionInBits);
        packer.ResetPositionAndMode(true);

        var readList = default(DisposableList<int>);
        packer.ReadDisposableDeltaList(old, ref readList);

        Assert.AreEqual(5, readList.Count, "Read list should have the same count");

        for (int i = 0; i < 5; i++)
            Assert.AreEqual(i * 2, readList[i], $"Read list item {i} should be equal to {@new[i]}");
    }
}
