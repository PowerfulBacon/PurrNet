using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace PurrNet
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ServerOnlyAttribute : PreserveAttribute
    {
        [UsedImplicitly]
        // TODO: Implement codegen for this attribute and stripping too
        public ServerOnlyAttribute(StripCodeModeOverride stripCodeMode = StripCodeModeOverride.Settings) { }
    }
}
