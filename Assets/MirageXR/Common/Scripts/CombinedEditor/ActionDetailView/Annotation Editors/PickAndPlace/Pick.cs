using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class Pick : MonoBehaviour
    {

        [SerializeField] private Transform placeLocation;
        [SerializeField] private GameObject pickOb;
        [SerializeField] private float correctionDistance;
        [SerializeField] private bool resetOnMiss = true;
        [SerializeField] private SpriteToggle iconToggle;
        [SerializeField] private Button changeModelButton;

        public Vector3 resetPos;
        private bool isMoving = false;
        private bool moveMode = true;


        public MeshRenderer ArrowRenderer
        {
            get { return GetComponentInChildren<MeshRenderer>(); }
        } 

        public Button ChangeModelButton
        {
            get
            {
                return changeModelButton;
            }
        }

        public bool MoveMode
        {
            set => moveMode = value;
            get => moveMode;
        }


        public string MyModelID
        {
            get; set;
        }

        // Start is called before the first frame update
        void Start()
        {
            pickOb = this.gameObject;
            ChangeCorrectionDistance(0.5f);
            SetResetPos(pickOb.transform.localPosition);
            moveMode = false;
            iconToggle.IsSelected = true;

            changeModelButton.onClick.AddListener(CapturePickModel);
        }

        void Update()
        {
            if (!moveMode)
            {
                if (transform.hasChanged)
                {
                    ManipulationStart();
                }
                else if (!transform.hasChanged)
                {
                    ManipulationStop();
                }
                transform.hasChanged = false;
            }
        }

        /// <summary>
        /// Ready to select a model augmentation for replcaing with the arrow
        /// </summary>
        private void CapturePickModel()
        {
            changeModelButton.GetComponent<Image>().color = Color.red;
            ActionEditor.Instance.pickArrowModelCapturing = (true, this);
        }



        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void SetMoveMode()
        {
            if (moveMode)
            {
                moveMode = false;
                SetResetPos(pickOb.transform.localPosition);
            }
            else
            {
                moveMode = true;
            }

            iconToggle.ToggleValue();
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void SetRestOnMiss(bool reset)
        {
            Debug.Log(reset);
            resetOnMiss = reset;
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void SetTargettransform(Transform target)
        {
            placeLocation = target;
        }


        /// <summary>
        /// Sets the possition of the pick objects reset location
        /// </summary> 
        public void SetResetPos(Vector3 pos)
        {
            resetPos = pos;
        }

        /// <summary>
        /// Change the distance that will be considered close enough to the desired place location
        /// </summary>
        public void ChangeCorrectionDistance(float distance)
        {
            correctionDistance = distance;
        }

        /// <summary>
        /// Sets isMoving to true to show that the object is being manipulated 
        /// </summary>
        public void ManipulationStart()
        {
            isMoving = true;
        }

        /// <summary>
        /// Sets isMoving to false to show that the object has stopped being manipulated 
        /// </summary>
        public void ManipulationStop()
        {
            isMoving = false;

            if (Mathf.Abs(pickOb.transform.localPosition.x - placeLocation.localPosition.x) <= correctionDistance &&
                Mathf.Abs(pickOb.transform.localPosition.y - placeLocation.localPosition.y) <= correctionDistance &&
                Mathf.Abs(pickOb.transform.localPosition.z - placeLocation.localPosition.z) <= correctionDistance)
            {
                pickOb.transform.localPosition = new Vector3(placeLocation.localPosition.x, placeLocation.localPosition.y, placeLocation.localPosition.z);
            }
            else if (resetOnMiss)
            {
                pickOb.transform.localPosition = new Vector3(resetPos.x, resetPos.y, resetPos.z);

            }

        }
    }
}
