using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public PuzzleSlot targetSlot;
    public float checkOffset = 15f;
    [HideInInspector] public bool isFinish;

    public bool IsCorrect()
    {
        Vector2 target = targetSlot.customSnapPosition;
        float dis = Vector2.Distance(GetComponent<RectTransform>().anchoredPosition, target);
        isFinish = dis <= checkOffset;
        return isFinish;
    }
}