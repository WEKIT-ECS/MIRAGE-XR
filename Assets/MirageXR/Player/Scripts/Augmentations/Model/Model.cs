using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Siccity.GLTFUtility;

namespace MirageXR
{
    public class Model : MirageXRPrefab
    {
        private float startLoadTime = 0.0f;
        private ToggleObject myToggleObject;
        private Animation animation;


        public ToggleObject MyToggleObject => myToggleObject;

        public bool LoadingCompleted
        {
            get; private set;
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization successful.</returns>
        public override bool Init (ToggleObject obj)
        {
            myToggleObject = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty (obj.url))
            {
                Debug.Log ("Content URL not provided.");
                return false;
            }
             
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent (obj))
            {
                Debug.Log ("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            /* The following is commented out until there is a need to use local prefabs (from the resources folder)
             * 
            if (!Utilities.TryParseStringToVector3(obj.position, out var position)) 
                Debug.LogWarning($"ParseStringToVector3 failed; value {obj.position}");
            if (!Utilities.TryParseStringToQuaternion(obj.rotation, out var rotation))
                Debug.LogWarning($"ParseStringToQuaternion failed; value {obj.rotation}");
            
            transform.localPosition = position;
            transform.rotation = rotation;

            // Load from resources.
            if (obj.url.StartsWith("resources://"))
            {
                // Instantiate from resources.
                var prefab = Resources.Load<GameObject>(obj.option);
                if (prefab)
                {
                    var model = Instantiate(prefab, Vector3.zero, Quaternion.identity);

                    // Set name.
                    model.name = obj.option;

                    // Set parent.
                    model.transform.SetParent(transform);

                    // Set transform.
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localEulerAngles = Vector3.zero;
                    model.transform.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogError($"{obj.option} prefab doesn't exist");
                }
            }
            else
            {
                LoadModel (obj);
            }

            */


            LoadModel(obj);

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj))
                    return false;
            }

            

            // If all went well, return true.
            return true;
        }


        private void LoadModel(ToggleObject obj)
        {
            startLoadTime = Time.time;
            string loadPath = Path.Combine(ActivityManager.Instance.Path, obj.option, "scene.gltf");

            Debug.Log($"Loading model: {loadPath}");

            Importer.ImportGLTFAsync(loadPath, new ImportSettings(), OnFinishLoadingAsync);
        }

        private void OnFinishLoadingAsync(GameObject _model, AnimationClip[] clip)
        {
            if(this == null)
            {
                Destroy(_model);
                return;
            }

            Debug.Log("Imported " + _model.name + " in " + (Time.time - startLoadTime).ToString() + " seconds");

            Vector3 startPos = transform.position + transform.forward * -0.5f + transform.up * -0.1f;

            _model.transform.SetParent(transform);
            _model.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _model.transform.position = startPos;
            _model.transform.localRotation = Quaternion.identity;

            _model.name = myToggleObject.option;
            _model.transform.localRotation *= Quaternion.Euler(-90f, 0f, 0f);

            ConfigureModel(_model, clip);
            MoveAndScaleModel(_model);

            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();

            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);
            transform.parent.localRotation = Quaternion.Euler(GetPoiRotation(myPoiEditor));

            LoadingCompleted = true;

            // configure and play animation

            if (clip.Length > 0)
            {
                Debug.Log("Animation(s) found (" + clip.Length + ")...isLegacy? " + clip[0].legacy.ToString());

                animation = _model.AddComponent<Animation>();
                animation.AddClip(clip[0], "leaning");
                animation.playAutomatically = true;
                animation.clip = clip[0];
                animation.clip.legacy = true;
                animation.Play();
            }
        }

        private List<Bounds> colliders;

        private void ConfigureModel(GameObject _model, AnimationClip[] clips)
        {
            _model.AddComponent<Rigidbody>();
            _model.GetComponent<Rigidbody>().isKinematic = true;
            _model.SetActive(true);

            Renderer[] renderers = _model.GetComponentsInChildren<Renderer>();
            colliders = new List<Bounds>();

            // add colliders to meshes
            foreach (Renderer r in renderers)
            {
                GameObject g = r.gameObject;

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

        private void AddCapsuleCollidersToPatient(Transform rootBone)
        {
            for (int child = 0;  child < rootBone.childCount; child++)
            {
                Transform childBone = rootBone.GetChild(child);
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
            Vector3 colliderSize = new Vector3(0f, 0f, 0f);
            int largestColliderIndex = 0;
            foreach (Bounds meshColl in colliders)
            {
                if (meshColl.size.sqrMagnitude > colliderSize.sqrMagnitude)
                {
                    colliderSize = meshColl.size;
                    largestColliderIndex = colliders.IndexOf(meshColl);
                }
            }

            Debug.Log("largest collider: " + largestColliderIndex.ToString() + " (" + colliderSize.ToString("F4") + ")");

            // set magnification and translation factors based on gltf info.
            float magnificationFactor = 0.5f / colliderSize.magnitude;

            if (float.IsInfinity(magnificationFactor))
            {
                print("infinite mag factor! reverts to '0.5'");
                magnificationFactor = 0.5f;
            }

            PoiEditor myPoiEditor = GetComponentInParent<PoiEditor>();


            if (Utilities.TryParseStringToVector3(myPoiEditor.GetMyPoi().scale, out Vector3 newPoiScale))
            {
                if (newPoiScale.x.Equals(newPoiScale.y) && newPoiScale.y.Equals(newPoiScale.z))
                {
                    magnificationFactor *= newPoiScale.x;
                }
            }


            myPoiEditor.ModelMagnification = magnificationFactor;
            Debug.Log(modelToAdjust.name + " has file mag. factor " + magnificationFactor.ToString("F4"));

            modelToAdjust.transform.localScale *= myPoiEditor.ModelMagnification;
            modelToAdjust.transform.localPosition = Vector3.zero;
        }


    }
}