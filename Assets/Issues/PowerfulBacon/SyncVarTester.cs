#nullable enable

using PurrNet;
using System;
using System.Collections.Generic;

public class SyncVarTester : NetworkIdentity
{

    public SyncVar<int> intTester = new SyncVar<int>(0);

    public SyncList<Foo> foo = new SyncList<Foo>(new List<Foo>() {
        new Foo(-1)
    });

    protected override void OnSpawned(bool asServer)
    {
        networkManager.onPlayerJoined += NetworkManager_onPlayerJoined;
        if (asServer)
        {
            intTester.value = UnityEngine.Random.Range(0, 100);
        }
    }

    private void NetworkManager_onPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
    {
        if (player.isServer || player == localPlayer || isReconnect || !asServer)
            return;
        // Late instantiation
        foo.Add(new Foo((int)player.id.value));
    }
}

[Serializable]
public class Foo : NetworkModule
{

    public SyncVar<Bar> test = new SyncVar<Bar>(new Bar());

    public SyncVar<int> shallowSyncVar = new SyncVar<int>(7);

    // Non standard constructor prevents PurrNet from instantiating this instantly
    public Foo(int example)
    {
        test.value.test = example;
        test.value.deepSyncVar.value = example;
        shallowSyncVar.value = example;
    }

    private Foo()
    {

    }

}

[Serializable]
public class Bar : NetworkModule
{

    public int test = 5;

    public SyncVar<int> deepSyncVar = new SyncVar<int>(6);

}

#nullable restore
