using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectingOverlay : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Label responseLabel;
    private Button quitToMenuButton;

    private void OnEnable()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
        responseLabel = document.rootVisualElement.Q<Label>("ResponseLabel");
        quitToMenuButton = document.rootVisualElement.Q<Button>("QuitToMenuButton");

        quitToMenuButton.RegisterCallback<ClickEvent>(QuitToMenu);
    }

    private void Start()
    {
        MultiplayerBehavior.Instance.OnFailedToJoinGame += MultiplayerBehavior_OnFailedToJoinGame;
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoinLobbyStarted += GameLobby_OnJoinLobbyStarted;
        GameLobby.Instance.OnJoinLobbyFailed += GameLobby_OnJoinLobbyFailed;
    }

    private void GameLobby_OnJoinLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to join to this lobby.");
    }

    private void GameLobby_OnJoinLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Trying to join the lobby...");
    }

    private void GameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to create a new lobby.");
    }

    private void GameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating lobby...");
    }

    private void MultiplayerBehavior_OnFailedToJoinGame(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void MultiplayerBehavior_OnTryingToJoinGame(object sender, EventArgs e)
    {
        ShowMessage("Connecting...");
    }

    private void ShowMessage(string message)
    {
        responseLabel.text = message;
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void QuitToMenu(ClickEvent evt)
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadScene(Loader.Scene.MainMenuScene);
    }

    private void OnDestroy()
    {
        MultiplayerBehavior.Instance.OnTryingToJoinGame -= MultiplayerBehavior_OnTryingToJoinGame;
        MultiplayerBehavior.Instance.OnFailedToJoinGame -= MultiplayerBehavior_OnFailedToJoinGame;
        GameLobby.Instance.OnCreateLobbyStarted -= GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed -= GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoinLobbyStarted -= GameLobby_OnJoinLobbyStarted;
        GameLobby.Instance.OnJoinLobbyFailed -= GameLobby_OnJoinLobbyFailed;
    }
}
