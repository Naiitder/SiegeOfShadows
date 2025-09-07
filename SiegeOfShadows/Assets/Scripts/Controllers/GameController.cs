using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public PlayerMovement player;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        player = FindAnyObjectByType<PlayerMovement>();
    }
}
