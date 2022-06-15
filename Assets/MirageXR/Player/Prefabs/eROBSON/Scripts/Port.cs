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

    private float bitScaleFactor = 0.23f;

    public bool Connected { get; private set; }

    private void Start()
    {
        myBit = transform.parent.gameObject;
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
            //Move the bit to the left or right side of this bit depends on the port
            var connectionPosition = Pole == Pole.POSITIVE ? detectedPortPole.transform.forward : -detectedPortPole.transform.forward;

            //if the detected port has NOT the same charge (different charge)
            if (detectedPortPole.Pole != Pole)
            {
                //Snap the bit to the port
                detectedBit.DisableManipulation();
                myBit.transform.position = detectedPortPole.transform.position  + new Vector3(0, 0, bitScaleFactor);
                Connected = true;
            }
            else
            {
                //Make a distance from the detected port
                myBit.transform.position = detectedPortPole.transform.position + connectionPosition * bitScaleFactor * 1.5f;
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Connected = false;
        detectedPortPole = null;

        if (detectedBit)
        {
            detectedBit.EnableManipulation();
            detectedBit = null;
        }
    }

}
