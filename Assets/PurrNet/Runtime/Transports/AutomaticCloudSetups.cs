using System;
using UnityEngine;

namespace PurrNet.Transports
{
    [Serializable]
    public class AutomaticCloudSetups
    {
        [SerializeField] private bool _adaptToEdgegap = true;

#if EDGEGAP_PURRNET_SUPPORT && UNITY_SERVER
        public bool adaptToEdgegap => _adaptToEdgegap;
#else
        public bool adaptToEdgegap => false;
#endif
    }
}
