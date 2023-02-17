using System;
using MirageXR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public enum Pole
{
    POSITIVE,
    NEGATIVE,
    USB
}

public class Port : MonoBehaviour
{
    private const float RayDistance = 0.02f;
    private const float SnappingDuration = 2f;

    [SerializeField] private Pole pole;
    [SerializeField] private GameObject IncorrectIconPrefab;
    [SerializeField] private bool reverseRay;
    [SerializeField] private bool portMovesSeparate;
    [SerializeField] private FixedPort fixedPort;

    public bool PortIsMovable => portMovesSeparate;

    public Vector3 PortPosition
    {
        get => fixedPort != null ? fixedPort.transform.localPosition : Vector3.zero;
        set
        {
            if (fixedPort != null)
            {
                fixedPort.transform.localPosition = value;
            }
        }
    }

    public Port DetectedPortPole { get; set; }

    public bool PortIsMovingSeparately { get; set;}

    public bool Connected { get; set; }

    public eROBSONItems ERobsonItem { get; private set; }

    private GameObject _eRobsonItemGameObject;
    private bool _shaking;

    private void Start()
    {
        fixedPort = GetComponentInParent<FixedPort>();
        ERobsonItem = GetComponentInParent<eROBSONItems>();
        _eRobsonItemGameObject = ERobsonItem.gameObject;
    }


    private void Update()
    {
        var myTransform = transform;
        var ray = new Ray(myTransform.position, myTransform.forward * (reverseRay ? -1 : 1));

        // Check if the ray hits any GameObjects within the specified distance
        if (Physics.Raycast(ray, out var hit, RayDistance, LayerMask.GetMask("eRobsonPort")))
        {
            ControlPortCollision(hit.collider.gameObject);
        }
        else
        {
            Disconnect();
        }

        Debug.DrawRay(myTransform.position, myTransform.forward * (RayDistance * (reverseRay ? -1 : 1)), Color.yellow);
    }


    /// <summary>
    /// Check the information of the port which are collided to each other and
    /// make a dissection for their connecting regarding to their information
    /// </summary>
    /// <param name="collidedPort"></param>
    private void ControlPortCollision(GameObject collidedPort)
    {
        DetectedPortPole = collidedPort.GetComponent<Port>();

        //If the other collider is not a port or it is already connected
        if (ERobsonItem == null || DetectedPortPole == null)
        {
            return;
        }

        //If the ports are not already connected
        if (Connected || DetectedPortPole.Connected)
        {
            return;
        }

        //Port is not neutral and the bit NOT moving by the user
        if (!ERobsonItem.IsMoving && !PortIsMovingSeparately)
        {
            return;
        }

        //If the bit is connected to at least one other bit
        if (ERobsonItem.Ports.Any(port => port.Connected))
        {
            return;
        }

        //Check the port can be connected to the detected port
        if (CanBeConnected())
        {

            //The order of calling these three functions is important
            //Make the pole be the parent of the bit
            MakePortBeParent();

            //Connect the ports
            Connect(DetectedPortPole);

            //Enable manipulation after a while
            StartCoroutine(MakeBitBeParent());

        }
        else if (DetectedPortPole.pole != Pole.USB && pole != Pole.USB)
        {
            DisplayWrongConnectivityMessage();
            Disconnect();
        }
    }


