using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // กด PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene("MapGame");
    }

    // เปิดหน้า Settings
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // กลับหน้า Main Menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ออกจากเกม
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}