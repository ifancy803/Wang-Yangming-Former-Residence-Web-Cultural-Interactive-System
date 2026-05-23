using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))] // 强制带渲染组件，避免报错
public class FadeInOutAutoDisable : MonoBehaviour
{
    [Header("淡入淡出总时长")]
    public float fadeDuration = 0.5f;

    [Header("淡入后等待时间")]
    public float waitTime = 1f;

    // 物体启用时自动执行
    private void OnEnable()
    {
        StartCoroutine(FadeSequence());
    }

    /// <summary>
    /// 淡入 → 等待 → 淡出 → 禁用
    /// </summary>
    private IEnumerator FadeSequence()
    {
        var render = GetComponent<CanvasRenderer>();
        if (render == null) yield break;

        // 1. 淡入
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            render.SetAlpha(Mathf.Lerp(0, 1, elapsed / fadeDuration));
            yield return null;
        }
        render.SetAlpha(1);

        // 2. 等待1秒
        yield return new WaitForSeconds(waitTime);

        // 3. 淡出
        elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            render.SetAlpha(Mathf.Lerp(1, 0, elapsed / fadeDuration));
            yield return null;
        }
        render.SetAlpha(0);

        // 4. 禁用物体
        gameObject.SetActive(false);
    }
}