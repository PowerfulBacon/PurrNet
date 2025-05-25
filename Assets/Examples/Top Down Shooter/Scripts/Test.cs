using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private int _localHealth = 100;
    [SerializeField] private SyncDictionary<int, int> _dictionary = new ();

    private void Awake()
    {
        _dictionary.onChanged += OnListChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _dictionary.onChanged -= OnListChanged;
    }

    private void OnListChanged(SyncDictionaryChange<int, int> change)
    {
        Debug.Log($"{change} | Tick: {networkManager.tickModule.syncedTick}");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
            SetHealth(_localHealth - 10);
        if (Input.GetKeyDown(KeyCode.C))
            TestList();
    }

    private void TestList()
    {
        if(!_dictionary.TryAdd(0, 0))
            _dictionary[0] += 1;
        if(!_dictionary.TryAdd(1, 0))
            _dictionary[1] += 1;
        if(!_dictionary.TryAdd(2, 0))
            _dictionary[2] += 1;
        if(!_dictionary.TryAdd(3, 0))
            _dictionary[3] += 1;

        _dictionary.Remove(0);
    }

    [ObserversRpc(bufferLast: true)]
    private void SetHealth(int newHealth)
    {
        _localHealth = newHealth;
    }
}
