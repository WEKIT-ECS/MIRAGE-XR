using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTilingFromLineLength : MonoBehaviour {

	public float scaleFactor = 16;
	LineRenderer line;

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (line.positionCount > 1) {
			float tiling = (line.GetPosition (line.positionCount - 1) - line.GetPosition (0)).magnitude * scaleFactor;
			line.material.mainTextureScale = new Vector2 (tiling, 1);
		}
	}
}
