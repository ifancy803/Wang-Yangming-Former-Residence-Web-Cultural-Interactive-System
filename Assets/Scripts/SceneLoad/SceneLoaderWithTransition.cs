using System;
using System.Collections;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderWithTransition : Singleton<SceneLoaderWithTransition>
{
    [Header("过渡动画设置")]
    public TransitionAnimator animator;
    
    [Header("测试场景")]
    public string scene1;
    public string scene2;
    
    [Header("过渡时间设置")]
    [Tooltip("场景加载完成后等待的时间（确保所有对象初始化）")]
    public float postLoadDelay = 0.1f;
    
    private bool isTransitioning = false;
    private Coroutine currentCoroutine = null;
    private string targetSceneName = "";

    protected override void Awake()
    {
        base.Awake();
        
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        if (animator != null)
        {
            animator.gameObject.SetActive(false);
            animator.autoPlay = false;  // 完全手动控制，禁止自动播放
            animator.enabled = true;
            animator.sceneNameToLoad = "";  // 关键：清空自动加载的场景名
            animator.onTransitionEnd.RemoveAllListeners();  // 清理所有默认监听
        }
    }
    
    void OnDestroy()
    {
        if (animator != null)
        {
            animator.onTransitionEnd.RemoveAllListeners();
        }
        
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
    }

    /// <summary>
    /// 带过渡效果切换场景
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    public void Fade(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning($"[SceneLoader] 已经在切换场景中 (目标: {targetSceneName})，忽略调用: {sceneName}");
            return;
        }
        
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogWarning($"[SceneLoader] 已经在场景 {sceneName} 中，无需切换");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogError("[SceneLoader] TransitionAnimator 未赋值！直接加载场景");
            SceneManager.LoadScene(sceneName);
            return;
        }
        
        // 停止之前的协程
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        
        // 设置状态
        isTransitioning = true;
        targetSceneName = sceneName;
        
        // 启动完整的过渡协程
        currentCoroutine = StartCoroutine(FullTransitionCoroutine());
        
        Debug.Log($"[SceneLoader] 开始切换场景: {SceneManager.GetActiveScene().name} -> {targetSceneName}");
    }
    
    /// <summary>
    /// 完整的过渡流程协程（推荐使用这种方式，逻辑更清晰）
    /// </summary>
    private IEnumerator FullTransitionCoroutine()
    {
        // 步骤1：播放淡出动画（覆盖当前场景）
        yield return PlayFadeOutCoroutine();
        
        // 步骤2：异步加载新场景
        yield return LoadSceneCoroutine();
        
        // 步骤3：等待场景完全初始化
        yield return new WaitForSeconds(postLoadDelay);
        
        // 步骤4：播放淡入动画（显示新场景）
        yield return PlayFadeInCoroutine();
        
        // 步骤5：完成过渡，重置状态
        FinishTransition();
    }
    
    /// <summary>
    /// 播放淡出动画
    /// </summary>
    private IEnumerator PlayFadeOutCoroutine()
    {
        Debug.Log("[SceneLoader] 开始淡出动画");
        
        animator.gameObject.SetActive(true);
        animator.profile.invert = false;
        animator.progress = 0;
        animator.Play();
        
        // 等待淡出动画完成
        while (animator.isPlaying)
        {
            yield return null;
        }
        
        Debug.Log("[SceneLoader] 淡出动画完成");
    }
    
    /// <summary>
    /// 异步加载场景
    /// </summary>
    private IEnumerator LoadSceneCoroutine()
    {
        Debug.Log($"[SceneLoader] 开始加载场景: {targetSceneName}");
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        
        if (asyncLoad == null)
        {
            Debug.LogError($"[SceneLoader] 无法加载场景: {targetSceneName}");
            yield break;
        }
        
        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 关键：等待两帧，确保场景完全激活并且所有Start/Awake都已执行
        yield return null;
        yield return null;
        
        Debug.Log($"[SceneLoader] 场景加载完成: {targetSceneName}");
    }
    
    /// <summary>
    /// 播放淡入动画
    /// </summary>
    private IEnumerator PlayFadeInCoroutine()
    {
        Debug.Log("[SceneLoader] 开始淡入动画");
        
        animator.profile.invert = true;
        animator.progress = 0;
        animator.Play();
        
        // 等待淡入动画完成
        while (animator.isPlaying)
        {
            yield return null;
        }
        
        Debug.Log("[SceneLoader] 淡入动画完成");
    }
    
    /// <summary>
    /// 完成过渡，重置所有状态
    /// </summary>
    private void FinishTransition()
    {
        Debug.Log($"[SceneLoader] 场景切换成功: {targetSceneName}");
        
        // 重置animator
        if (animator != null)
        {
            animator.profile.invert = false;
            animator.gameObject.SetActive(false);
            animator.onTransitionEnd.RemoveAllListeners();
        }
        
        // 重置状态
        isTransitioning = false;
        targetSceneName = "";
        currentCoroutine = null;
    }
    
    /// <summary>
    /// 检查是否正在切换场景
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    // 测试方法
    [ContextMenu("切换到Scene1")]
    public void TestLoadScene1()
    {
        Fade(scene1);
    }
    
    [ContextMenu("切换到Scene2")]
    public void TestLoadScene2()
    {
        Fade(scene2);
    }
}