using System;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            return;
        
        networkManager.onPlayerJoinedScene += OnPlayerJoinedScene;
        networkManager.onPlayerLoadedScene += OnPlayerLoadedScene;
        networkManager.onPlayerJoined += OnPlayerJoined;
    }

    protected override void OnDespawned()
    {
        networkManager.onPlayerJoinedScene -= OnPlayerJoinedScene;
        networkManager.onPlayerLoadedScene -= OnPlayerLoadedScene;
        networkManager.onPlayerJoined -= OnPlayerJoined;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) 
            networkManager.playerModule.CreateBot();
    }
}
