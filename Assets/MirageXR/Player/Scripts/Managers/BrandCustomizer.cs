using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BrandCustomizer : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(Init());
    }


    IEnumerator Init()
    {
        yield return new WaitForSeconds(1);

        if (!BrandManager.Instance || !BrandManager.Instance.Customizable) yield break;

        if (GetComponent<Button>())
        {
            Color newSecondaryColor = BrandManager.Instance.GetSecondaryColor();
            Button btn = GetComponent<Button>();
            ColorBlock colors = btn.colors;

            var factor = 0.7f;
            Color darkerColor = new Color(newSecondaryColor.r * factor, newSecondaryColor.g * factor, newSecondaryColor.b * factor, newSecondaryColor.a);
            colors.pressedColor = darkerColor;
            colors.highlightedColor = newSecondaryColor;

            if (!GetComponent<SessionButton>() && !GetComponent<ActionListItem>())
            {
                colors.normalColor = newSecondaryColor;
                colors.highlightedColor = darkerColor;
            }

            btn.colors = colors;
        }
        if (GetComponent<Image>())
        {
            if (GetComponent<Button>() && GetComponent<Button>().interactable)
                GetComponent<Image>().color = Color.white;
            else
                GetComponent<Image>().color = BrandManager.Instance.GetIconColor();
        }

        if (GetComponent<Text>())
        {
            if (GetComponentInParent<Button>())
            {
                if (GetComponentInParent<Button>().interactable)
                {
                    GetComponent<Text>().color = BrandManager.Instance.GetTextColor();
                }
            }
            else
            {
                GetComponent<Text>().color = BrandManager.Instance.GetTextColor();
            }
        }

        if (name == "floorTarget" && GetComponent<SpriteRenderer>())
            GetComponent<SpriteRenderer>().color = BrandManager.Instance.GetTaskStationColor();
    } 

}
