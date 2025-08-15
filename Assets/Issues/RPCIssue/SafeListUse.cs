using PurrNet;
using UnityEngine;

public class SafeListUse : NetworkBehaviour
{
    private SafeList<int> _safeList = new();
    
    public void Start()
    {
        _safeList.UpdateList();
    }
}
