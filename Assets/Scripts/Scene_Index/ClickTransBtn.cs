    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickTransBtn : MonoBehaviour
{
    public string sceneName;

    public void GetIndexButtonDown()
    {
        SceneLoaderWithTransition.Instance.Fade(sceneName);
    }
}
