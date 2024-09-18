using UnityEngine;
using Utility.UiKit.Runtime.Helpers; 

namespace MirageXR
{
    public class DumbManager<TBaseManager> : DumbSingleton<TBaseManager>
        where TBaseManager : DumbManager<TBaseManager>, new()
    {
        protected static TBaseManager Instantiate()
        {
            return Instance;
        }
    }
    
    public class Manager<TBaseManager> : Singleton<TBaseManager>
        where TBaseManager : Manager<TBaseManager>
    {
        protected static TBaseManager Instantiate()
        {
            return Instance;
        }
    }
    
    public class Singleton<TBaseBehaviour> : MonoBehaviour // TODO: BaseBehaviour
        where TBaseBehaviour : Singleton<TBaseBehaviour>
    {
        private static TBaseBehaviour _instance;

        public static TBaseBehaviour Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = (TBaseBehaviour) FindObjectOfType(typeof(TBaseBehaviour));
                if (_instance != null)
                {
                    return _instance;
                }

                var singleton = new GameObject(typeof(TBaseBehaviour).Name);

                _instance = singleton.AddComponent<TBaseBehaviour>();
                
                return _instance;
            }
        }
    }
}
