﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public void startGame()
    {
        SceneManager.LoadScene(1);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
