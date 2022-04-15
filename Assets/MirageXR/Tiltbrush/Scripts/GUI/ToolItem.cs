using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltBrush
{
  public class ToolItem : MonoBehaviour
  {
    public enum ToolType{
      Command,
      Tool
    }

    public ToolType type;
    public SketchControlsScript.GlobalCommands command;
    public BaseTool.ToolType tool;
    public bool isToggle = false;
    private MrtkSimpleBtn btn;
    private bool toolEnabled;

    void Awake(){
      btn = GetComponent<MrtkSimpleBtn>();
    }

    void Start(){
      UpdateState();
    }

    private void UpdateState(){
      if(isToggle){
        btn.SetSelected(toolEnabled);
        return;
      }

      btn.SetDisabled(!SketchControlsScript.m_Instance.IsCommandAvailable(command));
    }

    public void OnToolSelected()
    {
      if (toolEnabled)
      {
        if(type == ToolType.Tool){
          SketchSurfacePanel.m_Instance.DisableSpecificTool(tool);
        }
        else
          SketchControlsScript.m_Instance.IssueGlobalCommand(command);

        toolEnabled = false;
      }
      else
      {
        if(type == ToolType.Tool)
          SketchSurfacePanel.m_Instance.EnableSpecificTool(tool);
        else
          SketchControlsScript.m_Instance.IssueGlobalCommand(command);

        toolEnabled = true;
      }

      UpdateState();
    }
  }
}
