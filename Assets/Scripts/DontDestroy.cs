using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    // 单例静态实例
    public static DontDestroy Instance { get; private set; }

    private void Awake()
    {
        // 单例核心逻辑：保证全局只有一个实例
        if (Instance == null)
        {
            Instance = this;
            // 切换场景不销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 重复实例自动销毁
            Destroy(gameObject);
        }
    }
}