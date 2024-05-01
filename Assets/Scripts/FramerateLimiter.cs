using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateLimiter : MonoBehaviour
{
    [SerializeField] private int maxFramerate;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFramerate;
    }
}
