using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSystem
{
    public static void LoadScene(GameScenes index)
    {
        SceneManager.LoadScene((int)index);
    }

    public enum GameScenes
    {
        MainMenu = 0,
        Lobby = 1,
        Level = 2
    }
}
