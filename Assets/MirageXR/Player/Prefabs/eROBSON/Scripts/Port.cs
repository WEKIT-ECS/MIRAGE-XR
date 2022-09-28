
using MirageXR;
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
    [SerializeField] private GameObject IncorrectIconPrefab;

    public Port DetectedPortPole
    {
        get; set;
    }

    public bool Connected { get; set; }

    public eROBSONItems ERobsonItem => erobsonItem;


    private GameObject eRobsonItemGameObject;
    private eROBSONItems erobsonItem;

    private bool _shaking;

    private void Start()
    {
        eRobsonItemGameObject = transform.parent.gameObject;
        erobsonItem = eRobsonItemGameObject.GetComponent<eROBSONItems>();
    }


    /// <summary>
    /// When the port collides any other colliders
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {

        ControllPortCollision(other.gameObject);
    }



    /// <summary>
    /// Check the informarion of the port which are collided to eachother and
    /// make a disscion for their connecting regarding to their information
    /// </summary>
    /// <param name="collidedPort"></param>
    private void ControllPortCollision(GameObject collidedPort)
    {
        DetectedPortPole = collidedPort.GetComponent<Port>();

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
        else if (DetectedPortPole.pole != Pole.USB && pole != Pole.USB)
        {

            if (!RootObject.Instance.activityManager.EditModeActive)
            {
                if (!_shaking)
                {
                    //Show the feedback
                    var icon = Instantiate(IncorrectIconPrefab, DetectedPortPole.transform.position - Camera.main.transform.forward * 0.1f, Quaternion.identity);
                    var audioSource = icon.GetComponent<AudioSource>();

                    if (!audioSource.isPlaying)
                    {
                        audioSource.Play();
                        StartCoroutine(Skake(erobsonItem.transform));

                        if(icon)
                            Destroy(icon, audioSource.clip.length);
                    }
                }

            }

            OnDisconnecting();
        }
    }


    private IEnumerator Skake(Transform go)
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

        if (RootObject.Instance.activityManager.EditModeActive || erobsonItem.LoadedData == null)
        {
            var isUSBConnection = DetectedPortPole.pole == Pole.USB && pole == Pole.USB;
            var hasDifferentPole = DetectedPortPole.pole != pole && DetectedPortPole.pole != Pole.USB && pole != Pole.USB;
            var isAlreadyConnected = erobsonItem.connectedbits.Contains(DetectedPortPole.erobsonItem);

            connectIt = (hasDifferentPole || isUSBConnection) && !isAlreadyConnected;
        }
        else
        {
            if (erobsonItem.LoadedData.connectedbitsID.Contains(DetectedPortPole.erobsonItem.poiID))
            {
                connectIt = true;
            }
        }


        return connectIt;
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
        yield return new WaitForSeconds(1f);

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
