using i5.Toolkit.Core.VerboseLogging;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MirageXR
{
    public class Model : MirageXRPrefab
    {
        private static ActivityManager _activityManager => RootObject.Instance.activityManager;

        private float startLoadTime = 0.0f;
        private ToggleObject _obj;
        private Animation animation;

        public ToggleObject MyToggleObject => _obj;

        private ObjectManipulator objectManipulator;

        private void Awake()
        {
            objectManipulator = gameObject.GetComponent<ObjectManipulator>() ? gameObject.GetComponent<ObjectManipulator>() : gameObject.AddComponent<ObjectManipulator>();
        }

        private void Start()
        {
            Subscribe();
        }

        private void OnDestroy()
        {
            var poiEditor = GetComponentInParent<PoiEditor>();

            if (poiEditor)
            {
                poiEditor.EnableBoundsControl(false);
            }

            UnSubscribe();
        }

        private void Subscribe()
        {
            EventManager.OnAugmentationDeleted += DeleteModelData;
        }

        private void UnSubscribe()
        {
            EventManager.OnAugmentationDeleted -= DeleteModelData;
        }

        public bool LoadingCompleted
        {
            get; private set;
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization successful.</returns>
        public override bool Init(ToggleObject obj)
        {
            _obj = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            LoadModel(obj);

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj)) return false;
            }

            OnLock(_obj.poi, _obj.positionLock);
            EventManager.OnAugmentationLocked += OnLock;

            // If all went well, return true.

            return true;
        }

        private void LoadModel(ToggleObject obj)
        {
            startLoadTime = Time.time;

            obj.option = ZipUtilities.CheckFileForIllegalCharacters(obj.option);

            var loadPath = Path.Combine(RootObject.Instance.activityManager.ActivityPath, obj.option, "scene.gltf");

            Debug.LogTrace($"Loading model: {loadPath}");

            Importer.ImportGLTFAsync(loadPath, new ImportSettings(), OnFinishLoadingAsync);
        }

        private void OnFinishLoadingAsync(GameObject model, AnimationClip[] clip)
        {
            if (this == null)
            {
                Destroy(model);
                return;
            }

            Debug.LogTrace($"Imported {model.name} in {Time.time - startLoadTime} seconds");

            var startPos = transform.position + transform.forward * -0.5f + transform.up * -0.1f;

            model.transform.SetParent(transform);
            model.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            model.transform.position = startPos;
            model.transform.localRotation = Quaternion.identity;

            model.name = _obj.option;
            model.transform.localRotation *= Quaternion.Euler(-90f, 0f, 0f);

            ConfigureModel(model, clip);
            MoveAndScaleModel(model);

            var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();

            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);
            transform.parent.localEulerAngles = GetPoiRotation(myPoiEditor);

            LoadingCompleted = true;

            // configure and play animation

            if (clip.Length > 0)
            {
                Debug.LogDebug($"Animation(s) found ({clip.Length})...isLegacy? {clip[0].legacy}");

                animation = model.AddComponent<Animation>();
                animation.AddClip(clip[0], "leaning");
                animation.playAutomatically = true;
                animation.clip = clip[0];
                animation.clip.legacy = true;
                animation.Play();
            }

            InitManipulators();
        }

        private void InitManipulators()
        {
            var poiEditor = GetComponentInParent<PoiEditor>();

            if (poiEditor)
            {
                poiEditor.EnableBoundsControl(!_obj.positionLock);
            }

            var gridManager = RootObject.Instance.gridManager;
            var objectManipulator = GetComponentInParent<ObjectManipulator>();
            if (objectManipulator)
            {
                objectManipulator.OnManipulationStarted.RemoveAllListeners();
                objectManipulator.OnManipulationEnded.RemoveAllListeners();
                objectManipulator.OnManipulationStarted.AddListener(eventData => gridManager.onManipulationStarted(eventData.ManipulationSource));
                objectManipulator.OnManipulationEnded.AddListener(eventData =>
                {
                    gridManager.onManipulationEnded(eventData.ManipulationSource);
                    poiEditor.OnChanged();
                });
            }

            var boundsControl = GetComponentInParent<BoundsControl>();
            if (boundsControl)
            {
                boundsControl.RotateStarted.AddListener(() => gridManager.onRotateStarted?.Invoke(boundsControl.Target));
                boundsControl.RotateStopped.AddListener(() =>
                {
                    gridManager.onRotateStopped?.Invoke(boundsControl.Target);
                    poiEditor.OnChanged();
                });
                boundsControl.ScaleStarted.AddListener(() => gridManager.onScaleStarted?.Invoke(boundsControl.Target));
                boundsControl.ScaleStopped.AddListener(() =>
                {
                    gridManager.onScaleStopped?.Invoke(boundsControl.Target);
                    poiEditor.OnChanged();
                });
                boundsControl.TranslateStarted.AddListener(() => gridManager.onTranslateStarted?.Invoke(boundsControl.Target));
                boundsControl.TranslateStopped.AddListener(() =>
                {
                    gridManager.onTranslateStopped?.Invoke(boundsControl.Target);
                    poiEditor.OnChanged();
                });

                boundsControl.enabled = !_obj.positionLock;
            }
        }

        private List<Bounds> colliders;

        private void ConfigureModel(GameObject _model, AnimationClip[] clips)
        {
            _model.AddComponent<Rigidbody>();
            _model.GetComponent<Rigidbody>().isKinematic = true;
            _model.SetActive(true);

            var renderers = _model.GetComponentsInChildren<Renderer>();
            colliders = new List<Bounds>();

            // add colliders to meshes
            foreach (var r in renderers)
            {
                var g = r.gameObject;

                if (!g.GetComponent<MeshCollider>())
                {

                    if (g.GetComponent<SkinnedMeshRenderer>())
                    {
                        var rootBone = g.GetComponent<SkinnedMeshRenderer>().rootBone.transform;

                        // for skinned mesh renderers, add capsule colliders to main bones (2 levels) and scale list entry
                        AddCapsuleCollidersToPatient(rootBone);
                        Bounds capsuleBounds = rootBone.GetChild(0).GetComponent<CapsuleCollider>().bounds;
                        capsuleBounds.size *= 2.5f;
                        colliders.Add(capsuleBounds);
                    }
                    else
                    {
                        // for all other types, add a mesh collider
                        var newCollider = g.AddComponent<MeshCollider>();
                        colliders.Add(newCollider.bounds);
                    }

                }
                else
                {
                    colliders.Add(g.GetComponent<MeshCollider>().bounds);
                }
            }
        }

        private static void AddCapsuleCollidersToPatient(Transform rootBone)
        {
            for (int child = 0; child < rootBone.childCount; child++)
            {
                var childBone = rootBone.GetChild(child);
                var capcoll = childBone.gameObject.AddComponent<CapsuleCollider>();
                capcoll.center = new Vector3(0f, 0.75f, 0f);
                capcoll.radius = 0.5f;
                capcoll.height = 2f;

                for (int grandchild = 0; grandchild < childBone.childCount; grandchild++)
                {
                    Transform grandchildBone = childBone.GetChild(grandchild);
                    var capcoll2 = grandchildBone.gameObject.AddComponent<CapsuleCollider>();
                    capcoll2.center = new Vector3(0f, 1f, 0f);
                    capcoll2.radius = 0.5f;
                    capcoll2.height = 1.5f;
                }
            }
        }

        private void MoveAndScaleModel(GameObject modelToAdjust)
        {
            // get maximum extents
            var colliderSize = new Vector3(0f, 0f, 0f);
            int largestColliderIndex = 0;
            foreach (Bounds meshColl in colliders)
            {
                if (meshColl.size.sqrMagnitude > colliderSize.sqrMagnitude)
                {
                    colliderSize = meshColl.size;
                    largestColliderIndex = colliders.IndexOf(meshColl);
                }
            }

            Debug.LogDebug($"largest collider: {largestColliderIndex} ({colliderSize.ToString("F4")})");

            // set magnification and translation factors based on gltf info.
            float magnificationFactor = 0.5f / colliderSize.magnitude;

            if (float.IsInfinity(magnificationFactor))
            {
                print("infinite mag factor! reverts to '0.5'");
                magnificationFactor = 0.5f;
            }

            var myPoiEditor = GetComponentInParent<PoiEditor>();

            if (Utilities.TryParseStringToVector3(myPoiEditor.GetMyPoi().scale, out var newPoiScale))
            {
                if (newPoiScale.x.Equals(newPoiScale.y) && newPoiScale.y.Equals(newPoiScale.z))
                {
                    magnificationFactor *= newPoiScale.x;
                }
            }

            myPoiEditor.ModelMagnification = magnificationFactor;
            Debug.LogDebug($"{modelToAdjust.name} has file mag. factor {magnificationFactor:F4}");

            modelToAdjust.transform.localScale *= myPoiEditor.ModelMagnification;
            modelToAdjust.transform.localPosition = Vector3.zero;
        }

        private void DeleteModelData(ToggleObject augmentation)
        {
            if (augmentation != _obj) return;

            // check for existing model folder and delete if necessary
            var arlemPath = RootObject.Instance.activityManager.ActivityPath;
            string folderName = augmentation.option;
            string modelFolderPath = Path.Combine(arlemPath, folderName);

            if (Directory.Exists(modelFolderPath))
            {
                Debug.LogTrace("found model folder (" + modelFolderPath + "). Deleting...");
                Utilities.DeleteAllFilesInDirectory(modelFolderPath);
                Directory.Delete(modelFolderPath);
            }

            foreach (var pick in FindObjectsOfType<Pick>())
            {
                foreach (var child in pick.GetComponentsInChildren<Transform>())
                {
                    if (child.name.EndsWith(augmentation.poi))
                    {
                        Destroy(child.gameObject);
                        pick.ArrowRenderer.enabled = true;
                    }
                }
            }
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                objectManipulator.enabled = !_obj.positionLock;

                var poiEditor = GetComponentInParent<PoiEditor>();

                if (poiEditor)
                {
                    poiEditor.IsLocked(_obj.positionLock);

                    if(poiEditor.transform.GetComponent<BoundsControl>() && _activityManager.EditModeActive)
                    {
                        poiEditor.EnableBoundsControl(!_obj.positionLock);
                    }
                }

                if (gameObject.GetComponent<ObjectManipulator>())
                {
                    gameObject.GetComponent<ObjectManipulator>().enabled = !_obj.positionLock;
                }
            }
        }

        private void OnDisable()
        {
            EventManager.OnAugmentationLocked -= OnLock;
        }

        public override void Delete()
        {

        }
    }
}