using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField, PurrScene] private string _sceneOne, _sceneTwo; 
    
    private Queue<PlayerID> _bots = new();

    private bool _networkManagerLogs = false;

    private SyncEvent<int> _syncEvent = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _syncEvent += MyTestEvent;
    }

    private void OnDisable()
    {
        _syncEvent -= MyTestEvent;
    }

    private void MyTestEvent(int myInt)
    {
        Debug.Log($"Event triggered! {myInt}");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
            _syncEvent.Invoke(53);
        
        if (Input.GetKeyDown(KeyCode.X))
            _bots.Enqueue(networkManager.playerModule.CreateBot());
        if (Input.GetKeyDown(KeyCode.C)) 
            networkManager.playerModule.KickPlayer(_bots.Dequeue());

        if (Input.GetKeyDown(KeyCode.Alpha1))
            networkManager.sceneModule.LoadSceneAsync(_sceneOne);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            networkManager.sceneModule.LoadSceneAsync(_sceneTwo);
    }

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            return;
        
        networkManager.onPlayerJoinedScene += OnPlayerJoinedScene;
        networkManager.onPlayerLoadedScene += OnPlayerLoadedScene;
        networkManager.onPlayerJoined += OnPlayerJoined;
        networkManager.onPlayerLeft += OnPlayerLeft;
        networkManager.onPlayerLeftScene += OnPlayerLeftScene;
        networkManager.onPlayerUnloadedScene += OnPlayerUnloadedScene;
    }

    protected override void OnDespawned()
    {
        networkManager.onPlayerJoinedScene -= OnPlayerJoinedScene;
        networkManager.onPlayerLoadedScene -= OnPlayerLoadedScene;
        networkManager.onPlayerJoined -= OnPlayerJoined;
        networkManager.onPlayerLeft -= OnPlayerLeft;
        networkManager.onPlayerLeftScene -= OnPlayerLeftScene;
        networkManager.onPlayerUnloadedScene -= OnPlayerUnloadedScene;
    }

    private void OnPlayerJoinedScene(PlayerID player, SceneID scene, bool asServer)
    {
        if (!_networkManagerLogs)
            return;
        
        if (player.id == 1)
            return;

        Debug.Log($"Joined scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
    {
        if (!_networkManagerLogs)
            return;

        if (player.id == 1)
            return;

        Debug.Log($"Loaded scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
    {
        if (!_networkManagerLogs)
            return;

        if (player.id == 1)
            return;

        Debug.Log($"Joined: {player} | {asServer}");
    }

    private void OnPlayerUnloadedScene(PlayerID player, SceneID scene, bool asServer)
    {
        if (!_networkManagerLogs)
            return;

        if (player.id == 1)
            return;
        
        Debug.Log($"Unloaded scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerLeftScene(PlayerID player, SceneID scene, bool asServer)
    {
        if (!_networkManagerLogs)
            return;

        if (player.id == 1)
            return;

        Debug.Log($"Left scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerLeft(PlayerID player, bool asServer)
    {
        if (!_networkManagerLogs)
            return;

        if (player.id == 1)
            return;

        Debug.Log($"Left: {player} | {asServer}");
    }
}
