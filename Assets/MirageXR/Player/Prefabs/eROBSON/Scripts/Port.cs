using MirageXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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


    private void Start()
    {
        myBit = transform.parent.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        detectedPortPole = other.GetComponent<Port>();
        erobsonItem = myBit.GetComponent<eROBSONItems>();

        //If the port is already connected
        if (Connected)
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


        //If a port with different charge is detected
        if (detectedPortPole && detectedPortPole.Pole != Pole.NEUTRAL)
        {

            //if the detected port has NOT the same charge (different charge)
            if (detectedPortPole.Pole != Pole)
            {
                //Make the pole be the parent of the bit
                MakePortBeParent();

                //Disable manipulation
                erobsonItem.DisableManipulation();

                //Move the bit to the pole of the other bit (Snapping)
                transform.position = detectedPortPole.transform.position;

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
        detectedPortPole = null;
    }


}
