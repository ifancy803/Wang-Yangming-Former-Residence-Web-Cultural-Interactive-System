using UnityEngine;
using UnityEngine.EventSystems;

public class UIPuzzleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PuzzleSlot targetSlot;
    public float snapRange = 50f;

    private Vector2 originPos;
    private RectTransform rect;
    private PuzzlePiece puzzlePiece;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        puzzlePiece = GetComponent<PuzzlePiece>();
        originPos = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (puzzlePiece != null && puzzlePiece.isFinish) return;
        rect.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (puzzlePiece != null && puzzlePiece.isFinish)
        {
            return;
        }

        if (targetSlot == null)
        {
            rect.anchoredPosition = originPos;
            return;
        }

        // 直接吸附到空位上设置的 Vector3
        Vector2 targetPos = targetSlot.customSnapPosition;
        float distance = Vector2.Distance(rect.anchoredPosition, targetPos);

        if (distance <= snapRange)
        {
            // ✅ 精准吸附到你设定的坐标
            rect.anchoredPosition = targetPos;

            if (puzzlePiece != null)
                puzzlePiece.isFinish = true;
        }
        else
        {
            rect.anchoredPosition = originPos;
        }
    }
}