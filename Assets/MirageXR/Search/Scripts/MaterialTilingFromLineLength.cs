using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTilingFromLineLength : MonoBehaviour
{

    [SerializeField] private float scaleFactor = 16;
    private LineRenderer _line;

    // Use this for initialization
    void Start()
    {
        _line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_line.positionCount > 1)
        {
            float tiling = (_line.GetPosition(_line.positionCount - 1) - _line.GetPosition(0)).magnitude * scaleFactor;
            _line.material.mainTextureScale = new Vector2(tiling, 1);
        }
    }
}
