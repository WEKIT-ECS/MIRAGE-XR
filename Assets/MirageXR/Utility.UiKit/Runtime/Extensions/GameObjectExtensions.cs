using UnityEngine;

namespace Utility.UiKit.Runtime.Extensions
{
    public static class GameObjectExtensions
    {
        public static void SafeSetActive(this GameObject gameObject, bool value)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(value);
            }
        }
    }
}