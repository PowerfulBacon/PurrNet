using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace PurrNet
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ServerAttribute : PreserveAttribute
    {
        [UsedImplicitly]
        public ServerAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class OwnerAttribute : PreserveAttribute
    {
        [UsedImplicitly]
        public OwnerAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ClientAttribute : PreserveAttribute
    {
        [UsedImplicitly]
        public ClientAttribute() { }
    }
}
