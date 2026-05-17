using UnityEngine;

namespace Architect.Util;

public class LocalTrailRenderer : MonoBehaviour
{
    public const float NewPositionDistance = 0.04f;

    public LineRenderer lineRenderer;
    private Vector3 _lastPos;

    public void Reset()
    {
        lineRenderer.positionCount = 0;
        AddPoint(transform.localPosition);
    }

    private void Start()
    {
        lineRenderer.useWorldSpace = false;
        Reset();
    }


    private void Update()
    {
        var currentPosition = transform.localPosition;
        if (Vector3.Distance(currentPosition, _lastPos) > NewPositionDistance) AddPoint(currentPosition);
    }

    private void AddPoint(Vector3 newPoint)
    {
        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPoint);

        _lastPos = newPoint;
    }
}