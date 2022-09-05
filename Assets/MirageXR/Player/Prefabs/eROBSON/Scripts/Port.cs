
using System.Collections;
using UnityEngine;

public enum Pole
{
    POSITIVE,
    NEGATIVE,
    USB
}

public class Port : MonoBehaviour
{
    [SerializeField] private Pole pole;

    public Port DetectedPortPole
    {
        get; private set;
    }

    private GameObject eRobsonItemGameObject;
    private eROBSONItems erobsonItem;

    public bool Connected { get; set; }


    private void Start()
    {
        eRobsonItemGameObject = transform.parent.gameObject;
        erobsonItem = eRobsonItemGameObject.GetComponent<eROBSONItems>();
    }


    private void OnTriggerEnter(Collider other)
    {
        DetectedPortPole = other.GetComponent<Port>();

        //If the other collider is not a port or it is already connected
        if (erobsonItem == null || DetectedPortPole == null)
        {
            return;
        }

        //If the ports are not already connected
        if (Connected || DetectedPortPole.Connected)
            return;

        //Port is not neutral and the bit NOT moving by the user
        if (!erobsonItem.IsMoving)
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

        //Check the port can be connected to the detected port
        if (CanBeConnected())
        {

            //Make the pole be the parent of the bit
            MakePortBeParent();

            //Disable manipulation
            erobsonItem.DisableManipulation();

            //Move the bit to the pole of the other bit (Snapping)
            transform.position = DetectedPortPole.transform.position;
            transform.rotation = DetectedPortPole.transform.rotation;

            //Enable manipulation after a while
            StartCoroutine(MakeBitBeParent());

            OnConnecting(DetectedPortPole);

        }
        else if(DetectedPortPole.pole != Pole.USB && pole != Pole.USB)
        {

            //Move the bit to the left or right side of this bit depends on the port
            var connectionPosition = pole == Pole.POSITIVE ? -DetectedPortPole.transform.forward : DetectedPortPole.transform.forward;

            //Make a distance from the detected port
            eRobsonItemGameObject.transform.position = DetectedPortPole.transform.position + connectionPosition * 0.2f;

            OnDisconnecting();

        }

    }


    /// <summary>
    /// Check if the bits can be connected
    /// </summary>
    /// <returns></returns>
    private bool CanBeConnected()
    {
        return ((DetectedPortPole.pole != pole && DetectedPortPole.pole != Pole.USB && pole != Pole.USB) 
            || (DetectedPortPole.pole == Pole.USB && pole == Pole.USB))
            && !erobsonItem.connectedbits.Contains(DetectedPortPole.erobsonItem);
    }


    private void OnTriggerExit(Collider other)
    {
        OnDisconnecting();
    }


    /// <summary>
    /// Make the connected port(this) be the parent of the bit and all its children
    /// </summary>
    private void MakePortBeParent()
    {
        foreach (var child in eRobsonItemGameObject.GetComponentsInChildren<Transform>())
        {
            if (child.parent == eRobsonItemGameObject.transform)
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
                child.SetParent(eRobsonItemGameObject.transform);
            }
            yield return null;
        }

        //Make snap effect be more clear
        yield return new WaitForSeconds(0.1f);

        erobsonItem.EnableManipulation();
    }


    /// <summary>
    /// When the port is connected
    /// </summary>
    private void OnConnecting(Port detectedPort)
    {

        if (detectedPort && detectedPort.erobsonItem)
        {
            Connected = true;

            if (!erobsonItem.connectedbits.Contains(detectedPort.erobsonItem))
            {
                erobsonItem.connectedbits.Add(detectedPort.erobsonItem);
            }

            erobsonItem.connectedTime = new System.DateTime();
            ErobsonItemManager.BitConnected(erobsonItem);
        }
    }


    /// <summary>
    /// When the port is disconnected
    /// </summary>
    private void OnDisconnecting()
    {

        if (DetectedPortPole)
        {
            Connected = false;

            //Add connected bit to my list
            if (erobsonItem.connectedbits.Contains(DetectedPortPole.erobsonItem))
            {
                erobsonItem.connectedbits.Remove(DetectedPortPole.erobsonItem);
            }

            ErobsonItemManager.BitDisconnected(erobsonItem);

            DetectedPortPole = null;
        }

    }


}
