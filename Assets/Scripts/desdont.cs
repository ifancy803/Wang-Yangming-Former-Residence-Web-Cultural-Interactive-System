using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class desdont : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
