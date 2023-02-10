using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Siccity.GLTFUtility;

namespace MirageXR
{
    public class Model : MirageXRPrefab
    { 
        private static ActivityManager _activityManager => RootObject.Instance.activityManager;

        private float startLoadTime = 0.0f;
        private ToggleObject myToggleObject;
        private Animation animation;

        public ToggleObject MyToggleObject => myToggleObject;


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
            myToggleObject = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.Log("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
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

            // If all went well, return true.
            
            return true;
        }

        private void LoadModel(ToggleObject obj)
        {
            startLoadTime = Time.time;
            Debug.Log("obj.option = " + obj.option);

            obj.option = ZipUtilities.CheckFileForIllegalCharacters(obj.option);

            Debug.Log("File name = " + obj.option);

            var loadPath = Path.Combine(RootObject.Instance.activityManager.ActivityPath, obj.option, "scene.gltf");

            Debug.Log($"Loading model: {loadPath}");

            Importer.ImportGLTFAsync(loadPath, new ImportSettings(), OnFinishLoadingAsync);
        }

        private void OnFinishLoadingAsync(GameObject model, AnimationClip[] clip)
        {
            if (this == null)
            {
                Destroy(model);
                return;
            }

            Debug.Log($"Imported {model.name} in {Time.time - startLoadTime} seconds");

            var startPos = transform.position + transform.forward * -0.5f + transform.up * -0.1f;

            model.transform.SetParent(transform);
            model.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            model.transform.position = startPos;
            model.transform.localRotation = Quaternion.identity;

            model.name = myToggleObject.option;
            model.transform.localRotation *= Quaternion.Euler(-90f, 0f, 0f);

            ConfigureModel(model, clip);
            MoveAndScaleModel(model);

            var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();

            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);
            transform.parent.localRotation = Quaternion.Euler(GetPoiRotation(myPoiEditor));

            LoadingCompleted = true;

            // configure and play animation

            if (clip.Length > 0)
            {
                Debug.Log($"Animation(s) found ({clip.Length})...isLegacy? {clip[0].legacy}");

                animation = model.AddComponent<Animation>();
                animation.AddClip(clip[0], "leaning");
                animation.playAutomatically = true;
                animation.clip = clip[0];
                animation.clip.legacy = true;
                animation.Play();
            }

            var poiEditor = GetComponentInParent<PoiEditor>();

            if (poiEditor)
            {
                poiEditor.EnableBoundsControl(true);
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

            Debug.Log($"largest collider: {largestColliderIndex} ({colliderSize.ToString("F4")})");

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
            Debug.Log($"{modelToAdjust.name} has file mag. factor {magnificationFactor:F4}");

            modelToAdjust.transform.localScale *= myPoiEditor.ModelMagnification;
            modelToAdjust.transform.localPosition = Vector3.zero;
        }

        private void DeleteModelData(ToggleObject augmentation)
        {
            if (augmentation != myToggleObject) return;

            // check for existing model folder and delete if necessary
            var arlemPath = RootObject.Instance.activityManager.ActivityPath;
            string folderName = augmentation.option;
            string modelFolderPath = Path.Combine(arlemPath, folderName);

            if (Directory.Exists(modelFolderPath))
            {
                Debug.Log("found model folder (" + modelFolderPath + "). Deleting...");
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
    }
}