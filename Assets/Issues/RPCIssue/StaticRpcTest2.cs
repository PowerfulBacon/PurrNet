using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class StaticRpcTest2<T> : NetworkModule
{
    [TargetRpc]
    public static void StaticMethod(PlayerID test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    [TargetRpc]
    public void StaticMethodNotStatic(PlayerID test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    [TargetRpc]
    public static void StaticMethod(PlayerID[] test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    [TargetRpc]
    public void StaticMethodNotStatic(PlayerID[] test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    [TargetRpc]
    public static void StaticMethod(HashSet<PlayerID> test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    [TargetRpc]
    public void StaticMethodNotStatic(HashSet<PlayerID> test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }

    /*[TargetRpc]
    public static void StaticMethod(HashSet<PlayerID> test, T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }*/
}
