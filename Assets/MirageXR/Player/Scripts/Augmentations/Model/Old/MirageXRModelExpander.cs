using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// @bibeg
/// basic class for expanding the 3d model based on the parent gameobject which holds all the subcomponenet 3d models
/// The methods is activated on airtap
/// </summary>
public class MirageXRModelExpander : MonoBehaviour
{
    /// <summary>
    /// array for holding all the 3d models
    /// </summary>
    private List<Transform> modelBlock;
    /// <summary>
    /// value that determines if the model is expanded or not
    /// </summary>
    private bool isExpanded = false;
    /// <summary>
    /// center of the whole model
    /// </summary>
    private Vector3 modelCenter;
    /// <summary>
    /// The scalr to determine how far the model will expand
    /// </summary>
    private float expansionFactor = 0.1f;

    // Use this for initialization
    void Start()
    {
        modelBlock = new List<Transform>();

        //Iterate through the first depth of children and add them to list
        foreach (Transform T in gameObject.GetComponentsInChildren<Transform>())
        {
            if (T.parent == gameObject.transform)
            {
                modelBlock.Add(T);
            }
        }

        modelCenter = gameObject.transform.position;
    }
    /// <summary>
    /// Method to expand the 3d model by casting in the direction from the central point of the parent object in the direction of the child objects.
    /// The calculated vector is assigned as the new position of the child objects
    /// </summary>
    void ExpandModel()
    {

        if (isExpanded == false && modelBlock.Count != 0)
        {
            foreach (Transform g in modelBlock)
            {
                //calculate the vector between 2 points pointing from center to the center of the child and increase it by a scalar
                Vector3 direction = g.GetComponent<MeshRenderer>().bounds.center - modelCenter;
                //normalize  and add the new vector to the old child position
                g.position += direction.normalized * expansionFactor;
                //g.localPosition not working
            }
        }
        else
        {
            Debug.Log(isExpanded + " " + modelBlock.Count);
        }
        //change the expanded state
        isExpanded = !isExpanded;

    }

    /// <summary>
    /// methods for compressing the class back to original state. How ever since the initial position of the child is not saved the opposite vector
    /// is calculated and added to the expanded vector
    /// </summary>
    void CompressModel()
    {
        if (isExpanded == true && modelBlock.Count != 0)
        {
            foreach (Transform g in modelBlock)
            {
                //using transfrom.position and using localPos to origin or model center didnt work
                Vector3 direction = modelCenter - g.GetComponent<MeshRenderer>().bounds.center;
                //normalize but since the direction is opposite it should subract based on point to point system.
                g.position += direction.normalized * expansionFactor;

            }
        }
        else
        {
            Debug.Log(isExpanded + " " + modelBlock.Count);
        }
        isExpanded = !isExpanded;
    }
}
