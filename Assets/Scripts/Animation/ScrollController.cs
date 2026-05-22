using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SyncDissolveScroll : MonoBehaviour
{
    [Header("滚动设置")]
    [SerializeField] private float scrollOutDuration = 1.2f;
    [SerializeField] private float stayDuration = 3f;
    [SerializeField] private float scrollInDuration = 1.2f;
    
    [Header("位置设置")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-1500, 0);
    [SerializeField] private Vector2 shownPosition = new Vector2(0, 0);
    
    [Header("噪声溶解设置")]
    [SerializeField] [Range(1, 30)] private float noiseScale = 8f;
    [SerializeField] [Range(0, 0.2f)] private float edgeSoftness = 0.08f;
    [SerializeField] [Range(0, 0.3f)] private float noiseStrength = 0.15f;
    
    [Header("文字延迟设置")]
    [SerializeField] private bool enableTextDelay = true;
    [SerializeField] private float textDelayDuration = 1f;
    [SerializeField] private float textFadeInDuration = 0.5f;
    [SerializeField] private float textFadeOutDuration = 0.3f;
    [SerializeField] private AnimationCurve textFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("卷轴宽度效果")]
    [SerializeField] private bool enableWidthEffect = true;
    [SerializeField] private float hiddenWidth = 50f;
    [SerializeField] private float shownWidth = 800f;
    
    [Header("移动曲线")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("音效")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] [Range(0, 1)] private float soundVolume = 0.7f;
    
    // 单例
    public static SyncDissolveScroll Instance { get; private set; }
    
    private RectTransform rectTransform;
    private Image mainImage;
    private CanvasGroup parentCanvasGroup;
    private List<Material> dissolveMaterials = new List<Material>();
    private List<Graphic> textGraphics = new List<Graphic>();
    private List<CanvasGroup> textCanvasGroups = new List<CanvasGroup>();
    private Coroutine currentSequence;
    
    private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
    private static readonly int EdgeSoftness = Shader.PropertyToID("_EdgeSoftness");
    private static readonly int NoiseStrength = Shader.PropertyToID("_NoiseStrength");
    
    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        rectTransform = GetComponent<RectTransform>();
        mainImage = GetComponent<Image>();
        
        // 预热Shader
        PrewarmShader();
        
        // 添加CanvasGroup
        parentCanvasGroup = GetComponent<CanvasGroup>();
        if (parentCanvasGroup == null)
            parentCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        parentCanvasGroup.alpha = 0f;
        
        rectTransform.anchoredPosition = hiddenPosition;
        
        if (enableWidthEffect)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.x = hiddenWidth;
            rectTransform.sizeDelta = size;
        }
        
        // 分帧初始化避免卡顿
        StartCoroutine(InitializeOverFrames());
    }
    
    void PrewarmShader()
    {
        Shader dissolveShader = Shader.Find("UI/UniformDissolve");
        if (dissolveShader != null)
        {
            Material tempMat = new Material(dissolveShader);
            tempMat.SetFloat("_DissolveAmount", 0f);
            Destroy(tempMat);
        }
    }
    
    IEnumerator InitializeOverFrames()
    {
        yield return null;
        SetupDissolveMaterials();
        yield return null;
        SetupTextDelay();
        Debug.Log("卷轴初始化完成");
    }
    
    void SetupTextDelay()
    {
        if (!enableTextDelay) return;
        
        textGraphics.Clear();
        textCanvasGroups.Clear();
        
        // 获取所有文字组件
        textGraphics.AddRange(GetComponentsInChildren<Text>(true));
        
        // 支持TextMeshPro
        var tmpTexts = GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var tmp in tmpTexts)
        {
            textGraphics.Add(tmp);
        }
        
        if (textGraphics.Count == 0)
        {
            enableTextDelay = false;
            return;
        }
        
        Debug.Log($"找到 {textGraphics.Count} 个文字组件");
        
        foreach (var text in textGraphics)
        {
            CanvasGroup cg = text.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = text.gameObject.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f;
            textCanvasGroups.Add(cg);
        }
    }
    
    void SetTextAlpha(float alpha)
    {
        if (!enableTextDelay) return;
        
        foreach (var cg in textCanvasGroups)
        {
            if (cg != null)
                cg.alpha = alpha;
        }
    }
    
    void SetupDissolveMaterials()
    {
        Shader dissolveShader = Shader.Find("UI/UniformDissolve");
        if (dissolveShader == null)
        {
            Debug.LogError("找不到 UniformDissolve Shader！");
            return;
        }
        
        List<Graphic> graphics = new List<Graphic>();
        graphics.AddRange(GetComponentsInChildren<Graphic>(true));
        
        if (enableTextDelay)
        {
            graphics.RemoveAll(g => g is Text || g is TMPro.TMP_Text);
        }
        
        Debug.Log($"找到 {graphics.Count} 个图形组件");
        
        foreach (var graphic in graphics)
        {
            if (graphic == null) continue;
            
            Material newMat = new Material(dissolveShader);
            
            if (graphic is Image img && img.sprite != null)
            {
                newMat.mainTexture = img.sprite.texture;
            }
            else if (graphic is RawImage rawImg && rawImg.texture != null)
            {
                newMat.mainTexture = rawImg.texture;
            }
            
            newMat.SetFloat(NoiseScale, noiseScale);
            newMat.SetFloat(EdgeSoftness, edgeSoftness);
            newMat.SetFloat(NoiseStrength, noiseStrength);
            newMat.SetFloat(DissolveAmount, 0f);
            
            graphic.material = newMat;
            dissolveMaterials.Add(newMat);
        }
    }
    
    void SetDissolveAmount(float amount)
    {
        foreach (var mat in dissolveMaterials)
        {
            if (mat != null)
                mat.SetFloat(DissolveAmount, amount);
        }
        
        if (amount <= 0.1f)
        {
            parentCanvasGroup.alpha = 0f;
        }
        else if (amount >= 0.9f)
        {
            parentCanvasGroup.alpha = 1f;
        }
        else
        {
            parentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, (amount - 0.1f) / 0.8f);
        }
    }
    
    [ContextMenu("测试/开始动画")]
    public void StartScrollAnimation()
    {
        if (currentSequence != null)
            StopCoroutine(currentSequence);
        
        currentSequence = StartCoroutine(ScrollSequence());
    }
    
    IEnumerator ScrollSequence()
    {
        Debug.Log($"开始动画 - 文字延迟: {textDelayDuration}秒");
        
        if (openSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position, soundVolume);
        
        SetTextAlpha(0f);
        
        // 阶段1：卷轴展开
        yield return StartCoroutine(AnimateScroll(0f, 1f, scrollOutDuration, true));
        
        // 阶段2：文字延迟出现
        if (enableTextDelay && textGraphics.Count > 0)
        {
            if (textDelayDuration > 0)
                yield return new WaitForSeconds(textDelayDuration);
            
            float elapsed = 0f;
            while (elapsed < textFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = textFadeCurve.Evaluate(elapsed / textFadeInDuration);
                SetTextAlpha(t);
                yield return null;
            }
            SetTextAlpha(1f);
        }
        
        // 阶段3：停留
        yield return new WaitForSeconds(stayDuration);
        
        // 阶段4：收回
        if (closeSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(closeSound, Camera.main.transform.position, soundVolume);
        
        // 文字淡出
        if (enableTextDelay && textGraphics.Count > 0)
        {
            float elapsed = 0f;
            while (elapsed < textFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / textFadeOutDuration;
                SetTextAlpha(1f - t);
                yield return null;
            }
            SetTextAlpha(0f);
        }
        
        // 卷轴收回
        yield return StartCoroutine(AnimateScroll(1f, 0f, scrollInDuration, false));
        
        currentSequence = null;
    }
    
    IEnumerator AnimateScroll(float fromDissolve, float toDissolve, float duration, bool isOpening)
    {
        float elapsed = 0f;
        
        Vector2 startPos = isOpening ? hiddenPosition : shownPosition;
        Vector2 endPos = isOpening ? shownPosition : hiddenPosition;
        float startWidth = isOpening ? hiddenWidth : shownWidth;
        float endWidth = isOpening ? shownWidth : hiddenWidth;
        
        if (isOpening)
            parentCanvasGroup.alpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(elapsed / duration);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            if (enableWidthEffect)
            {
                Vector2 size = rectTransform.sizeDelta;
                size.x = Mathf.Lerp(startWidth, endWidth, t);
                rectTransform.sizeDelta = size;
            }
            
            float dissolve = Mathf.Lerp(fromDissolve, toDissolve, t);
            SetDissolveAmount(dissolve);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
        if (enableWidthEffect)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.x = endWidth;
            rectTransform.sizeDelta = size;
        }
        SetDissolveAmount(toDissolve);
        
        if (toDissolve >= 0.99f)
            parentCanvasGroup.alpha = 1f;
    }
    
    public void SetTextDelay(float delay)
    {
        textDelayDuration = Mathf.Max(0, delay);
    }
    
    [ContextMenu("测试/立即显示")]
    public void ForceShow()
    {
        StopAllCoroutines();
        rectTransform.anchoredPosition = shownPosition;
        SetDissolveAmount(1f);
        parentCanvasGroup.alpha = 1f;
        SetTextAlpha(1f);
        if (enableWidthEffect)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.x = shownWidth;
            rectTransform.sizeDelta = size;
        }
    }
    
    [ContextMenu("测试/立即隐藏")]
    public void ForceHide()
    {
        StopAllCoroutines();
        rectTransform.anchoredPosition = hiddenPosition;
        SetDissolveAmount(0f);
        parentCanvasGroup.alpha = 0f;
        SetTextAlpha(0f);
        if (enableWidthEffect)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.x = hiddenWidth;
            rectTransform.sizeDelta = size;
        }
    }
    
    [ContextMenu("测试/立即收回")]
    public void ForceClose()
    {
        if (currentSequence != null)
            StopCoroutine(currentSequence);
        StartCoroutine(QuickClose());
    }
    
    IEnumerator QuickClose()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        
        Vector2 startPos = rectTransform.anchoredPosition;
        float startWidth = rectTransform.sizeDelta.x;
        float startDissolve = dissolveMaterials.Count > 0 ? dissolveMaterials[0].GetFloat(DissolveAmount) : 1;
        
        SetTextAlpha(0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, hiddenPosition, t);
            
            Vector2 size = rectTransform.sizeDelta;
            size.x = Mathf.Lerp(startWidth, hiddenWidth, t);
            rectTransform.sizeDelta = size;
            
            float dissolve = Mathf.Lerp(startDissolve, 0f, t);
            SetDissolveAmount(dissolve);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = hiddenPosition;
        Vector2 finalSize = rectTransform.sizeDelta;
        finalSize.x = hiddenWidth;
        rectTransform.sizeDelta = finalSize;
        SetDissolveAmount(0f);
        parentCanvasGroup.alpha = 0f;
        currentSequence = null;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        
        foreach (var mat in dissolveMaterials)
        {
            if (mat != null)
                Destroy(mat);
        }
    }
}