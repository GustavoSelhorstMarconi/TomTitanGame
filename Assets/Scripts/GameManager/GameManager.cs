using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool canNextLevel = false;
    private int nextLevel = 0;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
        if (canNextLevel && Input.GetKeyDown(KeyCode.R))
        {
            LoadNextLevel();
        }
    }
    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    private void Quit()
    {
        Application.Quit();
    }

    public void UnlockNextLevel()
    {
        nextLevel = SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings - 1 ? SceneManager.GetActiveScene().buildIndex + 1 : SceneManager.sceneCountInBuildSettings - 1;
        canNextLevel = true;
    }
}