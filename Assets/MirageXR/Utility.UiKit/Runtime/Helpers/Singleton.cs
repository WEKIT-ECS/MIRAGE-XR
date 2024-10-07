using UnityEngine;

namespace Utility.UiKit.Runtime.Helpers
{
    public class DumbSingleton<TDumbSingleton>
        where TDumbSingleton : DumbSingleton<TDumbSingleton>, new()
    {
        private static TDumbSingleton _instance;

        public static TDumbSingleton Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = new TDumbSingleton();
                
                return _instance;
            }
        }
    }
}