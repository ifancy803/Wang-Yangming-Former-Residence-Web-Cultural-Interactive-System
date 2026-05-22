using UnityEngine;
using UnityEngine.UI;

public class FlowArrowFadeLoop : MonoBehaviour
{
    public RawImage arrowImage;
    public float scrollSpeed = 0.5f;      // 滚动速度
    public float fadeWidth = 0.2f;        // 淡入淡出宽度（0~0.5）
    public Vector2 scrollDirection = Vector2.right;
    
    private float scrollOffset = 0f;
    
    void Update()
    {
        // 1. 滚动 UV（实现箭头移动）
        scrollOffset += scrollSpeed * Time.deltaTime;
        Rect uvRect = arrowImage.uvRect;
        uvRect.position = scrollDirection * scrollOffset;
        arrowImage.uvRect = uvRect;
        
        // 2. 动态计算透明度（模拟淡入淡出）
        // 原理：根据 UV 在屏幕上的位置，边缘变透明
        Vector2 screenUV = GetScreenUV();
        float alpha = CalculateFadeAlpha(screenUV);
        
        Color color = arrowImage.color;
        color.a = alpha;
        arrowImage.color = color;
    }
    
    // 获取 RawImage 在世界空间/屏幕空间的 UV 位置
    Vector2 GetScreenUV()
    {
        // 对于 UI 元素，用本地坐标转屏幕 UV
        RectTransform rect = arrowImage.rectTransform;
        Vector3 worldPos = rect.TransformPoint(rect.rect.center);
        // 简化版：用 x 坐标做淡入淡出
        return new Vector2(worldPos.x / Screen.width, worldPos.y / Screen.height);
    }
    
    float CalculateFadeAlpha(Vector2 uv)
    {
        // 根据滚动方向决定淡入淡出的轴
        float pos;
        if (scrollDirection.x != 0)
            pos = uv.x;
        else
            pos = uv.y;
        
        // 计算到屏幕两端的距离
        float distToLeft = pos;
        float distToRight = 1 - pos;
        float edgeDist = Mathf.Min(distToLeft, distToRight);
        
        // 边缘淡入淡出
        float fade = Mathf.Clamp01(edgeDist / fadeWidth);
        return fade;
    }
}