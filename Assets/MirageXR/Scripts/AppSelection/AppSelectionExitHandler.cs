//using UnityEngine;

//namespace MirageXR
//{
//    public class AppSelectionExitHandler : MonoBehaviour
//    {
//        void OnEnable()
//        {
//            SpatialMappingHelper.DeactivateSpatialMapping();
//            EventManager.OnAppSelectionExit += HandleExit;
//        }

//        void OnDisable()
//        {
//            EventManager.OnAppSelectionExit -= HandleExit;
//        }

//        void HandleExit()
//        {
//            Destroy(gameObject);
//        }
//    }
//}