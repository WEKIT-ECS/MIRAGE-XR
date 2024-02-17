using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiRope))]
public class RopeSweepCut : MonoBehaviour
{

    public Camera cam;

    ObiRope rope;
    LineRenderer lineRenderer;
    Vector3 cutStartPosition;

    private void Awake()
    {
        rope = GetComponent<ObiRope>();

        AddMouseLine();
    }

    private void OnDestroy()
    {
        DeleteMouseLine();
    }

    private void AddMouseLine()
    {
        GameObject line = new GameObject("Mouse Line");
        lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.numCapVertices = 2;
        lineRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.sharedMaterial.color = Color.cyan;
        lineRenderer.enabled = false;
    }

    private void DeleteMouseLine()
    {
        if (lineRenderer != null)
            Destroy(lineRenderer.gameObject);
    }

    private void LateUpdate()
    {
        // do nothing if we don't have a camera to cut from.
        if (cam == null) return;

        // process user input and cut the rope if necessary.
        ProcessInput();
    }

    /**
     * Very simple mouse-based input. Not ideal for multitouch screens as it only supports one finger, though.
     */
    private void ProcessInput()
    {
        // When the user clicks the mouse, start a line cut:
        if (Input.GetMouseButtonDown(0))
        {
            cutStartPosition = Input.mousePosition;
            lineRenderer.SetPosition(0, cam.ScreenToWorldPoint(new Vector3(cutStartPosition.x, cutStartPosition.y, 0.5f)));
            lineRenderer.enabled = true;
        }

        if (lineRenderer.enabled)
            lineRenderer.SetPosition(1, cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f)));

        // When the user lifts the mouse, proceed to cut.
        if (Input.GetMouseButtonUp(0))
        {
            ScreenSpaceCut(cutStartPosition, Input.mousePosition);
            lineRenderer.enabled = false;
        }
    }


    /**
     * Cuts the rope using a line segment, expressed in screen-space.
     */
    private void ScreenSpaceCut(Vector2 lineStart, Vector2 lineEnd)
    {
        // keep track of whether the rope was cut or not.
        bool cut = false;

        // iterate over all elements and test them for intersection with the line:
        for (int i = 0; i < rope.elements.Count; ++i)
        {
            // project the both ends of the element to screen space.
            Vector3 screenPos1 = cam.WorldToScreenPoint(rope.solver.positions[rope.elements[i].particle1]);
            Vector3 screenPos2 = cam.WorldToScreenPoint(rope.solver.positions[rope.elements[i].particle2]);

            // test if there's an intersection:
            if (SegmentSegmentIntersection(screenPos1, screenPos2, lineStart, lineEnd, out float r, out float s))
            {
                cut = true;
                rope.Tear(rope.elements[i]);
            }
        }

        // If the rope was cut at any point, rebuilt constraints:
        if (cut) rope.RebuildConstraintsFromElements();
    }

    /**
     * line segment 1 is AB = A+r(B-A)
     * line segment 2 is CD = C+s(D-C)
     * if they intesect, then A+r(B-A) = C+s(D-C), solving for r and s gives the formula below.
     * If both r and s are in the 0,1 range, it meant the segments intersect.
     */
    private bool SegmentSegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out float r, out float s)
    {
        float denom = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);
        float rNum = (A.y - C.y) * (D.x - C.x) - (A.x - C.x) * (D.y - C.y);
        float sNum = (A.y - C.y) * (B.x - A.x) - (A.x - C.x) * (B.y - A.y);

        if (Mathf.Approximately(rNum, 0) || Mathf.Approximately(denom, 0))
        {  r = -1; s = -1; return false; }

        r = rNum / denom;
        s = sNum / denom;

        return (r >= 0 && r <=1  && s >= 0 && s <= 1);
    }
}
