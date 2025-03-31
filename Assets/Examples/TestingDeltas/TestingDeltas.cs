using PurrNet;
using PurrNet.Modules;

public class TestingDeltas : NetworkIdentity
{
    private DeltaMessagerModule GetMessenger(bool asServer)
    {
        if (networkManager.TryGetModule<DeltaMessagerFactory>(asServer, out var factory) &&
            factory.TryGetModule(sceneId, out var messenger))
        {
            return messenger;
        }

        return null;
    }

    protected override void OnSpawned(bool asServer)
    {
        // GetMessenger(asServer).
    }

    private void OnGUI()
    {
    }
}
