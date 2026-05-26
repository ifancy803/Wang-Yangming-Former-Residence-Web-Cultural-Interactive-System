using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 加这行

public class ClickScreen : MonoBehaviour
{
    public string sceneName;
    
    void Update()
    {
        // 检测鼠标左键 / 触屏点击（新 Input System）
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SceneLoaderWithTransition.Instance.Fade(sceneName);
        }
    }
}