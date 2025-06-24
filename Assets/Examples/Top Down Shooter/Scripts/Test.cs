using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private Texture _testTexture;
    [SerializeField] private Sprite _testSprite;

    [PurrButton]
    private void SendMat()
    {
        TestRpc(_testTexture);
        TestRpc(_testSprite);
    }
    
    [ObserversRpc]
    private void TestRpc(Texture receivedTexture)
    {
        Debug.Log($"Received Texture: {receivedTexture.name}", receivedTexture);
    }
    
    [ObserversRpc]
    private void TestRpc(Sprite receivedSprite)
    {
        Debug.Log($"Received sprite: {receivedSprite.name}", receivedSprite);
    }
}
