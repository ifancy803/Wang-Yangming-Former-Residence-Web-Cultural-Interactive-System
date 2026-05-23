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
    public float postLoadDelay = 0.2f;
    
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
            animator.autoPlay = false;
            animator.enabled = true;
            animator.sceneNameToLoad = "";
            animator.onTransitionEnd.RemoveAllListeners();
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
    public void Fade(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] 场景名为空！");
            return;
        }

        // 强制重置所有状态（解决卡死）
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        
        isTransitioning = false;
        targetSceneName = "";

        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.Log($"[SceneLoader] 当前已在场景: {sceneName}");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogError("[SceneLoader] TransitionAnimator 未赋值！");
            SceneManager.LoadScene(sceneName);
            return;
        }
        
        isTransitioning = true;
        targetSceneName = sceneName;
        
        currentCoroutine = StartCoroutine(FullTransitionCoroutine());
        
        Debug.Log($"[SceneLoader] 切换: {SceneManager.GetActiveScene().name} -> {targetSceneName}");
    }
    
    private IEnumerator FullTransitionCoroutine()
    {
        // 淡出
        animator.gameObject.SetActive(true);
        animator.profile.invert = false;
        animator.progress = 0;
        animator.Play();
        while (animator.isPlaying) yield return null;
        
        // 加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        while (!asyncLoad.isDone) yield return null;
        
        yield return new WaitForSeconds(postLoadDelay);
        yield return null;
        yield return null;
        
        // 淡入
        animator.profile.invert = true;
        animator.progress = 0;
        animator.Play();
        while (animator.isPlaying) yield return null;
        
        // 结束
        animator.gameObject.SetActive(false);
        animator.profile.invert = false;
        
        isTransitioning = false;
        targetSceneName = "";
        currentCoroutine = null;
        
        Debug.Log($"[SceneLoader] 切换完成: {targetSceneName}");
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
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