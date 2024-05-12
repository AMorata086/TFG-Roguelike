using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSelectionUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Button mainMenuButton;
    private TextField lobbyCode;
    private TextField nameInputField;
    private Button readyButton;

    private void OnEnable()
    {
        mainMenuButton = document.rootVisualElement.Q<Button>("MainMenuButton");
        lobbyCode = document.rootVisualElement.Q<TextField>("LobbyCode");
        nameInputField = document.rootVisualElement.Q<TextField>("NameInputField");
        readyButton = document.rootVisualElement.Q<Button>("ReadyButton");

        mainMenuButton.RegisterCallback<ClickEvent>(QuitToMenu);
        readyButton.RegisterCallback<ClickEvent>(PlayerReady);
    }

    private void Start()
    {
        Lobby joinedLobby = GameLobby.Instance.GetLobby();

        SetLobbyCode(joinedLobby.LobbyCode);

        nameInputField.value = MultiplayerBehavior.Instance.GetPlayerName();
        nameInputField.RegisterValueChangedCallback(ChangePlayerName);
    }

    private void ChangePlayerName(ChangeEvent<string> evt)
    {
        MultiplayerBehavior.Instance.SetPlayerName(evt.newValue);
    }

    private void SetLobbyCode(string lobbyCode)
    {
        this.lobbyCode.value = lobbyCode;
    }

    private void QuitToMenu(ClickEvent evt)
    {
        GameLobby.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Loader.LoadScene(Loader.Scene.MainMenuScene);
    }

    private void PlayerReady(ClickEvent evt)
    {
        PlayerSelectReady.Instance.SetPlayerReady(nameInputField.value);
    }
}
