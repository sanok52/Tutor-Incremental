using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class PlayGameObject : MonoBehaviour
{
    private void Awake()
    {
        var components = GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null || comp == this) continue;
            RegisterComponent(comp);
        }
    }

    private void OnDestroy()
    {
        var components = GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null || comp == this) continue;
            UnregisterComponent(comp);
        }
    }

    private void RegisterComponent(object comp)
    {
        var interfaces = comp.GetType().GetInterfaces();
        foreach (var iface in interfaces)
        {
            if (!iface.IsGenericType) continue;
            var genDef = iface.GetGenericTypeDefinition();
            Type stepType = iface.GetGenericArguments()[0];

            if (genDef == typeof(IPlayGameObject<>))
            {
                var method = typeof(GameMainPlayer).GetMethod("Register", BindingFlags.Public | BindingFlags.Instance);
                var genericMethod = method.MakeGenericMethod(stepType);
                genericMethod.Invoke(GameMainPlayer.Instance, new[] { comp });
                Debug.Log($"Registered {comp.GetType().Name} for step {stepType.Name} as IPlayGameObject");
            }
            else if (genDef == typeof(IPlayGameObjectRoutine<>))
            {
                var method = typeof(GameMainPlayer).GetMethod("Register", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(IPlayGameObjectRoutine<>).MakeGenericType(stepType) }, null);
                if (method == null)
                    method = typeof(GameMainPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .First(m => m.Name == "Register" && m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IPlayGameObjectRoutine<>));
                var genericMethod = method.MakeGenericMethod(stepType);
                genericMethod.Invoke(GameMainPlayer.Instance, new[] { comp });
                Debug.Log($"Registered {comp.GetType().Name} for step {stepType.Name} as IPlayGameObjectRoutine");
            }
        }
    }

    private void UnregisterComponent(object comp)
    {
        var interfaces = comp.GetType().GetInterfaces();
        foreach (var iface in interfaces)
        {
            if (!iface.IsGenericType) continue;
            var genDef = iface.GetGenericTypeDefinition();
            Type stepType = iface.GetGenericArguments()[0];

            if (genDef == typeof(IPlayGameObject<>))
            {
                var method = typeof(GameMainPlayer).GetMethod("Unregister", BindingFlags.Public | BindingFlags.Instance);
                var genericMethod = method.MakeGenericMethod(stepType);
                genericMethod.Invoke(GameMainPlayer.Instance, new[] { comp });
            }
            else if (genDef == typeof(IPlayGameObjectRoutine<>))
            {
                var method = typeof(GameMainPlayer).GetMethod("Unregister", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(IPlayGameObjectRoutine<>).MakeGenericType(stepType) }, null);
                if (method == null)
                    method = typeof(GameMainPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .First(m => m.Name == "Unregister" && m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IPlayGameObjectRoutine<>));
                var genericMethod = method.MakeGenericMethod(stepType);
                genericMethod.Invoke(GameMainPlayer.Instance, new[] { comp });
            }
        }
    }
}