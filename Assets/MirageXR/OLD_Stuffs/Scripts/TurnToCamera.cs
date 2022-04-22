using UnityEngine;

public class TurnToCamera : MonoBehaviour {

    public bool isBillboard = true;
    private Vector3 _origRotation;

    void Start()
    {
        _origRotation = transform.rotation.eulerAngles;
    }

    void Update () {
        if ( isBillboard == true ) {
            gameObject.transform.LookAt( Camera.main.transform, Vector3.up );
            gameObject.transform.Rotate( 0f, 180f, 0f, Space.Self );
            gameObject.transform.Rotate(_origRotation);
        }
	}

}
