using System.Collections;
using UnityEngine;

public class CubeNetFolder : MonoBehaviour
{
    public GameObject[] sides;
    public GameObject top;
    public GameObject left;
    public GameObject right;
    public GameObject botOne;
    public GameObject botTwo;
    public GameObject button;

    public void fold() {
        folder(1);
        button.SetActive(false);
    }

    private void folder(int side)
    {

        switch (side)
        {
            case 1:
                StartCoroutine(rotate(top, 90, true, true, side + 1));
                break;
            case 2:
                StartCoroutine(rotate(left, 90, true, false, side + 1));
                break;
            case 3:
                StartCoroutine(rotate(right, 270, false, false, side + 1));
                break;
            case 4:
                StartCoroutine(rotate(botOne, 270, false, true, side + 1));
                break;
            case 5:
                StartCoroutine(rotate(botTwo, 270, false, true, side + 1));
                break;
            case 6:
                Debug.Log("Done");
                break;
        }


    }


    private IEnumerator rotate(GameObject piv, float rot, bool plus, bool x, int next)
    {
        if (x)
        {
            while (Mathf.Floor(piv.transform.localEulerAngles.x) != rot)
            {
                if (plus)
                {
                    piv.transform.localEulerAngles = new Vector3(piv.transform.localEulerAngles.x + 1, 0, 0);
                }
                else
                {
                    piv.transform.localEulerAngles = new Vector3(piv.transform.localEulerAngles.x - 1, 0, 0);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            while (Mathf.Floor(piv.transform.localEulerAngles.z) != rot)
            {
                if (plus)
                {
                    piv.transform.localEulerAngles = new Vector3(0, 0, piv.transform.localEulerAngles.z + 1);

                }
                else
                {
                    piv.transform.localEulerAngles = new Vector3(0, 0, piv.transform.localEulerAngles.z - 1);
                }

                yield return new WaitForSeconds(0.1f);

            }
        }
        folder(next);
    }
}
