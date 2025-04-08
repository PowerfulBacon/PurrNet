using PurrNet;
using PurrNet.Modules;
using UnityEngine;

public class SpawnOnServerLoadScene : PurrMonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        OnSceneLoaded(default, asServer);
    }

    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
    }

    private void OnSceneLoaded(SceneID scene, bool asserver)
    {
        if (asserver)
        {
            Debug.Log("Server loaded scene, instantiating prefab");
            UnityProxy.Instantiate(_prefab);
        }
    }
}
