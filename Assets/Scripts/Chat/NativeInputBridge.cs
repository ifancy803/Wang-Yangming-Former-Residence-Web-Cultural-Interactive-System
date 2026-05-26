using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class NativeInputBridgeTMP : MonoBehaviour, IPointerClickHandler
{
    public TMP_InputField tmpInputField;

    void Start()
    {
        if (tmpInputField == null)
            tmpInputField = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTMPInputClicked();
    }

    public void OnTMPInputClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("ShowNativeInput");
#endif
    }

    // ✅修复：不覆盖，追加打字
    public void OnInputChange(string text)
    {
        if (tmpInputField != null)
        {
            // 原来的文字 + 新输入的文字
            tmpInputField.text = tmpInputField.text + text;
        }
    }
}