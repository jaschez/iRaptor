using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    bool paused = false;

    [SerializeField] GameObject pauseMenu;
    [SerializeField] TMP_Text seedText;

    private void Start()
    {
        pauseMenu.SetActive(false);
        seedText.text = "iRAPTOR-"+GameManagerModule.GetInstance().GetGameSeed();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;

            UpdateMenu();
        }
    }

    public void SetPause(bool p)
    {
        paused = p;

        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (paused)
        {
            Cursor.visible = true;

            Controls.AllowControls(false);

            pauseMenu.SetActive(true);

            Time.timeScale = 0;
        }
        else
        {
            Cursor.visible = false;

            Controls.AllowControls(true);

            pauseMenu.SetActive(false);

            Time.timeScale = 1;
        }
    }
}
