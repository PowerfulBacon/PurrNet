using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    private Queue<PlayerID> _bots = new();
    
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
        Debug.Log($"Joined scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
    {
        Debug.Log($"Loaded scene: {player} | {scene} | {asServer}");
    }

    private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
    {
        Debug.Log($"Joined: {player} | {asServer}");
    }

    private void OnPlayerUnloadedScene(PlayerID player, SceneID scene, bool asServer)
    {
        Debug.Log($"Unloaded scene: {player} | {asServer}");
    }

    private void OnPlayerLeftScene(PlayerID player, SceneID scene, bool asServer)
    {
        Debug.Log($"Left scene: {player} | {asServer}");
    }

    private void OnPlayerLeft(PlayerID player, bool asServer)
    {
        Debug.Log($"Left: {player} | {asServer}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) 
            _bots.Enqueue(networkManager.playerModule.CreateBot());
        if (Input.GetKeyDown(KeyCode.C)) 
            networkManager.playerModule.KickPlayer(_bots.Dequeue());
    }
}
