using PurrNet;
using UnityEngine;

[RegisterNetworkType(typeof(Texture))]
[RegisterNetworkType(typeof(Sprite))]
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
    private void TestRpc(object receivedTexture)
    {
        Debug.Log($"Received: {receivedTexture.GetType().Name}");
    }
}
