using MirageXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Pole
{
    POSITIVE,
    NEGATIVE,
    NEUTRAL
}

public class Port : MonoBehaviour
{
    [SerializeField] private Pole pole;

    public Pole Pole => pole;

    private Port detectedPortPole;


    private GameObject myBit;
    private eROBSONItems erobsonItem;

    public bool Connected { get; private set; }

    private async void Start()
    {
        //Add the image marker controller to the scene if it is not exist yet (first bit)
        if (FindObjectOfType<ERobsonImageMarkerController>() == null)
        {
            // Get the prefab from the references
            var erobsonImageMarkerController = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("eROBSON/Prefabs/ErobsonImageMarkerController");
            // if the prefab reference has been found successfully
            if (erobsonImageMarkerController != null)
            {
                Instantiate(erobsonImageMarkerController, Vector3.zero, Quaternion.identity);
            }
        }

        myBit = transform.parent.gameObject;
    }

    /// <summary>
    /// Get the list of all connected bits in the scene
    /// </summary>
    public List<eROBSONItems> ConnectedERobsonItems
    {
        get
        {
            return FindObjectsOfType<eROBSONItems>().ToList().FindAll(i => i.Ports.ToList().Find(p => p.Connected));
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        detectedPortPole = other.GetComponent<Port>();
        erobsonItem = myBit.GetComponent<eROBSONItems>();

        //If the other collider is not a port or it is already connected
        if(detectedPortPole == null || detectedPortPole.Connected)
        {
            return;
        }

        //Port is not neutral and the bit NOT moving by the user
        if (!erobsonItem.IsMoving || detectedPortPole.Pole == Pole.NEUTRAL)
        {
            return;
        }

        //If the bit is connected to atleast one other bit
        foreach (var port in erobsonItem.Ports)
        {
            if (port.Connected)
            {
                return;
            }
        }

        //if the detected port has NOT the same charge (different charge) and it is not connected to any other bit
        if (detectedPortPole.Pole != Pole && !ConnectedERobsonItems.Contains(detectedPortPole.erobsonItem))
        {

            //Make the pole be the parent of the bit
            MakePortBeParent();

            //Disable manipulation
            erobsonItem.DisableManipulation();

            //Move the bit to the pole of the other bit (Snapping)
            transform.position = detectedPortPole.transform.position;
            transform.rotation = detectedPortPole.transform.rotation;

            //Enable manipulation after a while
            StartCoroutine(MakeBitBeParent());
        }
        else
        {
            //Move the bit to the left or right side of this bit depends on the port
            var connectionPosition = Pole == Pole.POSITIVE ? -detectedPortPole.transform.forward : detectedPortPole.transform.forward;

            //Make a distance from the detected port
            myBit.transform.position = detectedPortPole.transform.position + connectionPosition * 0.2f;

            //Enable manipulation after a while
            StartCoroutine(MakeBitBeParent());

            OnConnected();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        OnDisconnected();
    }


    /// <summary>
    /// Make the connected port(this) be the parent of the bit and all its children
    /// </summary>
    private void MakePortBeParent()
    {
        foreach (var child in myBit.GetComponentsInChildren<Transform>())
        {
            if (child.parent == myBit.transform)
            {
                child.SetParent(transform);
            }
        }
    }


    /// <summary>
    /// Make the bit be the parent of its children and both ports (negative and positive) again
    /// </summary>
    IEnumerator MakeBitBeParent()
    {

        foreach (var child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.parent == transform || child == transform)
            {
                child.SetParent(myBit.transform);
            }
            yield return null;
        }
        erobsonItem.EnableManipulation();
    }


    /// <summary>
    /// Port is disconnected
    /// </summary>
    private void OnConnected()
    {
        Connected = true;
    }


    /// <summary>
    /// When the port is disconnected
    /// </summary>
    private void OnDisconnected()
    {
        Connected = false;

        if(detectedPortPole)
            detectedPortPole.Connected = false;
    }


}
