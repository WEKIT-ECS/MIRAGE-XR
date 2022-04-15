using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeDebugRay : MonoBehaviour
{
  public static RuntimeDebugRay instance;
  private List<LineRenderer> lines;
  private List<Ray> rays;

  private void Awake(){
    if(instance == null)
      instance = this;

    //Generate pool of lines
    lines = new List<LineRenderer>();
    rays = new List<Ray>();
    CreateLines(20);
  }

  private void OnDestroy(){
    instance = null;
  }

  public void DrawRay(Ray ray){
    rays.Add(ray);
  }

  private void CreateLines(int amount){
    for(var i=0; i<amount; i++){
      var line = new GameObject("Line " + lines.Count).AddComponent<LineRenderer>();
      line.endWidth = 0.01f;
      line.startWidth = 0.01f;
      line.transform.SetParent(transform);
      lines.Add(line);
      line.gameObject.SetActive(false);
    }
  }

  private void Update(){
    if(rays.Count > lines.Count)
      CreateLines(rays.Count - lines.Count);

    for(var i=0; i<lines.Count; i++){
      lines[i].gameObject.SetActive(false);
    }

    for(var i=0; i<rays.Count; i++){
      lines[i].SetPositions(new Vector3[]{rays[i].origin, rays[i].origin + (rays[i].direction * 2)});
      lines[i].gameObject.SetActive(true);
    }

    rays.Clear();
  }
}
