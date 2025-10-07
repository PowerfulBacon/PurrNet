using PurrNet.Logging;
using PurrNet.Packing;
using UnityEngine;

public class TestingDeltas : MonoBehaviour
{
    void Start()
    {
        using var packer = BitPackerPool.Get();

        ulong old = 1;
        ulong @new = 76;

        DeltaPacker<ulong>.Write(packer, old, @new);

        packer.ResetPosition();

        ulong read = default;
        DeltaPacker<ulong>.Read(packer, old, ref read);

        PurrLogger.Log($"Read: {read}. It should be {@new}");
    }
}
