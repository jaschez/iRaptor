using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public static class SceneSystem
{
    public static void LoadScene(GameScenes index)
    {
        DOTween.KillAll();
        SceneManager.LoadScene((int)index);
    }

    public enum GameScenes
    {
        MainMenu = 0,
        Lobby = 1,
        Level = 2,
        LoadedLevel = 3
    }
}
