using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Pooling;
using UnityEngine;

public class TestingDeltas : MonoBehaviour
{
    void Start()
    {
        var old = DisposableList<int>.Create();

        old.Add(1);

        old.Add(2);

        old.Add(5);

        var @new = DisposableList<int>.Create(old);

        @new[1] = 3;

        using var packer = BitPackerPool.Get();

        DeltaPacker<DisposableList<int>>.Write(packer, old, @new);
        packer.ResetPosition();

        DisposableList<int> result = default;
        DeltaPacker<DisposableList<int>>.Read(packer, old, ref result);

        Debug.Log(@new);
        Debug.Log(result);
    }
}
