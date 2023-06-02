using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace MirageXR
{
    public class BrandCustomizer : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(Init());
        }


        private IEnumerator Init()
        {
            yield return new WaitForSeconds(1);

            if (!BrandManager.Instance || !BrandManager.Instance.Customizable)
            {
                yield break;
            }

            if (GetComponent<Button>())
            {
                var newSecondaryColor = BrandManager.Instance.SecondaryColor;
                var btn = GetComponent<Button>();
                var colors = btn.colors;

                var factor = 0.7f;
                var darkerColor = new Color(newSecondaryColor.r * factor, newSecondaryColor.g * factor, newSecondaryColor.b * factor, newSecondaryColor.a);
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
                {
                    GetComponent<Image>().color = Color.white;
                }
                else
                {
                    GetComponent<Image>().color = BrandManager.Instance.IconColor;
                }
            }

            if (GetComponent<Text>())
            {
                if (GetComponentInParent<Button>())
                {
                    if (GetComponentInParent<Button>().interactable)
                    {
                        GetComponent<Text>().color = BrandManager.Instance.TextColor;
                    }
                }
                else
                {
                    GetComponent<Text>().color = BrandManager.Instance.TextColor;
                }
            }

            if (name == "floorTarget" && GetComponent<SpriteRenderer>())
            {
                GetComponent<SpriteRenderer>().color = BrandManager.Instance.TaskStationColor;
            }
        }
    }
}
