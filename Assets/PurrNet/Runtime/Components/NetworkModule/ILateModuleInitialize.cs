
using PurrNet;

/// <summary>
/// Attach to something inserted into a network module to intercept initialization events.
/// Implemented on all network modules so that they can initialize when attached to a parent
/// module, such as a SyncVar.
/// </summary>
public interface ILateModuleInitialize
{

    /// <summary>
    /// Invoked when attached to a network module outside of being initialized at runtime.
    /// This function is invoked by the Sync variable module types to initialize any modules
    /// that are inserted into them which may not have existed when the sync type was initially
    /// networked.
    /// </summary>
    /// <param name="parentIdentity">The parent network identity, the ultimate network identity that contains the module that we attached to.</param>
    /// <param name="attachedToModule">The module that we were attached to, usually a SyncVar<>, SyncList<>, or other sync type but custom calls may be implemented.</param>
    void LateInitialize(NetworkIdentity parentIdentity, NetworkModule attachedToModule, byte? moduleID = null);

}
