using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestConnectionEvents
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene($"SimpleNetworkManager");
    }

    public IEnumerator WaitWithTimeout(Func<bool> predicate, float timeout)
    {
        float timer = 0;

        while (!predicate())
        {
            if (timer >= timeout)
                Assert.Fail("Timeout reached.");
            timer += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator ConnectionStates()
    {
        var manager = GameObject.Find("PurrNet");

        if (!manager)
            Assert.Fail("PurrNet not found in scene.");

        if (!manager.TryGetComponent(out NetworkManager networkManager))
            Assert.Fail("NetworkManager not found in PurrNet.");

        var clientStates = new Queue<ConnectionState>();
        var serverStates = new Queue<ConnectionState>();

        networkManager.onClientConnectionState += OnClientState;
        networkManager.onServerConnectionState += OnServerState;

        networkManager.StartHost();

        yield return WaitWithTimeout(() => networkManager.isHost, 5);

        var connecting = clientStates.Dequeue();
        Assert.AreEqual(ConnectionState.Connecting, connecting);

        var connected = clientStates.Dequeue();
        Assert.AreEqual(ConnectionState.Connected, connected);

        var connectedState = serverStates.Dequeue();
        Assert.AreEqual(ConnectionState.Connecting, connectedState);

        var connectingState = serverStates.Dequeue();
        Assert.AreEqual(ConnectionState.Connected, connectingState);


        yield break;

        void OnClientState(ConnectionState state)
        {
            clientStates.Enqueue(state);
        }

        void OnServerState(ConnectionState state)
        {
            serverStates.Enqueue(state);
        }
    }

    [UnityTest]
    public IEnumerator RemoteProceduralCalls()
    {
        var manager = GameObject.Find("PurrNet");

        if (!manager)
            Assert.Fail("PurrNet not found in scene.");

        if (!manager.TryGetComponent(out NetworkManager networkManager))
            Assert.Fail("NetworkManager not found in PurrNet.");

        networkManager.StartHost();

        yield return WaitWithTimeout(() => networkManager.isHost, 5);

        yield break;
    }
}
