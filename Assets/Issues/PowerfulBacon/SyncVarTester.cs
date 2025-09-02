#nullable enable

using PurrNet;
using System;

public class SyncVarTester : NetworkIdentity
{

    public SyncVar<int> intTester = new SyncVar<int>(0);

    public SyncDictionary<int, Foo> foo = new SyncDictionary<int, Foo>();

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
        if (foo.ContainsKey((int)player.id.value))
            return;
        // Late instantiation
        foo.Add((int)player.id.value, new Foo((int)player.id.value));
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
