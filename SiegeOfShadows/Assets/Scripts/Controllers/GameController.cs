using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }
}
