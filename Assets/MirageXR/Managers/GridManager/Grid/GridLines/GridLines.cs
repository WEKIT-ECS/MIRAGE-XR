using UnityEngine;

public class GridLines : MonoBehaviour
{
    [SerializeField] private GridLine _lineX;
    [SerializeField] private GridLine _lineY;
    [SerializeField] private GridLine _lineZ;

    public void DrawLines(Vector3 position)
    {
        _lineX.Draw(position);
        _lineY.Draw(position);
        _lineZ.Draw(position);
    }
}
