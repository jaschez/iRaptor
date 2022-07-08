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
        Intro,
        MainMenu,
        Lobby,
        Level,
        LoadedLevel
    }
}
