using LearningExperienceEngine;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MirageXR
{
    public class Model : MirageXRPrefab
    {
        private const string GLTF_NAME = "scene.gltf";
        private const float LIBRARY_MODEL_SCALE = 2f;

        private static LearningExperienceEngine.ActivityManager _activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        private LearningExperienceEngine.ToggleObject _obj;
        private Animation _animation;
        private GltfImport _gltf;

        public LearningExperienceEngine.ToggleObject MyToggleObject => _obj;

        private List<Bounds> _colliders;
        private bool _isLibraryModel;

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

            _gltf?.Dispose();
        }

        private void Subscribe()
        {
            LearningExperienceEngine.EventManager.OnAugmentationDeleted += DeleteModelData;
        }

        private void UnSubscribe()
        {
            LearningExperienceEngine.EventManager.OnAugmentationDeleted -= DeleteModelData;
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
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;

            _isLibraryModel = _obj.text.Equals(ModelLibraryManager.LibraryKeyword);
            
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;

            if (_isLibraryModel)
            {
                LoadLibraryModel($"Library/{obj.option}").AsAsyncVoid();
            }
            else
            {
                LoadGltf(obj).AsAsyncVoid();
            }

            if (!obj.id.Equals("UserViewport"))
            {
                if (!SetGuide(obj))
                {
                    return false;
                }
            }

            OnLock(_obj.poi, _obj.positionLock);
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            return true;
        }

        private async Task<bool> LoadGltf(LearningExperienceEngine.ToggleObject content)
        {
            content.option = ZipUtilities.RemoveIllegalCharacters(content.option);
            var loadPath = Path.Combine(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath, content.option, GLTF_NAME);

            ImportSettings importSettings = new()
            {
                AnimationMethod = AnimationMethod.Legacy
            };

            _gltf = new GltfImport();
            var success = await _gltf.Load(new Uri(loadPath), importSettings);
            var instantiator = new GameObjectInstantiator(_gltf, transform);
            if (success)
            {
                success = await _gltf.InstantiateMainSceneAsync(instantiator); // was: transform
            }

            if (!success)
            {
                UnityEngine.Debug.LogError($"Can't load model on the path {loadPath}");
                return false;
            }

            var legacyAnimation = instantiator.SceneInstance.LegacyAnimation;
            if (legacyAnimation != null)
            {
                Debug.Log("playing animation (with data from gltfast scene instantiator).");
                var clips = _gltf.GetAnimationClips();
                if (clips is { Length: > 0 } && clips[0] is not null)
                {
                    legacyAnimation.clip = clips[0];
                    legacyAnimation.clip.wrapMode = WrapMode.Loop;
                    legacyAnimation.clip.legacy = true;
                    legacyAnimation.clip.EnsureQuaternionContinuity();
                    legacyAnimation.playAutomatically = true;
                    legacyAnimation.Play(clips[0].name);
                }
            }

            OnFinishLoading(transform.Find("Sketchfab_model").gameObject); // , _gltf.GetAnimationClips()
            return true;
        }

        private void OnFinishLoading(GameObject model) // , AnimationClip[] clip
        {
            if (this == null)
            {
                Destroy(model);
                return;
            }
            
            var modelTransform = transform;
            var startPos = modelTransform.position + modelTransform.forward * -0.5f + modelTransform.up * -0.1f;
            
            model.transform.SetParent(modelTransform);
            //model.name = _obj.option;

            //Do not manipulate the library models at the start
            if (!_isLibraryModel)
            {
                model.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                model.transform.position = startPos;
                model.transform.localRotation = Quaternion.identity;
            }
                        
            model.transform.localRotation *= Quaternion.Euler(-90f, 0f, 0f);

            if (!_isLibraryModel)
            {
                ConfigureModel(model); // , clip
            }

            MoveAndScaleModel(model);
            
            var parent = transform.parent;
            var myPoiEditor = parent.gameObject.GetComponent<PoiEditor>();

            var defaultScale = _isLibraryModel ? new Vector3(LIBRARY_MODEL_SCALE, LIBRARY_MODEL_SCALE, LIBRARY_MODEL_SCALE) : Vector3.one;
            parent.localScale = GetPoiScale(myPoiEditor, defaultScale);
            parent.localEulerAngles = GetPoiRotation(myPoiEditor);

            LoadingCompleted = true;

            /*
            // configure and play animation
            if (clip is { Length: > 0 })
            {
                Debug.LogDebug($"Animation(s) found ({clip.Length})...isLegacy? {clip[0].legacy}, clip[0]: {clip[0].name}");

                _animation = model.AddComponent<Animation>();
                _animation.AddClip(clip[0], clip[0].name);
                _animation.clip = _animation.GetClip(clip[0].name);
                _animation.clip.wrapMode = UnityEngine.WrapMode.Loop;
                _animation.clip.legacy = true;
                _animation.clip.EnsureQuaternionContinuity();
                //_animation.clip.SetCurve("", typeof(Camera), "field of view", AnimationCurve.Linear(0.0f, 60.0f, 10.0f, 90.0f));
                _animation.playAutomatically = true;
                _animation.Play(clip[0].name);
                Debug.Log("Playing? " + _animation.isPlaying);
            }
            */
            
            InitManipulators();
        }

        private async Task LoadLibraryModel(string libraryModelPrefabName)
        {
            try
            {
                var model = await Addressables.LoadAssetAsync<GameObject>(libraryModelPrefabName);
                var instantiatedModel = Instantiate((GameObject)model, transform);
                OnFinishLoading(instantiatedModel); // , null
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load LibraryModel from Addressables\nerror:{e}");
            }
        }

        private void InitManipulators()
        {
            var poiEditor = GetComponentInParent<PoiEditor>();

            if (poiEditor)
            {
                poiEditor.EnableBoundsControl(!_obj.positionLock);
            }
            
            var gridManager = RootObject.Instance.GridManager;
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
            } else
            {
                Debug.LogWarning("[Model] Could not find boundary box control of the model.");
            }
            /*
            */
        }

        private void ConfigureModel(GameObject model) // , AnimationClip[] clips
        {
            var rb = model.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            model.SetActive(true);

            var renderers = model.GetComponentsInChildren<Renderer>();
            _colliders = new List<Bounds>();
            
            // add colliders to meshes
            foreach (var r in renderers)
            {
                var g = r.gameObject;

                g.TryGetComponent<MeshCollider>(out var meshCollider);
                if (!meshCollider)
                {
                    g.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer);
                    if (skinnedMeshRenderer && skinnedMeshRenderer.rootBone)
                    {
                        var rootBone = skinnedMeshRenderer.rootBone.transform;

                        // for skinned mesh renderers, add capsule colliders to main bones (2 levels) and scale list entry
                        AddCapsuleCollidersToPatient(rootBone);
                        Bounds capsuleBounds = rootBone.GetChild(0).GetComponent<CapsuleCollider>().bounds;
                        capsuleBounds.size *= 2.5f;
                        _colliders.Add(capsuleBounds);
                    }
                    else
                    {
                        if (IsMeshValid(r.GetComponent<MeshFilter>()))
                        {
                            var newCollider = g.AddComponent<MeshCollider>();
                            _colliders.Add(newCollider.bounds);
                        }
                    }
                }
                else
                {
                    _colliders.Add(meshCollider.bounds);
                }
            }
        }

        private static bool IsMeshValid(MeshFilter meshFilter)
        {
            try
            {
                var value = UnityEngine.Debug.unityLogger.logEnabled;
                UnityEngine.Debug.unityLogger.logEnabled = false;
                var triangles = meshFilter.mesh.GetTriangles(0);
                UnityEngine.Debug.unityLogger.logEnabled = value;
                return triangles.Length != 0;
            }
            catch (Exception)
            {
                return false;
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
                    var collider = grandchildBone.gameObject.AddComponent<CapsuleCollider>();
                    collider.center = new Vector3(0f, 1f, 0f);
                    collider.radius = 0.5f;
                    collider.height = 1.5f;
                }
            }
        }

        private void MoveAndScaleModel(GameObject modelToAdjust)
        {
            // get maximum extents
            var colliderSize = new Vector3(0f, 0f, 0f);
            int largestColliderIndex = 0;
            
            if (_colliders != null)
            {
                foreach (Bounds meshColl in _colliders)
                {
                    if (meshColl.size.sqrMagnitude > colliderSize.sqrMagnitude)
                    {
                        colliderSize = meshColl.size;
                        largestColliderIndex = _colliders.IndexOf(meshColl);
                    }
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

        private void DeleteModelData(LearningExperienceEngine.ToggleObject augmentation)
        {
            if (augmentation != _obj) return;

            // check for existing model folder and delete if necessary
            var arlemPath = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath;
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
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }

        public override void Delete()
        {

        }
    }
}