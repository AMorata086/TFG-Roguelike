using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Button resumeButton;
    private Button quitButton;
    public bool IsPaused;

    private void OnEnable()
    {
        IsPaused = false;
        document.rootVisualElement.style.display = DisplayStyle.None;
        VisualElement root = document.rootVisualElement;
        resumeButton = root.Q<Button>("ResumeButton");
        quitButton = root.Q<Button>("QuitButton");

        resumeButton.RegisterCallback<ClickEvent>(OnClickResumeGame);
        quitButton.RegisterCallback<ClickEvent>(QuitToMenu);
    }

    public void OpenPauseMenu()
    {
        IsPaused = true;
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void OnClickResumeGame(ClickEvent clickEvent)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        document.rootVisualElement.style.display = DisplayStyle.None;
    }
    

    private void QuitToMenu(ClickEvent clickEvent)
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadScene(Loader.Scene.MainMenuScene);
    }
}
