using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelSizeController : MonoBehaviour
{
    [SerializeField] private BoxCollider textCollider;
    [SerializeField] private RectTransform textRectTransfrom;


    private void OnRectTransformDimensionsChange()
    {
        var textRect = textRectTransfrom.rect;

        textCollider.size = new Vector3(textRect.width, textRect.height, 1);
        textCollider.center = new Vector3(0, -(textRect.height / 2));
    }



}
