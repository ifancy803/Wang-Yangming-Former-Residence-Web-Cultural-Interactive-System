using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class UIUnregularClick : MonoBehaviour, ICanvasRaycastFilter
{
    private PolygonCollider2D _collider;
    private RectTransform _rectTransform;

    void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public bool IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _rectTransform, screenPosition, eventCamera, out Vector3 worldPoint);
        return _collider.OverlapPoint(worldPoint);
    }
}