using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIScript : MonoBehaviour
{
    [SerializeField] private UIDocument userInterface;
    private Button singlePlayerButton;
    private Button multiplayerButton;
    private Button exitButton;

    private void OnEnable()
    {
        VisualElement root = userInterface.rootVisualElement;
        singlePlayerButton = root.Q<Button>("SinglePlayerButton");
        multiplayerButton = root.Q<Button>("MultiplayerButton");
        exitButton = root.Q<Button>("QuitButton");

        singlePlayerButton.RegisterCallback<ClickEvent>(PlayOffline);
        multiplayerButton.RegisterCallback<ClickEvent>(PlayOnline);
        exitButton.RegisterCallback<ClickEvent>(QuitGame);
    }

    private void PlayOffline(ClickEvent clickEvent)
    {
        MultiplayerBehavior.playMultiplayer = false;
        Loader.LoadScene(Loader.Scene.LobbyScene);
    }

    private void PlayOnline(ClickEvent clickEvent)
    {
        MultiplayerBehavior.playMultiplayer = true;
        Loader.LoadScene(Loader.Scene.LobbyScene);
    }

    private void QuitGame(ClickEvent clickEvent)
    {
        Application.Quit();
    }
}
