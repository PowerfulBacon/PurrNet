using PurrNet;
using UnityEngine;

public class TestTimer : NetworkIdentity
{
    [SerializeField]
    private SyncTimer countdown = new();

    private void Update()
    {
        if (!isServer)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            countdown.StartTimer(3f);
    }
}
