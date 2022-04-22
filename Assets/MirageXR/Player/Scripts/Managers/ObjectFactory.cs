using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace MirageXR
{
    public class ObjectFactory : MonoBehaviour
    {
        private void OnEnable()
        {
            // Register to event manager events.
            EventManager.OnToggleObject += Toggle;
        }

        private void OnDisable()
        {
            // Unregister from event manager events.
            EventManager.OnToggleObject -= Toggle;
        }

        /// <summary>
        /// Toggle on/off action toggle object.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <param name="isActivating">Toggle state.</param>
        private static void Toggle(ToggleObject obj, bool isActivating)
        {
            // Do magic based on the object type.
            switch (obj.type)
            {
                case "tangible":
                    // Check predicate.
                    switch (obj.predicate)
                    {
                        // Handle action symbols by default.
                        default:
                            if (isActivating)
                            {
                                if (obj.id == "ActionsViewport")
                                {
                                    ActivatePrefab("UiSymbolPrefab", obj);
                                }
                                else
                                {
                                    // Last minute change for enabling 3D glyph option.
                                    if (obj.predicate.StartsWith("3d:"))
                                    {
                                        obj.option = obj.predicate.Replace("3d:","");

                                        obj.url = "resources://" + obj.predicate;

                                        ActivatePrefab("Model/ModelPrefab", obj);
                                    }     
                                    else if (obj.predicate.StartsWith("act:"))
                                    {
                                        obj.option = obj.predicate.Replace("act:", "");

                                        obj.url = "resources://" + obj.predicate;

                                        var glyphModel = "Glyphs/" + obj.option;
                                        ActivatePrefab(glyphModel, obj);
                                    }
                                    else if (obj.predicate.StartsWith("vfx:"))
                                    {
                                        obj.option = obj.predicate.Replace("vfx:", "");

                                        obj.url = "resources://" + obj.predicate;

                                        var vfxModel = "VFX/" + obj.option;
                                        ActivatePrefab(vfxModel, obj);
                                    }
                                    else  if (obj.predicate.StartsWith("char:"))
                                    {
                                        obj.option = obj.predicate.Replace("char:", "");

                                        //obj.predicate = "character";
                                        obj.url = "resources://" + obj.predicate;


                                        var charModel = "Characters/" + obj.option + "/" + obj.option;
                                        ActivatePrefab(charModel, obj);
                                    }
                                    else if (obj.predicate.StartsWith("plugin:"))
                                    {
                                        obj.option = "PluginControllerPrefab";
                                       
                                        ActivatePrefab(obj.option, obj);
                                    }
                                    // True and tested 2D symbols.
                                    else
                                        ActivatePrefab("SymbolPrefab", obj);
                                }    
                            }

                            else
                            {
                                if (obj.id == "ActionsViewport")
                                    DestroyPrefab(obj);
                                else
                                {
                                    // Last minute change to enable 3D glyph option.
                                    if (obj.predicate.StartsWith("3d_"))
                                    {
                                        // Let's clean the crap from the name.
                                        obj.option = obj.predicate.Replace("3d_", "");

                                        obj.url = "resources://" + obj.predicate;

                                        obj.predicate = "model";

                                        DestroyPrefab(obj);
                                    }
                                    else if (obj.predicate.StartsWith("act:"))
                                    {
                                        obj.option = obj.predicate.Replace("act:", "");

                                        //obj.predicate = "glyph";
                                        obj.url = "resources://" + obj.predicate;

                                        DestroyPrefab(obj);
                                    }
                                    else if (obj.predicate.StartsWith("vfx:"))
                                    {
                                        obj.option = obj.predicate.Replace("vfx:", "");

                                        //obj.predicate = "glyph";
                                        obj.url = "resources://" + obj.predicate;

                                        DestroyPrefab(obj);
                                    }
                                    else if (obj.predicate.StartsWith("char:"))
                                    {
                                        obj.option = obj.predicate.Replace("char:", "");

                                        //obj.predicate = "character";
                                        obj.url = "resources://" + obj.predicate;

                                        DestroyPrefab(obj);
                                    }
                                    else if (obj.predicate.StartsWith("plugin:"))
                                    {
                                        DestroyPrefab(obj);
                                    }
                                    // True and tested 2D symbol option.
                                    else
                                        DestroyPrefab(obj);
                                }
                                    
                            }
                            break;

                        // Handle label type.
                        case "label":
                            if (isActivating)
                                ActivatePrefab("LabelPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;

                        // Handle detect type.
                        case "detect":
                            if (isActivating)
                            {
                                ActivatePrefab("DetectPrefab", obj);
                            }
                            else
                                DestroyPrefab(obj);
                            break;

                        // Audio type.
                        case "audio":
                        case "sound":
                            if (isActivating)
                            {
                                ActivatePrefab("AudioPrefab", obj);
                            }
                            else
                            {
                                DestroyPrefab(obj);
                            }

                            break;

                        // Video type.
                        case "video":
                            if (isActivating)
                            {
                                ActivatePrefab("VideoPrefab", obj);
                            }
                            else
                            {
                                DestroyPrefab(obj);
                            }

                            break;

                        // Post it label type.
                        case "postit":
                            if (isActivating)
                                ActivatePrefab("PostItPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;
                        
                        // 3D model type.
                        case "3dmodel":
                        case "model":
                            if (isActivating)
                            {
                                ActivatePrefab("ModelPrefab", obj);
                            }
                            else
                                DestroyPrefab(obj);
                            break;
                        
                        // Ghost tracks type.
                        case "ghosttracks":
                            if (isActivating)
                                ActivatePrefab(obj.option.Split(':')[0], obj);
                            else
                                DestroyPrefab(obj);
                            break;

                        // Ghost hands type.
                        case "hands":
                            if (isActivating)
                                Debug.Log("hands activated");
                                //ActivatePrefab("HandsPrefab", obj);
                            //else
                               // DestroyPrefab(obj);
                            break;

                        // Image type.
                        case "image":
                            if (isActivating)
                            {
                                ActivatePrefab("ImagePrefab", obj);
                            }
                            else
                            {
                                DestroyPrefab(obj);
                            }
                            break;
                        // Image type.
                        case "imagemarker":
                            if (isActivating)
                            {
                                ActivatePrefab("ImageMarkerPrefab", obj);
                            }
                            else
                            {
                                DestroyPrefab(obj);
                            }
                            break;

                        // Potentiometer type.
                        case "potentiometer":
                            if (isActivating)
                                ActivatePrefab("PotentiometerPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;

                        // Show card button type.
                        case "menutoggle":
                            if (isActivating)
                                ActivatePrefab("ShowCardsPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;

                        // VTT demo specific hacks.
                        case "filterIn":
                            if (isActivating)
                                ActivatePrefab("FilterIn", obj);
                            else
                                DestroyPrefab(obj);
                            break;

                        case "filterOut":
                            if (isActivating)
                                ActivatePrefab("FilterOut", obj);
                            else
                                DestroyPrefab(obj);
                            break;
                        case "pickandplace":
                            if (isActivating)
                                ActivatePrefab("pickandplaceprefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;
                        case "plugin":
                            if (isActivating)
                                ActivatePrefab("PluginControllerPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;
                        case "drawing":
                            if(isActivating)
                                ActivatePrefab("DrawingPrefab", obj);
                            else
                                DestroyPrefab(obj);
                            break;
                    }
                    break;

                default:
                    EventManager.DebugLog("ObjectFactory: Toggle - Unknown type: " + obj.type);
                    break;
            }
        }

        private static void ActivatePrefab(string prefab, ToggleObject obj)
        {
            var actionList = ActivityManager.Instance.ActionsOfTypeAction;
            GameObject temp = null;
            var activeActionIndex = actionList.IndexOf(ActivityManager.Instance.ActiveAction);

            try
            {
                //if we are in the active step and the annotaiton exists in this step
                if (actionList[activeActionIndex] == ActivityManager.Instance.ActiveAction
                    && actionList[activeActionIndex].enter.activates.Find(p => p.poi == obj.poi) != null)
                {
                    var objIfExist = GameObject.Find(obj.poi);

                    if (objIfExist)
                    {
                        var annotationChildren = objIfExist.transform.childCount;

                        //prevent duplication
                        if (annotationChildren > 0)
                        {
                            DestroyPrefab(obj);
                        }
                    }

                    //if this is the only clone of this object in this annotation
                    var prefabInResources = Resources.Load<GameObject>("Prefabs/" + prefab);
                    //if the prefab file exist
                    if(prefabInResources != null)
                    {
                        temp = Instantiate(prefabInResources, Vector3.zero, Quaternion.identity);
                        AddExtraComponents(temp, obj);
                    }
                    else
                    {
                        if (obj.predicate.StartsWith("char"))
                        {
                            var charPrefabPath = $"{ActivityManager.Instance.Path}/characterinfo/{obj.option}";
                            if (File.Exists(charPrefabPath))
                            {
                                var loadedAssetBundle = AssetBundle.LoadFromFile(charPrefabPath);
                                temp = Instantiate((GameObject) loadedAssetBundle.LoadAsset(obj.option), Vector3.zero, Quaternion.identity);
                                loadedAssetBundle.Unload(false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            if (temp == null) return;

            // Try to initialize and if it fails, debug and destroy the object.
            if (temp.GetComponent<MirageXRPrefab>() && !temp.GetComponent<MirageXRPrefab>().Init(obj))
            {
                EventManager.DebugLog($"Couldn't create the {prefab}. {obj.id}/{obj.poi}/{obj.predicate}");
                temp.GetComponent<MirageXRPrefab>().Delete();
            }
        }

        private async static void AddExtraComponents(GameObject go, ToggleObject annotationToggleObject)
        {

            switch (annotationToggleObject.predicate)
            {
                case string p when p.Contains("3d"):
                    var obstacle = go.AddComponent<NavMeshObstacle>();
                    obstacle.size = go.transform.localScale / 4;
                    break;

                case string p when p.StartsWith("act") || p.StartsWith("vfx") || p.Equals("image") || p.Equals("video"):
                    if (DisableBounding(annotationToggleObject)) return;
                    var boundingBox = go.AddComponent<BoundingBoxGenerator>();
                    boundingBox.CustomScaleHandlesConfiguration = Resources.Load<ScaleHandlesConfiguration>("Prefabs/CustomBoundingScaleHandlesConfiguration");
                    boundingBox.CustomRotationHandlesConfiguration = Resources.Load<RotationHandlesConfiguration>("Prefabs/CustomBoundingRotationHandlesConfiguration");
                    await boundingBox.AddBoundingBox(annotationToggleObject, Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsCalculationMethod.RendererOverCollider);

                    //disable rotation for image
                    if (DisableBoundingRotation(annotationToggleObject))
                    {
                        boundingBox.CustomRotationHandlesConfiguration.ShowHandleForX = false;
                        boundingBox.CustomRotationHandlesConfiguration.ShowHandleForY = false;
                        boundingBox.CustomRotationHandlesConfiguration.ShowHandleForZ = false;
                    }

                    break;
                default:
                    break;
            }

        }


        private static bool DisableBoundingRotation(ToggleObject annotationToggleObject)
        {
            return (annotationToggleObject.predicate.Equals("image") || annotationToggleObject.predicate.Equals("video"));
        }


        /// <summary>
        /// If bounding box needs to be disabled for some of the augmentations, add them in this method
        /// </summary>
        /// <param name="annotationToggleObject"></param>
        /// <returns></returns>
        private static bool DisableBounding(ToggleObject annotationToggleObject)
        {
            var boundingState = false;
            var annotationName = annotationToggleObject.predicate.ToLower();

            //add any other augmentation which need bounding box be disabled for, in this statment.
            if (annotationName == "act:measure")
                boundingState = true;

            return boundingState;
        }



        private static void DestroyPrefab(ToggleObject obj)
        {
            // For storing the gameobject to be destroyed.
            GameObject temp = null;
             
            // Do magic based on the object type.
            switch (obj.type)
            {
                case "tangible":

                    // Default path.
                    var path = $"{obj.id}/{obj.poi}/";

                    if (obj.id.Equals("ActionsViewport"))
                    {
                        path = "TaskListPanel/Symbols/";
                    }

                    if (obj.id.Equals("UserViewport"))
                    {
                        path = string.Empty;
                    }

                    // Check predicate.
                    switch (obj.predicate)
                    {
                        // Handle action symbols by default.
                        default:
                            temp = GameObject.Find(path + obj.predicate);
                            break;
                        case "label":
                            // Let's see if we have an object for deletion...
                            temp = GameObject.Find(path + "label_" + obj.text.Split(' ')[0]);
                            break;
                        case "3dmodel":
                        case "model":
                            // Let's see if we have an object for deletion...
                            temp = GameObject.Find(path + obj.predicate + "_" + obj.option);
                            break;
                        case "image":

                            // Get the last bit of the url.
                            var id = obj.url.Split('/')[obj.url.Split('/').Length - 1];

                            // Let's see if we have an object for deletion...
                            temp = GameObject.Find(path + obj.predicate + "_" + id);
                            break;
                        case "imagemarker":

                            Debug.Log("----------" + path );

                            GameObject.Find(path).GetComponentInChildren<ImageMarkerController>().platformOnDestroy();
                    break;
                        case "pickandplace":                       

                            // Let's see if we have an object for deletion...
                            temp = GameObject.Find(path + obj.predicate);
                            temp.GetComponent<PickAndPlaceController>().SavePositions();
                            
                            break;

                    }
                    //for all type of glyphs icons
                    if (obj.predicate.StartsWith("act") || obj.predicate.StartsWith("vfx") || obj.predicate.StartsWith("char") || obj.predicate.StartsWith("plugin"))
                        temp = GameObject.Find(path + obj.predicate);

                    break;

                default:
                    EventManager.DebugLog("ObjectFactory: Delete - Unknown type: " + obj.type);
                    break;
            }

            // If object exists...
            if (temp != null)
                Destroy(temp);
        }
    }
}