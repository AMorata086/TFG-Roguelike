using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class DisconnectMenu : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Label disconnectMenuLabel;
    private Button quitToDesktopButton;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        VisualElement rootVisualElement = document.rootVisualElement;
        rootVisualElement.style.display = DisplayStyle.None;
        disconnectMenuLabel = rootVisualElement.Q<Label>("DisconnectMenuLabel");
        quitToDesktopButton = rootVisualElement.Q<Button>("QuitToDesktopButton");

        quitToDesktopButton.RegisterCallback<ClickEvent>(QuitGame);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        bool isHost;

        if (clientId == NetworkManager.ServerClientId)
        {
            isHost = true;
        } 
        else
        {
            isHost = false;
        }

        ShowDisconnectMenu(isHost);
    }

    private void ShowDisconnectMenu(bool isHost)
    {
        string labelText;

        if(isHost)
        {
            labelText = "HOST HAS DISCONNECTED!";
        }
        else
        {
            labelText = "CLIENT HAS DISCONNECTED";
        }
        disconnectMenuLabel.text = labelText;

        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void QuitGame(ClickEvent evt)
    {
        Application.Quit();
    }
}
