using System;

namespace PurrNet.Pooling
{
    public struct AllocationDetails
    {
        public readonly string allocationTrace;
        public readonly string targetType;
        public readonly WeakReference<object> reference;

        public AllocationDetails(object target)
        {
            allocationTrace = Environment.StackTrace;
            targetType = target.GetType().ToString();
            reference = new WeakReference<object>(target);
        }
    }
}