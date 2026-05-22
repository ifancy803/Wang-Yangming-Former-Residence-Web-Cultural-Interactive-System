using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LifeClick : MonoBehaviour,IPointerClickHandler
{
    [Description]
    public string lifeText;

    public TMP_Text Text;

    public void OnPointerClick(PointerEventData eventData)
    {
        Text.text = lifeText;
        SyncDissolveScroll.Instance.StartScrollAnimation();
    }
}
