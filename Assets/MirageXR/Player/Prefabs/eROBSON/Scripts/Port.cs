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

    public GameObject PortBit => myBit;

    public Pole Pole => pole;

    private GameObject myBit;

    private Port detectedPortPole;

    private eROBSONItems detectedBit;

    public bool Connected { get; private set; }

    private Rigidbody rb;

    private void Start()
    {
        myBit = transform.parent.gameObject;
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        detectedPortPole = other.GetComponent<Port>();
        detectedBit = myBit.GetComponent<eROBSONItems>();

        //If the port is already connected
        if (Connected)
        {
            return;
        }

        //If the bit is connected to atleast one other bit
        foreach (var port in detectedBit.Ports)
        {
            if (port.Connected)
            {
                return;
            }
        }

        var touchedObject = detectedBit.TouchedObject;

        //If a port with different charge is detected
        if (detectedPortPole && detectedPortPole.Pole != Pole.NEUTRAL && touchedObject != null)
        {

            //if the detected port has NOT the same charge (different charge)
            if (detectedPortPole.Pole != Pole)
            {
                //Make the pool be the parent of the bit
                foreach (var child in myBit.GetComponentsInChildren<Transform>())
                {
                    if(child.parent == myBit.transform)
                    {
                        child.SetParent(transform);
                    }
                }

                //Snap the bit to the port
                detectedBit.DisableManipulation();

                //Move the bit to the pole of the other bit (Snapping)
                rb.MovePosition(detectedPortPole.transform.position);
                Connected = true;
            }
            else
            {
                //Move the bit to the left or right side of this bit depends on the port
                var connectionPosition = Pole == Pole.POSITIVE ? detectedPortPole.transform.forward : -detectedPortPole.transform.forward;

                //Make a distance from the detected port
                transform.position = detectedPortPole.transform.position + connectionPosition * 0.2f;

                //Make the bit item as the bit parent again
                ReParentComponent();
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {

        //Make the bit item as the bit parent again
        ReParentComponent();
        Connected = false;
        detectedPortPole = null;

        if (detectedBit)
        {
            detectedBit.EnableManipulation();
            detectedBit = null;
        }
    }

    private void ReParentComponent()
    {
        gameObject.transform.SetParent(myBit.transform);

        foreach (var child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.parent == transform)
            {
                child.SetParent(myBit.transform);
            }
        }

       
    }

}
