using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Button createGameButton;
    private TextField lobbyJoinCode;
    private Button joinGameButton;

    private void OnEnable()
    {
        createGameButton = document.rootVisualElement.Q<Button>("CreateGameButton");
        lobbyJoinCode = document.rootVisualElement.Q<TextField>("LobbyJoinCode");
        joinGameButton = document.rootVisualElement.Q<Button>("JoinGameButton");

        createGameButton.RegisterCallback<ClickEvent>(CreateGame);
        joinGameButton.RegisterCallback<ClickEvent>(JoinGame);
    }

    private void CreateGame(ClickEvent evt)
    {
        
        GameLobby.Instance.CreateLobby(UnityEngine.Random.Range(0, 100000).ToString(), true);
    }

    private void JoinGame(ClickEvent evt)
    {
        string lobbyCode = lobbyJoinCode.value;

        GameLobby.Instance.JoinLobby(lobbyCode);
    }
}
