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
        quitToDesktopButton = rootVisualElement.Q<Button>("QuitToMenuButton");

        quitToDesktopButton.RegisterCallback<ClickEvent>(QuitToMenu);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId)
        {
            ShowDisconnectMenu(true);
        } 
        else
        {
            /*
                This causes a non intended behavior when a player tries to join a game that is already in progress. 
                There are 2 options to solve this:
                    - Allowing this behavior, as it is now. This stops the game when a client tries to join a game in progress.
                    - Adding a button to the Disconnect view that only appears if a client disconnects and lets the host to
                        continue playing the game.
             */
            ShowDisconnectMenu(false);
        }
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

    private void QuitToMenu(ClickEvent evt)
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadScene(Loader.Scene.MainMenuScene);
    }
}
