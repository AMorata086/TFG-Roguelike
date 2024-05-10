using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class MultiplayerMenuScript : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private VisualElement mainContainer;
    private VisualElement multiplayerButtonsContainer;
    private Button hostButton;
    private Button clientButton;

    private void OnEnable()
    {
        VisualElement root = document.rootVisualElement;
        hostButton = root.Q<Button>("StartHost");
        clientButton = root.Q<Button>("StartClient");
        mainContainer = root.Q<VisualElement>("MainContainer");
        multiplayerButtonsContainer = root.Q<VisualElement>("MultiplayerButtonsContainer");

        hostButton.RegisterCallback<ClickEvent>(StartHostSession);
        clientButton.RegisterCallback<ClickEvent>(StartClientSession);
    }

    private void StartHostSession(ClickEvent clickEvent)
    {
        NetworkManager.Singleton.StartHost();
        mainContainer.style.display = DisplayStyle.Flex;
        multiplayerButtonsContainer.style.display = DisplayStyle.None;
    }

    private void StartClientSession(ClickEvent clickEvent)
    {
        NetworkManager.Singleton.StartClient();
        mainContainer.style.display = DisplayStyle.Flex;
        multiplayerButtonsContainer.style.display = DisplayStyle.None;
    }
}
