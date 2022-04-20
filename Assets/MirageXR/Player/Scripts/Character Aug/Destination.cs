using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace MirageXR
{
    public class Destination : MonoBehaviour
    {

        [SerializeField] private TextMesh number;
        [SerializeField] private GameObject pointer;

        private BoundsControl boundsControl;

        public TextMesh GetNumber()
        {
            return number;
        }

        public CharacterController MyCharacter
        {
            get; set;
        }

        private void Awake()
        {
            if (GetComponentInChildren<Canvas>().worldCamera != null)
                GetComponentInChildren<Canvas>().worldCamera = null;
        }


        void Start()
        {
            StartCoroutine(Init());

            boundsControl = GetComponent<BoundsControl>();

        }


        void ManipulationStarted()
        {
            MyCharacter.AnyNodeMoving = true;
        }


        void ManipulationEnded()
        {
            MyCharacter.AnyNodeMoving = false;
            MyCharacter.AnimationClipPlaying(false);

        }


        //bring the model a bit up to avoinding it to fall into the floor
        private void AvoidSpatialCover()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, LayerMask.NameToLayer("Spatial Awareness")))
                if (Mathf.Abs(hit.point.y - transform.position.y) < 0.01f)
                    gameObject.transform.localPosition += new Vector3(0, 0.2f, 0);

        }


        IEnumerator Init()
        {
            while (MyCharacter == null)
            {
                yield return null;
            }

            if (MyCharacter && MyCharacter.MovementType == "inplace")
            {
                transform.Find("Canvas").gameObject.SetActive(false);
            }
            else
            {
                if (MyCharacter && MyCharacter.Destinations != null)
                    UpdateIndexNumber();
            }

            transform.localScale = MyCharacter.transform.localScale / 5;

            GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener(delegate { ManipulationStarted(); });
            GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { ManipulationEnded(); });
            var boundsControl = GetComponent<BoundsControl>();
            boundsControl.ScaleStopped.AddListener(OnScaling);
            GetComponent<BoundingBoxGenerator>().OnlyRotateAround(boundsControl, BoundingRotationType.Y);
        }

        private void OnScaling()
        {
            MyCharacter.transform.localScale = transform.localScale * 5;

            //resize all other nodes as this one
            MyCharacter.Destinations.ForEach(d => { if (d != gameObject) d.transform.localScale = transform.localScale; });
        }



        void UpdateIndexNumber()
        {
            number.text = (MyCharacter.Destinations.FindIndex(x => x == gameObject) + 1).ToString();
        }


        private void FixedUpdate()
        {
            if (!MyCharacter || !MyCharacter.CharacterParsed) return;

            if (MyCharacter.AnyNodeMoving)
                AvoidSpatialCover();


            var charPosXZ = new Vector3(MyCharacter.transform.position.x, 0, MyCharacter.transform.position.z);
            var MyPosXZ = new Vector3(transform.position.x, 0, transform.position.z);

            //if there is only one node and it is me
            if (MyCharacter.Destinations.Count == 1)
            {
                //The character position when there is only one node
                if (MyCharacter.MovementType != "followplayer")
                {
                    MyCharacter.GetComponent<NavMeshAgent>().updatePosition = false;
                    MyCharacter.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                    MyCharacter.transform.localRotation = transform.localRotation;
                }
                MyCharacter.SelectClip();
            }

            //last node decides the rotation of the character
            else if (MyCharacter.Destinations[MyCharacter.Destinations.Count - 1] == gameObject &&
                Vector3.Distance(charPosXZ, MyPosXZ) <= MyCharacter.Agent.stoppingDistance &&
                MyCharacter.transform.localRotation != transform.localRotation)
                MyCharacter.transform.localRotation = Quaternion.Lerp(MyCharacter.transform.localRotation, transform.localRotation, 0.5f);

            //show the pointer if this node is the last node
            pointer.gameObject.SetActive(MyCharacter.Destinations[MyCharacter.Destinations.Count - 1] == gameObject);
        }



        public void DeleteNode()
        {
            if (MyCharacter && MyCharacter.Destinations.Count == 1)
            {
                return;
            }

            DeactiveAllNodeBounding();

            //remeke the destination list without this node
            List<GameObject> newDestinations = new List<GameObject>();

            foreach (GameObject des in MyCharacter.Destinations)
            {
                if (des != gameObject)
                {
                    newDestinations.Add(des);

                    //refresh the numbers
                    des.GetComponent<Destination>().GetNumber().text = (newDestinations.FindIndex(x => x == des) + 1).ToString();
                }

            }

            MyCharacter.Destinations = newDestinations;
            MyCharacter.SaveJson();

            //follow next node
            MyCharacter.FollowThePath(MyCharacter.Destinations.Count - 1, false);

            //activate bounding of the last node
            MyCharacter.Destinations[MyCharacter.Destinations.Count - 1].GetComponent<BoundsControl>().Active = true;

            //delete this node
            Destroy(gameObject);

        }


        /// <summary>
        /// Disable bounding boxes of all nodes
        /// </summary>
        void DeactiveAllNodeBounding()
        {
            for (int i = 0; i < MyCharacter.Destinations.Count; i++)
            {
                MyCharacter.Destinations[i].GetComponent<BoundsControl>().Active = false;
            }
        }


        public void AddNode()
        {
            DeactiveAllNodeBounding();

            GameObject newDesNodeModel = Instantiate(gameObject, transform.position + new Vector3(0.1f, 0, 0), transform.rotation);

            //activate the boundingbox of the new node
            newDesNodeModel.GetComponent<BoundsControl>().Active = true;

            Destination newDestination = newDesNodeModel.GetComponent<Destination>();
            newDestination.MyCharacter = MyCharacter;
            newDesNodeModel.name = "CharacterDestination(Clone)";
            newDesNodeModel.transform.SetParent(transform.parent);
            MyCharacter.Destinations.Add(newDesNodeModel);

            //update the index num
            newDestination.UpdateIndexNumber();

            //follow the new node
            MyCharacter.FollowThePath(MyCharacter.Destinations.Count - 1, false);

            //save json file again
            MyCharacter.SaveJson();
        }

    }

}
