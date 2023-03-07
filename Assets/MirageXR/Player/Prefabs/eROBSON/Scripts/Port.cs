using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum Pole
{
    POSITIVE,
    NEGATIVE,
    USB
}

public class Port : MonoBehaviour
{
    private const float RAY_DISTANCE = 0.03f;
    private const float SNAPPING_DURATION = 2f;

    [SerializeField] private Pole pole;
    [SerializeField] private GameObject IncorrectIconPrefab;
    [SerializeField] private bool reverseRay;
    [SerializeField] private bool portMovesSeparate;
    [SerializeField] private FixedPort fixedPort;


    /// <summary>
    /// Check if the port is moving by the user
    /// </summary>
    public bool PortIsMovable => portMovesSeparate;


    /// <summary>
    /// Gets the port which is connecting to this port
    /// </summary>
    public Port DetectedPortPole { get; set; }


    /// <summary>
    /// Checks if ports are USB power and P3. They can't be same neither
    /// </summary>
    private bool UsbPowerConnectionCheck => (DetectedPortPole.pole == Pole.USB && pole == Pole.USB) &&
                                            ((DetectedPortPole.ERobsonItem.ID == BitID.USBPOWER && ERobsonItem.ID != BitID.USBPOWER) ||
                                             (DetectedPortPole.ERobsonItem.ID != BitID.USBPOWER && ERobsonItem.ID == BitID.USBPOWER));

    /// <summary>
    /// If the port is connected
    /// </summary>
    public bool Connected { get; set; }


    /// <summary>
    /// The bit which contains this port
    /// </summary>
    public eROBSONItems ERobsonItem { get; private set; }


    //Caches
    private GameObject _eRobsonItemGameObject;
    private bool _shaking;
    private Camera _cam;



    private void Start()
    {
        fixedPort = GetComponentInParent<FixedPort>();
        ERobsonItem = GetComponentInParent<eROBSONItems>();
        _cam = Camera.main;
        _eRobsonItemGameObject = ERobsonItem.gameObject;
    }


    private void FixedUpdate()
    {
        var myTransform = transform;
        var ray = new Ray(myTransform.position, myTransform.forward * (reverseRay ? -1 : 1));

        // Check if the ray hits any GameObjects within the specified distance
        if (Physics.Raycast(ray, out var hit, RAY_DISTANCE, LayerMask.GetMask("eRobsonPort")))
        {
            ControlPortCollision(hit.collider.gameObject);
        }
        else
        {
            Disconnect();
        }

        Debug.DrawRay(myTransform.position, myTransform.forward * (RAY_DISTANCE * (reverseRay ? -1 : 1)), Color.yellow);
    }


    /// <summary>
    /// Check the information of the port which are collided to each other and
    /// make a dissection for their connecting regarding to their information
    /// </summary>
    /// <param name="collidedPort">The GameObject of the port which is detected and will connected to this port</param>
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
        if (!ERobsonItem.IsMoving && (!fixedPort || (fixedPort && fixedPort.IsLock)))
        {
            return;
        }


        //If the bit has more than one port and it is connected to at least one other bit
        if (ERobsonItem.Ports.Length > 1 && ERobsonItem.Ports.Any(port => port.Connected))
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
        else if (!UsbPowerConnectionCheck)
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
        var icon = Instantiate(IncorrectIconPrefab, DetectedPortPole.transform.position - _cam.transform.forward * 0.1f, Quaternion.identity);
        var audioSource = icon.GetComponent<AudioSource>();

        if (audioSource.isPlaying)
        {
            return;
        }

        audioSource.Play();
        StartCoroutine(Shake(ERobsonItem.transform));

        if (icon)
        {
            Destroy(icon, audioSource.clip.length);
        }
    }


    /// <summary>
    /// Shake the gameobject
    /// </summary>
    /// <param name="go">The gameobject that will be shaken</param>
    /// <returns>return the status of shaking</returns>
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
    /// <returns>a boolean that says the bit can be connected or not to the detected port</returns>
    private bool CanBeConnected()
    {
        var connectIt = false;

        if (RootObject.Instance.activityManager.EditModeActive || ERobsonItem.LoadedData == null)
        {
            var hasDifferentPole = DetectedPortPole.pole != pole && (DetectedPortPole.pole != Pole.USB && pole != Pole.USB);
            var isAlreadyConnected = ERobsonItem.ConnectedBits.Contains(DetectedPortPole.ERobsonItem);

            connectIt = (UsbPowerConnectionCheck || hasDifferentPole) && !isAlreadyConnected;
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
        if (PortIsMovable)
        {
            return;
        }

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
            yield return new WaitForSeconds(SNAPPING_DURATION);
            ERobsonItem.EnableManipulation();
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
        yield return new WaitForSeconds(SNAPPING_DURATION);

        ERobsonItem.EnableManipulation();
    }


    /// <summary>
    /// When the port is connected
    /// </summary>
    private void Connect(Port detectedPort)
    {
        if (!detectedPort || !detectedPort.ERobsonItem)
        {
            return;
        }

        //If port is already registered as a connected port for the detected port
        if (ERobsonItem.ConnectedBits.Contains(detectedPort.ERobsonItem))
        {
            return;
        }

        ERobsonItem.ConnectedBits.Add(detectedPort.ERobsonItem);
        detectedPort.ERobsonItem.ConnectedBits.Add(ERobsonItem);
        ERobsonItem.connectedTime = new DateTime();
        ERobsonItem.DisableManipulation();

        //Move the bit port to the port of the other bit (Snapping)
        if (!PortIsMovable)
        {
            var detectedPortTransform = DetectedPortPole.transform;

            //Move the bit to the detected port
            transform.SetPositionAndRotation(detectedPortTransform.position, detectedPortTransform.rotation);
        }
        else
        {
            try
            {
                //Move the port to the detected port
                StartCoroutine(fixedPort.LetConnect(SNAPPING_DURATION, DetectedPortPole.transform.position));
            }
            catch (NullReferenceException)
            {
                Debug.LogError("movablePortManipulatorObject in Port component is not assigned. When you" +
                               "Check portMovesSeparate, it should be assigned too.");
            }
        }

        ErobsonItemManager.BitConnected(ERobsonItem);
        ErobsonItemManager.BitConnected(detectedPort.ERobsonItem);
        Connected = true;
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

        //Add connected bit to my list
        if (ERobsonItem.ConnectedBits.Contains(DetectedPortPole.ERobsonItem))
        {
            ERobsonItem.ConnectedBits.Remove(DetectedPortPole.ERobsonItem);
        }

        ErobsonItemManager.BitDisconnected(ERobsonItem);
        ErobsonItemManager.BitDisconnected(DetectedPortPole.ERobsonItem);

        DetectedPortPole = null;

        Connected = false;
    }
}