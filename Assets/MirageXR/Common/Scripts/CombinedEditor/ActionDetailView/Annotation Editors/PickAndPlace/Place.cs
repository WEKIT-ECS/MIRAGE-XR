using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
/*
namespace MirageXR
{
    public class Pick : MonoBehaviour
    {

        public Transform placeLocation;
        public GameObject pickOb;
        public float correctionDistance;
        public bool resetOnMiss = true;
        public bool moveMode = true;

        private bool isMoving = false;
        public Vector3 resetPos;



<<<<<<< HEAD
        // Start is called before the first frame update
        void Start()
        {
            pickOb = this.gameObject;
            changeCorrectionDistance(0.5f);
            setResetPos(pickOb.transform.localPosition);
        }
=======
    // Start is called before the first frame update
    void Start()
    {
        pickOb = this.gameObject;
        ChangeCorrectionDistance(0.5f);
        SetResetPos(pickOb.transform.localPosition);      
    }
>>>>>>> 55eb6be7398f057b0e0973d8af228b1c3495229f

        void Update()
        {
            if (!moveMode)
            {
<<<<<<< HEAD
                if (transform.hasChanged)
                {
                    manipulationStart();
                }
                else if (!transform.hasChanged)
                {
                    manipulationStop();
                }
                transform.hasChanged = false;
=======
                ManipulationStart();
>>>>>>> 55eb6be7398f057b0e0973d8af228b1c3495229f
            }
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void setMoveMode()
        {
            if (moveMode)
            {
<<<<<<< HEAD
                moveMode = false;
                setResetPos(pickOb.transform.localPosition);
=======
                ManipulationStop();
>>>>>>> 55eb6be7398f057b0e0973d8af228b1c3495229f
            }
            else
            {
                moveMode = true;
            }

        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void setRestOnMiss(bool reset)
        {
<<<<<<< HEAD
            Debug.Log(reset);
            resetOnMiss = reset;
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void setTargettransform(Transform target)
        {
            placeLocation = target;
        }


        /// <summary>
        /// Sets the possition of the pick objects reset location
        /// </summary> 
        public void setResetPos(Vector3 pos)
        {
            resetPos = pos;
        }
=======
            moveMode = false;
            //setResetPos(pickOb.transform.localPosition);
        }
        else {
            moveMode = true;
        }       
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
>>>>>>> 55eb6be7398f057b0e0973d8af228b1c3495229f

        /// <summary>
        /// Change the distance that will be considered close enough to the desired place location
        /// </summary>
        public void changeCorrectionDistance(float distance)
        {
            correctionDistance = distance;
        }

<<<<<<< HEAD
        /// <summary>
        /// Sets isMoving to true to show that the object is being manipulated 
        /// </summary>
        public void manipulationStart()
        {
            isMoving = true;
        }

        /// <summary>
        /// Sets isMoving to false to show that the object has stopped being manipulated 
        /// </summary>
        public void manipulationStop()
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
=======
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
    public void ChangeCorrectionDistance(float distance) {
        correctionDistance = distance;
    }

    /// <summary>
    /// Sets isMoving to true to show that the object is being manipulated 
    /// </summary>
    public void ManipulationStart() {
        isMoving = true;
    }

    /// <summary>
    /// Sets isMoving to false to show that the object has stopped being manipulated 
    /// </summary>
    public void ManipulationStop() {
        isMoving = false;
>>>>>>> 55eb6be7398f057b0e0973d8af228b1c3495229f

        }
    }
}
*/

