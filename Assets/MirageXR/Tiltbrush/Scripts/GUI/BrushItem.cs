using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.UI;

namespace TiltBrush
{
  public class BrushItem : MonoBehaviour
  {
    public Image icon;
    public Text brushName;

    private BrushDescriptor brush;
    private MrtkSimpleBtn btn;
    private Image btnImage;

    public BrushDescriptor Brush => brush;

    private ColorBlock defaultColor;
    private ColorBlock selectedColor;

    void Awake(){
      btn = gameObject.GetComponent<MrtkSimpleBtn>();
      btnImage = gameObject.GetComponent<Image>();
    }

    //Set the selectable colorMultiplier to 0 so it doesn't erase the selected color
    public void SetSelected(bool selected){
      btn.SetSelected(selected);
    }

    public void SetInfo(BrushDescriptor brush){
      this.brush = brush;
      if(brush == null)
        return;

      icon.sprite = Sprite.Create(brush.m_ButtonTexture, new Rect(0, 0, brush.m_ButtonTexture.width, brush.m_ButtonTexture.height), Vector2.zero);
      brushName.text = brush.m_DurableName;
    }

    public void SelectBrush(){
      BrushController.m_Instance.SetActiveBrush(brush);
    }
  }
}
