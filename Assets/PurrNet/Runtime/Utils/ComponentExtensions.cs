using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;

public static class ComponentExtensions
{
    #region Animator

    private static readonly FieldInfo _avatarRootField;
    
    static ComponentExtensions()
    {
        _avatarRootField = typeof(Animator).GetField("avatarRoot", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    public static Transform GetAvatarRoot(this Animator animator)
    {
#if !UNITY_2022_3_OR_NEWER
        if (animator == null || _avatarRootField == null)
        {
            return null;
        }
        return _avatarRootField.GetValue(animator) as Transform;
#else
        if (animator == null)
        {
            return null;
        }
        return animator.avatarRoot;
#endif
    }

    #endregion
    
#if !UNITY_2022_3_OR_NEWER
    public static int GetComponentIndex(this Component component)
    {
        if (component == null)
        {
            return -1;
        }

        var components = ListPool<Component>.Get();
        component.gameObject.GetComponents(component.GetType(), components);
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i] == component)
            {
                ListPool<Component>.Release(components);
                return i;
            }
        }

        ListPool<Component>.Release(components);
        return -1;
    }

    public static T GetComponentAtIndex<T>(this GameObject gameObject, int index) where T : Component
    {
        if (gameObject == null || index < 0)
        {
            return null;
        }

        var components = ListPool<T>.Get();
        gameObject.GetComponents<T>(components);
        if (index >= components.Count)
        {
            ListPool<T>.Release(components);
            return null;
        }

        ListPool<T>.Release(components);
        return components[index];
    }
#endif
}