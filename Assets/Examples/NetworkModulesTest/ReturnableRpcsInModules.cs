namespace PurrNet.Examples
{
    public class ReturnableRpcsInModules : NetworkModule
    {
        /*public override async void OnSpawn(bool asServer)
        {
            if (!asServer)
            {
                var res = await PingPongTest("Ping");
                PurrLogger.Log("PingPongTest result: " + res);
            }
        }
        
        [ServerRpc(requireOwnership: false)]
        Task<string> PingPongTest<T>(T ping)
        {
            DirectoryInfo thisDir = new DirectoryInfo(".");
            return Task.FromResult(ping + " " + thisDir.Name);
        }*/
    }
}
