using UnityEngine;

public class SpawnAndCallRpc : MonoBehaviour
{
    [SerializeField] private PrefabTest _prefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        var instance = Instantiate(_prefab);
        instance.test.value = 42; // Set the SyncVar value
        instance.Print("Hello from SpawnAndCallRpc!");
    }
}
