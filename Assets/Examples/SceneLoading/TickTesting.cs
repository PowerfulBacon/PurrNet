using PurrNet;
using UnityEngine;

public class TickTesting : NetworkIdentity, ITick
{
    [SerializeField] private ulong _tick;

    [SerializeField] private SyncVar<bool> _testingSomeValue;

    protected override void OnSpawned()
    {
        _testingSomeValue.onChanged += OnChanged;
    }

    private void OnChanged(bool value)
    {
        Debug.Log($"Changed to {value}");
    }

    [PurrButton]
    public void ToggleTestingValue()
    {
        _testingSomeValue.value = !_testingSomeValue.value;
    }

    public void OnTick(float delta)
    {
        ++_tick;
    }
}
