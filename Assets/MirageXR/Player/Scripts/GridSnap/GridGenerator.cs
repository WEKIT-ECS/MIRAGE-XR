using DG.Tweening.Core.Easing;
using MirageXR;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance { get; private set; }

    public GameObject selectedObject;

    public GameObject gridObject;

    private float renderDistance;

    public float snapIncrement;

    private List<GameObject> gridObjects;
    public ParticleSystem ParticleSystem { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        gridObjects = new List<GameObject>();
        ParticleSystem = GetComponent<ParticleSystem>();

        renderDistance = snapIncrement * 3;
    }

    public void RenderGrid(Vector3 snappedPosition)
    {
        ParticleSystem.Clear();

        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();

        var positions = DeterminePoints(snappedPosition);

        foreach(var pos in positions)
        {
            ep.position = pos;
            var distance = Vector3.Distance(snappedPosition, pos);
            ep.startColor = new Color(ep.startColor.r, ep.startColor.g, ep.startColor.b, renderDistance / distance);
            ParticleSystem.Emit(ep, 1);
        }
    }

    public void RenderGridOld(Vector3 snappedPosition)
    {
        foreach (GameObject go in gridObjects)
        {
            Destroy(go);
        }
        gridObjects.Clear();

        var points = DeterminePoints(snappedPosition);
        foreach (var point in points)
        {
            var obj = Instantiate(gridObject);
            obj.transform.position = point;
            gridObjects.Add(obj);
        }
    }

    private List<Vector3> DeterminePoints(Vector3 snappedPosition)
    {
        var sideLength = 2 * renderDistance / snapIncrement;
        var corner = snappedPosition - new Vector3(sideLength / 2, sideLength / 2, sideLength / 2) * snapIncrement;

        var points = new List<Vector3>();
        for(int i = 0; i < sideLength; i++)
        {
            for(int j = 0; j < sideLength; j++)
            {
                for(int k = 0; k < sideLength; k++)
                {
                    var point = corner + new Vector3(i, j, k) * snapIncrement;

                    if(Vector3.Distance(point, snappedPosition) <= renderDistance)
                        points.Add(point);
                }
            }
        }

        return points;
    }
}
