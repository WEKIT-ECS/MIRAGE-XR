using System;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirageXR
{
    using BoundsCalculationMethod = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsCalculationMethod;

    public class ObjectFactory : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        private void OnEnable()
        {
            // Register to event manager events.
            LearningExperienceEngine.EventManager.OnToggleObject += Toggle;
        }

        private void OnDisable()
        {
            // Unregister from event manager events.
            LearningExperienceEngine.EventManager.OnToggleObject -= Toggle;
        }

        /// <summary>
        /// Toggle on/off action toggle object.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <param name="isActivating">Toggle state.</param>
        private static void Toggle(LearningExperienceEngine.ToggleObject obj, bool isActivating)
        {
            // Do magic based on the object type.
            switch (obj.type)
            {
                case LearningExperienceEngine.ActionType.Tangible:
                    {
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
                                            obj.option = obj.predicate.Replace("3d:", "");

                                            obj.url = "resources://" + obj.predicate;

                                            ActivatePrefab("Model/ModelPrefab", obj);
                                        }
                                        else if (obj.predicate.StartsWith("act:"))
                                        {
                                            obj.option = obj.predicate.Replace("act:", "");

                                            obj.url = "resources://" + obj.predicate;

                                            var glyphModel = $"Glyphs/{obj.option}";
                                            ActivatePrefab(glyphModel, obj);
                                        }
                                        else if (obj.predicate.StartsWith("effect:") || obj.predicate.StartsWith("vfx:"))
                                        {
                                            obj.predicate = obj.predicate.Replace("vfx:", "effect:");
                                            obj.option = obj.predicate.Replace("effect:", "");

                                            obj.url = "resources://" + obj.predicate;

                                            var vfxModel = $"VFX/{obj.option}";
                                            ActivatePrefab(vfxModel, obj);
                                        }
                                        else if (obj.predicate.StartsWith("char:"))
                                        {
                                            obj.option = obj.predicate.Replace("char:", "");

                                            //obj.predicate = "character";
                                            obj.url = "resources://" + obj.predicate;


                                            var charModel = $"Characters/{obj.option}";
                                            ActivatePrefab(charModel, obj);
                                        }
                                        else if (obj.predicate.StartsWith("plugin:"))
                                        {
                                            obj.option = "PluginControllerPrefab";

                                            ActivatePrefab(obj.option, obj);
                                        }
                                        // True and tested 2D symbols.
                                        else
                                        {
                                            ActivatePrefab("SymbolPrefab", obj);
                                        }
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
                                        else if (obj.predicate.StartsWith("effect:"))
                                        {
                                            obj.option = obj.predicate.Replace("effect:", "");

                                            // obj.predicate = "glyph";
                                            obj.url = "resources:// " + obj.predicate;

                                            DestroyPrefab(obj);
                                        }
                                        /*else if (obj.predicate.StartsWith("char:"))
                                        {
                                            //obj.option = obj.predicate.Replace("char:", "");

                                            //obj.predicate = "character";
                                            obj.url = "resources:// " + obj.predicate;

                                            DestroyPrefab(obj);
                                        }*/
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
                                {
                                    ActivatePrefab("LabelPrefab", obj);
                                }
                                else
                                {
                                    DestroyPrefab(obj);
                                }

                                break;

                            // Handle detect type.
                            case "detect":
                                if (isActivating)
                                {
                                    ActivatePrefab("DetectPrefab", obj);
                                }
                                else
                                {
                                    DestroyPrefab(obj);
                                }

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
                                    Debug.LogInfo("hands activated");
                                // ActivatePrefab("HandsPrefab", obj);
                                // else
                                //  DestroyPrefab(obj);
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

                            // Show card button type.
                            case "menutoggle":
                                if (isActivating)
                                    ActivatePrefab("ShowCardsPrefab", obj);
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
                                if (isActivating)
                                    ActivatePrefab("DrawingPrefab", obj);
                                else
                                    DestroyPrefab(obj);
                                break;
                            case "virtualinstructor":
                                if (isActivating)
                                {
                                    InstructorContentData dataModel;
                                    try
                                    {
                                        dataModel = JsonConvert.DeserializeObject<InstructorContentData>(obj.option);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(e);
                                        return;
                                    }
                                    var charModel = $"Instructors/{dataModel.CharacterName}_instructor";
                                    ActivatePrefab(charModel, obj);
                                }
                                else
                                    DestroyPrefab(obj);
                                break;
                        }

                        break;
                    }
                default:
                    {
                        Debug.LogError("ObjectFactory: Toggle - Unknown type: " + obj.type);
                        break;
                    }
            }
        }

        private static async void ActivatePrefab(string prefab, LearningExperienceEngine.ToggleObject obj)
        {
            var actionList = activityManager.ActionsOfTypeAction;
            GameObject temp = null;
            var activeActionIndex = actionList.IndexOf(activityManager.ActiveAction);

            // if we are in the active step and the annotation exists in this step
            if (actionList[activeActionIndex] == activityManager.ActiveAction
                && actionList[activeActionIndex].enter.activates.Find(p => p.poi == obj.poi) != null)
            {
                var objIfExist = GameObject.Find(obj.poi);

                if (objIfExist)
                {
                    Debug.Log("obj.poi exists: found content augmentation " + obj.poi );
                    var annotationChildren = objIfExist.transform.childCount;

                    // prevent duplication
                    if (annotationChildren > 0)
                    {
                        DestroyPrefab(obj);
                    }
                }

                // Get the prefab from the Adressables references

                // old code
                //var prefabInAddressable = await ReferenceLoader.GetAssetReferenceAsync<GameObject>(prefab);
                //// if the prefab reference has been found successfully
                //if (prefabInAddressable != null)
                //{
                //    temp = Instantiate(prefabInAddressable, Vector3.zero, Quaternion.identity);
                //    _ = AddExtraComponents(temp, obj);
                //}

                // new code
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefab);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject res = handle.Result;
                    temp = UnityEngine.Object.Instantiate(res, Vector3.zero, Quaternion.identity);
                    _ = AddExtraComponents(temp, obj);
                }
                else
                {
                    Debug.LogError("FATAL ERROR: Could not instantiate ContentAugmentation prefab " + prefab);
                }

            }

            if (temp == null)
            {
                return;
            }

            // Try to initialize and if it fails, debug and destroy the object.
            var miragePrefab = temp.GetComponent<MirageXRPrefab>();
            if (miragePrefab && !miragePrefab.Init(obj)) // also calls MirageXRPrefab.SetParent()
            {
                Debug.LogError($"Couldn't create the {prefab}. {obj.id}/{obj.poi}/{obj.predicate}");
                temp.GetComponent<MirageXRPrefab>().Delete();
            }
        }


        /// <summary>
        /// All post creation component will be added to the augmentation objects in this method
        /// </summary>
        /// <param name="go"></param>
        /// <param name="annotationToggleObject"></param>
        /// <returns></returns>
        private static async Task AddExtraComponents(GameObject go, LearningExperienceEngine.ToggleObject annotationToggleObject)
        {
            switch (annotationToggleObject.predicate)
            {
                case { } p when p.Contains("3d"):
                {
                    var obstacle = go.AddComponent<NavMeshObstacle>();
                    obstacle.size = go.transform.localScale / 4;
                    break;
                }

                case { } p when p.StartsWith("act") || p.StartsWith("effect") || p.Equals("image") || p.Equals("video"):
                {
                    if (DisableBounding(annotationToggleObject))
                    {
                        return;
                    }

                    await AddBoundingBox(go, annotationToggleObject);

                    break;
                }
            }
        }


        /// <summary>
        /// Add bounding box to the augmentation that need that
        /// </summary>
        /// <param name="go"></param>
        /// <param name="annotationToggleObject"></param>
        /// <returns></returns>
        private static async Task AddBoundingBox(GameObject go, LearningExperienceEngine.ToggleObject annotationToggleObject)
        {
            var boundingBox = go.AddComponent<BoundingBoxGenerator>();
            boundingBox.CustomScaleHandlesConfiguration = Resources.Load<ScaleHandlesConfiguration>("Prefabs/CustomBoundingScaleHandlesConfiguration");
            boundingBox.CustomRotationHandlesConfiguration = Resources.Load<RotationHandlesConfiguration>("Prefabs/CustomBoundingRotationHandlesConfiguration");
            await boundingBox.AddBoundingBox(annotationToggleObject, BoundsCalculationMethod.RendererOverCollider, false, true, BoundingRotationType.ALL, true);

            // disable rotation for image
            if (DisableBoundingRotation(annotationToggleObject))
            {
                boundingBox.CustomRotationHandlesConfiguration.ShowHandleForX = false;
                boundingBox.CustomRotationHandlesConfiguration.ShowHandleForY = false;
                boundingBox.CustomRotationHandlesConfiguration.ShowHandleForZ = false;
            }

            await Task.CompletedTask;
        }


        private static IEnumerator WaitForParent(GameObject gameObject, System.Action callback)
        {
            yield return new WaitWhile(() => gameObject.transform.parent);
            callback?.Invoke();
        }

        private static bool DisableBoundingRotation(LearningExperienceEngine.ToggleObject annotationToggleObject)
        {
            return annotationToggleObject.predicate.Equals("image") || annotationToggleObject.predicate.Equals("video");
        }

        /// <summary>
        /// If bounding box needs to be disabled for some of the augmentations, add them in this method
        /// </summary>
        /// <param name="annotationToggleObject"></param>
        /// <returns></returns>
        private static bool DisableBounding(LearningExperienceEngine.ToggleObject annotationToggleObject)
        {
            var boundingState = false;
            var annotationName = annotationToggleObject.predicate.ToLower();

            // add any other augmentation which need bounding box be disabled for, in this statement.
            if (annotationName == "act:measure")
            {
                boundingState = true;
            }

            return boundingState;
        }

        private static void DestroyPrefab(LearningExperienceEngine.ToggleObject obj)
        {
            GameObject temp = null;

            if (obj.type == LearningExperienceEngine.ActionType.Tangible)
            {
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

                switch (obj.predicate)
                {
                    case "label":
                        {
                            temp = GameObject.Find($"{path}label_{obj.text.Split(' ')[0]}");
                            break;
                        }
                    case "3dmodel":
                    case "model":
                        {
                            temp = GameObject.Find($"{path}{obj.predicate}_{obj.option}");
                            break;
                        }
                    case "image":
                        {
                            var id = obj.url.Split('/')[obj.url.Split('/').Length - 1];
                            temp = GameObject.Find($"{path}{obj.predicate}_{id}");
                            break;
                        }
                    case "imagemarker":
                        {
                            var id = obj.url.Split('/')[obj.url.Split('/').Length - 1];
                            temp = GameObject.Find($"{path}{obj.predicate}_{id}");
                            break;
                        }
                    case "pickandplace":
                        {
                            temp = GameObject.Find(path + obj.predicate);
                            break;
                        }
                    case "virtualinstructor":
                    {
                            try
                            {
                                var data = JsonConvert.DeserializeObject<InstructorContentData>(obj.option);
                                temp = GameObject.Find(path + data.CharacterName);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                            
                            break;
                        }
                    default:
                        {
                            temp = GameObject.Find(path + obj.predicate);
                            break;
                        }
                }

                //for all type of glyphs icons
                if (obj.predicate.StartsWith("act") || obj.predicate.StartsWith("effect") ||
                    obj.predicate.StartsWith("char") || obj.predicate.StartsWith("plugin"))
                {
                    temp = GameObject.Find(path + obj.predicate);
                }
            }
            else
            {
                Debug.LogError("ObjectFactory: Delete - Unknown type: " + obj.type);
            }

            if (temp != null)
            {
                Destroy(temp);
            }
        }
    }
}