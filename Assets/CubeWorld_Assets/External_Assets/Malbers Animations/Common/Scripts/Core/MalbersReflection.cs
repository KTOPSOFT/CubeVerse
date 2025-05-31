using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class MalbersReflection
{
    #region Reflections
    /// <summary>  Given an Object and the name of a property, it returns the Property value  </summary>
    public static PropertyInfo GetProperty<T>(this UnityEngine.Object component, string propertyName)
    {
        Type type = component.GetType();

        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (property == null)
        {
            Debug.LogError($"Property '{propertyName}' of type '{typeof(T)}' not found on [{type.FullName}]");
            return null;
        }
        else if (property.PropertyType != typeof(T))
        {
            Debug.LogError($"Property '{propertyName}' was found, but it does not have the type '{typeof(T)}'. '{type.FullName}'.");
            return null;
        }

        return property;
    }


    /// <summary> Given an Object and the name of a Method with no parameter, it returns the Property value  </summary>
    public static MethodInfo GetMethod<T>(this UnityEngine.Object component, string methodName)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));

        Type type = component.GetType();

        MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (method == null)
        {
            Debug.Log($"Method ({typeof(T)})[{methodName}] not found on [{type.FullName}].", component);
            return null;
        }
        else if (method.GetParameters().Length > 0)
        {
            Debug.LogWarning($"Method ({typeof(T)})[{methodName}] needs arguments. Skip", component);
            return null;
        }
        else if (method.ReturnType != typeof(T))
        {
            Debug.Log($"Method [{methodName}] was found, but it does not have the type ({typeof(T)}). '{type.FullName}'.", component);
            return null;
        }

        return method;
    }


    public static FieldInfo GetField<T>(this UnityEngine.Object component, string fieldName)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));

        Type type = component.GetType();

        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (field == null)
        {
            Debug.LogError($"Variable ({typeof(T)})[{fieldName}] not found on [{type.FullName}].");
            return default;
        }
        else if (field.FieldType != typeof(T))
        {
            Debug.LogError($"Variable [{fieldName}] was found, but it does not have the type ({typeof(T)}). '{type.FullName}'.");
            return default;
        }

        return field;
    }

    /// <summary>  Given an Object and the name of a property, it returns the Property value  </summary>
    public static T GetPropertyValue<T>(this UnityEngine.Object component, string propertyName)
    {
        var property = component.GetProperty<T>(propertyName);
        if (property == null) return default;
        return (T)property.GetValue(component);
    }

    /// <summary> Given an Object and the name of a Method with no parameter, it returns the Property value  </summary>
    public static T GetMethodValue<T>(this UnityEngine.Object component, string methodName)
    {
        MethodInfo method = GetMethod<T>(component, methodName);
        if (method == null) return default;
        return (T)method.Invoke(component, null);
    }

    public static T GetFieldValue<T>(this UnityEngine.Object component, string fieldName)
    {
        var field = GetField<T>(component, fieldName);
        if (field == null) return default;
        return (T)field.GetValue(component);
    }

    /// <summary>Converts a Method Info into a Unity Action</summary>
    public static UnityAction<T> CreateDelegate<T>(object target, MethodInfo method)
    {
        var del = (UnityAction<T>)Delegate.CreateDelegate(typeof(UnityAction<T>), target, method);
        return del;
    }

    /// <summary>Converts a Method Info into a Unity Action</summary>
    public static UnityAction CreateDelegate(object target, MethodInfo method)
    {
        var del = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method);
        return del;
    }

    /// <summary> Returns a Unity Action from a component and a method. Used to connect methods in the inspector </summary>
    public static UnityAction GetUnityAction(this Component c, string component, string method)
    {
        var sender = (c.GetComponent(component) ?? c.GetComponentInParent(component)) ?? c.GetComponentInChildren(component);

        MethodInfo methodPtr;

        //Debug.Log("sender = " + sender);

        if (sender != null)
        {
            methodPtr = sender.GetType().GetMethod(method, new Type[0]);
        }
        else return null;

        if (methodPtr != null)
        {
            // Debug.Log("methodPtr = " + methodPtr.Name);
            var action = CreateDelegate(sender, methodPtr);
            return (action);
        }

        return null;
    }

    public static Type FindType(string qualifiedTypeName)
    {
        Type t = Type.GetType(qualifiedTypeName);

        if (t != null)
        {
            return t;
        }
        else
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(qualifiedTypeName);
                if (t != null)
                    return t;
            }
            return null;
        }
    }

    public static UnityAction<T> GetUnityAction<T>(this Component c, string component, string method)
    {
        if (string.IsNullOrEmpty(component)) return null;

        var sender = (c.GetComponent(component) ?? c.GetComponentInParent(component)) ?? c.GetComponentInChildren(component);
        if (sender == null) return null;

        var methodPtr = sender.GetType().GetMethod(method, new Type[] { typeof(T) });

        if (methodPtr != null)
        {
            var action = CreateDelegate<T>(sender, methodPtr);
            return (action);
        }

        PropertyInfo property = sender.GetType().GetProperty(method);

        if (property != null)
        {
            var action = CreateDelegate<T>(sender, property.SetMethod);
            return (action);
        }

        return null;
    }

    public static T GetFieldClass<T>(this Component owner, string component, string field) where T : class
    {
        var sender = owner.GetComponent(component);

        if (sender != null)
        {
            FieldInfo methodPtr = sender.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);

            if (methodPtr != null)
            {
                return methodPtr.GetValue(sender) as T;
            }
        }
        return null;
    }


    /// <summary> Invoke with Parameters </summary>
    public static bool InvokeWithParams(this MonoBehaviour sender, string method, object args)
    {
        Type argType = null;

        if (args != null) argType = args.GetType();

        MethodInfo methodPtr;

        if (argType != null)
        {
            methodPtr = sender.GetType().GetMethod(method, new Type[] { argType });
        }
        else
        {
            try
            {
                methodPtr = sender.GetType().GetMethod(method);
            }
            catch (Exception)
            {
                //methodPtr = sender.GetType().GetMethods().First
                //(m => m.Name == method && m.GetParameters().Count() == 0);
                // Debug.Log("OTHER");
                throw;
            }

        }

        if (methodPtr != null)
        {
            if (args != null)
            {
                var arguments = new object[1] { args };
                methodPtr.Invoke(sender, arguments);
                return true;
            }
            else
            {
                methodPtr.Invoke(sender, null);
                return true;
            }
        }

        PropertyInfo property = sender.GetType().GetProperty(method);

        if (property != null)
        {
            property.SetValue(sender, args, null);
            return true;

        }
        return false;
    }

    #endregion
    /// <summary>Invoke with Parameters and Delay </summary>
    public static void InvokeDelay(this MonoBehaviour behaviour, string method, object options, YieldInstruction wait)
    {
        behaviour.StartCoroutine(_invoke(behaviour, method, wait, options));
    }

    private static IEnumerator _invoke(this MonoBehaviour behaviour, string method, YieldInstruction wait, object options)
    {
        yield return wait;

        Type instance = behaviour.GetType();
        MethodInfo mthd = instance.GetMethod(method);
        mthd.Invoke(behaviour, new object[] { options });

        yield return null;
    }


    /// <summary>Invoke with Parameters for Scriptable objects</summary>
    public static void Invoke(this ScriptableObject sender, string method, object args)
    {
        var methodPtr = sender.GetType().GetMethod(method);

        if (methodPtr != null)
        {
            if (args != null)
            {
                var arguments = new object[1] { args };
                methodPtr.Invoke(sender, arguments);
            }
            else
            {
                methodPtr.Invoke(sender, null);
            }
        }
    }

    /// <summary> Uses Getcomponent in childern but with a string</summary>
    public static Component GetComponentInChildren(this Component owner, string classtype)
    {
        var sender = owner.GetComponent(classtype);
        if (sender) return sender;
        else
        {
            foreach (Transform item in owner.transform)
            {
                var found = item.GetComponentInChildren(classtype);
                if (found) return found;
            }
        }

        return null;
    }

    /// <summary> Uses GetComponent in Parent but with a string</summary>
    public static Component GetComponentInParent(this Component owner, string classtype)
    {
        var sender = owner.GetComponent(classtype);

        if (sender != null)
        {
            return sender;
        }
        else
        {
            if (owner.transform.parent == null)
            {
                return null;
            }
            else
            {
                return owner.transform.parent.GetComponentInParent(classtype);
            }
        }
    }

    /// <summary> Gets a real copy of a component / </summary>
    private static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                catch { }
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddCopyComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd);
    }

    ///// <summary>  Resize a Listener Number from a Unity Event Base </summary>
    //public static int GetListenerNumber(this UnityEventBase unityEvent)
    //{
    //    var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
    //    var invokeCallList = field.GetValue(unityEvent);
    //    var property = invokeCallList.GetType().GetProperty("Count");
    //    return (int)property.GetValue(invokeCallList);
    //}

}
