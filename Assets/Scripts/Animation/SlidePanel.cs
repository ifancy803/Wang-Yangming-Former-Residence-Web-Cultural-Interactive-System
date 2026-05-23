using UnityEngine;
using UnityEngine.UI;

public class SlidePanel : MonoBehaviour
{
    [Header("滑动动画")]
    public float slideTime = 0.6f; // 滑动总时间
    public float elasticity = 5f; // 弹性强度（越大弹得越明显）

    [Header("位置设置")]
    public Vector2 hiddenPos; // 隐藏的位置
    public Vector2 showPos;  // 显示的位置

    private RectTransform _rect;
    private bool _isOpen = false;
    private bool _isAnimating = false;
    private float _timer = 0;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
        // 初始设置为隐藏状态
        _rect.anchoredPosition = hiddenPos;
    }

    void Update()
    {
        if (!_isAnimating) return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / slideTime);

        // 弹性缓动动画（最舒服的弹性效果）
        float animT = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);

        // 目标位置：打开=显示，关闭=隐藏
        Vector2 targetPos = _isOpen ? showPos : hiddenPos;
        _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, targetPos, animT);

        // 动画结束
        if (t >= 1)
        {
            _isAnimating = false;
            _rect.anchoredPosition = targetPos;
        }
    }

    // 按钮绑定这个方法！
    [ContextMenu("<UNK>")]
    public void ToggleSlide()
    {
        if (_isAnimating) return; // 动画中禁止重复触发

        _isOpen = !_isOpen;
        _timer = 0;
        _isAnimating = true;
    }
}