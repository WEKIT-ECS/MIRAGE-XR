using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class GridGenerator : MonoBehaviour
{
    public float snapIncrement;
    private float renderDistance;

    /// <summary>
    /// Gets the singleton instance of this class.
    /// </summary>
    public static GridGenerator Instance { get; private set; }

    /// <summary>
    /// Gets the <see cref="ParticleSystem"/> attached to the same GameObject.
    /// </summary>
    public ParticleSystem ParticleSystem { get; private set; }

    private void Awake()
    {
        // Assure singleton instance of the class.
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
        ParticleSystem = GetComponent<ParticleSystem>();

        // Currently just a reasonable value. Might want to make adjustable.
        renderDistance = snapIncrement * 3;
    }

    /// <summary>
    /// Renders the grid through the <see cref="ParticleSystem"/>.
    /// </summary>
    /// <param name="snappedPosition">The position around which to render the grid.</param>
    public void RenderGrid(Vector3 snappedPosition)
    {
        ParticleSystem.Clear();

        ParticleSystem.EmitParams ep = new ();

        var positions = DeterminePoints(snappedPosition);

        foreach (var pos in positions)
        {
            ep.position = pos;
            var distance = Vector3.Distance(snappedPosition, pos);
            // Attempted fading with distance. Needs rework.
            //ep.startColor = new Color(ep.startColor.r, ep.startColor.g, ep.startColor.b, renderDistance / distance);
            ParticleSystem.Emit(ep, 1);
        }
    }

    /// <summary>
    /// Calculates the grid points around a position that should be displayed.
    /// </summary>
    /// <param name="snappedPosition">The position around which to calculate the closest points.</param>
    /// <returns>List of Vector3 points that should be displayed with the <see cref="ParticleSystem"/>.</returns>
    private List<Vector3> DeterminePoints(Vector3 snappedPosition)
    {
        var sideLength = 2 * renderDistance / snapIncrement;
        var corner = snappedPosition - new Vector3(sideLength / 2, sideLength / 2, sideLength / 2) * snapIncrement;

        var points = new List<Vector3>();
        for (int i = 0; i < sideLength; i++)
        {
            for (int j = 0; j < sideLength; j++)
            {
                for (int k = 0; k < sideLength; k++)
                {
                    var point = corner + new Vector3(i, j, k) * snapIncrement;

                    if (Vector3.Distance(point, snappedPosition) <= renderDistance)
                    {
                        points.Add(point);
                    }
                }
            }
        }

        return points;
    }
}
