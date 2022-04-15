using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSelector : MonoBehaviour
{
  public GameObject brushesBtn;
  public GameObject colorsBtn;
  public GameObject toolsBtn;

  public GameObject brushesPanel;
  public GameObject colorsPanel;
  public GameObject toolsPanel;

  public void OpenBrushesPanel(){
    colorsPanel.SetActive(false);
    toolsPanel.SetActive(false);
    brushesPanel.SetActive(true);
  }

  public void OpenColorsPanel(){
    brushesPanel.SetActive(false);
    toolsPanel.SetActive(false);
    colorsPanel.SetActive(true);
  }

  public void OpenToolsPanel(){
    colorsPanel.SetActive(false);
    brushesPanel.SetActive(false);
    toolsPanel.SetActive(true);
  }
}