    /// <summary>
    /// Display an error icon with sound on the port
    /// </summary>
    private void DisplayWrongConnectivityMessage()
    {
        if (RootObject.Instance.activityManager.EditModeActive)
        {
            return;
        }

        if (_shaking)
        {
            return;
        }

        //Show the feedback
        var icon = Instantiate(IncorrectIconPrefab, DetectedPortPole.transform.position - Camera.main.transform.forward * 0.1f, Quaternion.identity);
        var audioSource = icon.GetComponent<AudioSource>();

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            StartCoroutine(Shake(ERobsonItem.transform));

            if (icon)
            {
                Destroy(icon, audioSource.clip.length);
            }
        }
    }


    /// <summary>
    /// Shake the gameobject
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerator Shake(Transform go)
    {
        _shaking = true;

        var temp = go;

        for (int i = 0; i < 10; i++)
        {
            var rand = (float)Utilities.GetRandomDouble(0, 0.01f);
            go.localPosition += new Vector3(rand, 0, 0);
            yield return new WaitForSeconds(0.02f);
            go.localPosition -= new Vector3(rand, 0, 0);
            yield return new WaitForSeconds(0.02f);
        }

        go.localPosition = temp.localPosition;

        _shaking = false;
    }


    /// <summary>
    /// Check if the bits can be connected
    /// </summary>
    /// <returns></returns>
    private bool CanBeConnected()
    {
        var connectIt = false;

        if (RootObject.Instance.activityManager.EditModeActive || ERobsonItem.LoadedData == null)
        {
            var isUsbConnection = DetectedPortPole.pole == Pole.USB && pole == Pole.USB;
            var hasDifferentPole = DetectedPortPole.pole != pole && DetectedPortPole.pole != Pole.USB && pole != Pole.USB;
            var isAlreadyConnected = ERobsonItem.ConnectedBits.Contains(DetectedPortPole.ERobsonItem);

            connectIt = (hasDifferentPole || isUsbConnection) && !isAlreadyConnected;
        }
        else
        {
            if (ERobsonItem.LoadedData.connectedbitsID.Contains(DetectedPortPole.ERobsonItem.poiID))
            {
                connectIt = true;
            }
        }

        return connectIt;
    }


    /// <summary>
    /// Make the connected port(this) be the parent of the bit and all its children
    /// </summary>
    private void MakePortBeParent()
    {
        if (PortIsMovable) return;

        foreach (var child in _eRobsonItemGameObject.GetComponentsInChildren<Transform>())
        {
            if (child.parent == _eRobsonItemGameObject.transform)
            {
                child.SetParent(transform);
            }
        }
    }


    /// <summary>
    /// Make the bit be the parent of its children and both ports (negative and positive) again
    /// </summary>
    private IEnumerator MakeBitBeParent()
    {
        if (PortIsMovable)
        {
            //Make snap effect be more clear
            yield return new WaitForSeconds(SnappingDuration);
            fixedPort.GetComponent<ObjectManipulator>().enabled = true;
            yield break;
        }

        foreach (var child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.parent == transform || child == transform)
            {
                child.SetParent(_eRobsonItemGameObject.transform);
            }
            yield return null;
        }

        //Make snap effect be more clear
        yield return new WaitForSeconds(SnappingDuration);

        ERobsonItem.EnableManipulation();
    }


    /// <summary>
    /// When the port is connected
    /// </summary>
    private void Connect(Port detectedPort)
    {
        if (detectedPort && detectedPort.ERobsonItem)
        {
            Connected = true;

            if (!ERobsonItem.ConnectedBits.Contains(detectedPort.ERobsonItem))
            {
                ERobsonItem.ConnectedBits.Add(detectedPort.ERobsonItem);
                ERobsonItem.connectedTime = new DateTime();
                ErobsonItemManager.BitConnected(ERobsonItem);
            }

            //Move the bit port to the port of the other bit (Snapping)
            if (!PortIsMovable)
            {
                //Disable manipulation
                ERobsonItem.DisableManipulation();
                transform.position = DetectedPortPole.transform.position;

                //Move the bit to the detected port
                transform.rotation = DetectedPortPole.transform.rotation;
            }
            else
            {
                try
                {
                    //Disable manipulation
                    fixedPort.GetComponent<ObjectManipulator>().enabled = false;

                    //Move the port to the detected port
                    fixedPort.transform.position = DetectedPortPole.transform.position;
                }
                catch (NullReferenceException)
                {
                    Debug.LogError("movablePortManipulatorObject in Port component is not assigned. When you" +
                                   "Check portMovesSeparate, it should be assigned too.");
                }

            }
        }
    }


    /// <summary>
    /// When the port is disconnected
    /// </summary>
    private void Disconnect()
    {
        if (!DetectedPortPole)
        {
            return;
        }

        Connected = false;

        //Add connected bit to my list
        if (ERobsonItem.ConnectedBits.Contains(DetectedPortPole.ERobsonItem))
        {
            ERobsonItem.ConnectedBits.Remove(DetectedPortPole.ERobsonItem);
        }

        ErobsonItemManager.BitDisconnected(ERobsonItem);

        DetectedPortPole = null;
    }

}
