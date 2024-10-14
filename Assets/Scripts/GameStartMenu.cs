using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject levelEditor;

    void Start()
    {
        EnableMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        HideAll();
        SceneManager.LoadScene(1);
    }

    public void HideAll()
    {
        mainMenu.SetActive(false);
        levelEditor.SetActive(false);
    }

    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        levelEditor.SetActive(false);
    }

    public void EnableLevelEditor()
    {
        mainMenu.SetActive(false);
        levelEditor.SetActive(true);
    }

    public void SaveAndReturnToMainMenu()
    {
        EnableMainMenu();
    }
}