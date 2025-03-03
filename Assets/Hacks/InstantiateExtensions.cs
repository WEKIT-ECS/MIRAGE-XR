using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Hacks
{
    public static class InstantiateExtensions
    {
        public static UnityEvent<Object> OnInstantiated { get; private set; } = new();

        public static void Initialize()
        {
             var harmony = new Harmony("mirage.xr");
             PatchInstantiateMethods(harmony);
        }

        private static void PatchInstantiateMethods(Harmony harmony)
        {
            var type = typeof(Object);
            var methodName = nameof(Object.Instantiate);

            var newType = typeof(InstantiateExtensions);

            var prefixMethod = newType.GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static);
            var postfixMethod = newType.GetMethod(nameof(Postfix), BindingFlags.NonPublic | BindingFlags.Static);
            var prefix = new HarmonyMethod(prefixMethod);
            var postfix = new HarmonyMethod(postfixMethod);

            var sourceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
 
            foreach (var method in sourceMethods)
            {
                if (method.Name == methodName)
                {
                    try
                    {
                        harmony.Patch(method, prefix, postfix);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"[Harmony] method patch exception: {e}");
                    }
                }
            }
        }

        private static void Prefix(Object original)
        {
        }

        // ReSharper disable once InconsistentNaming
        private static void Postfix(Object __result)
        {
            OnInstantiated.Invoke(__result);
        }
    }
}
