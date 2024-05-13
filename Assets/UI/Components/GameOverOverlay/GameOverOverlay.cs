using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class GameOverOverlay : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Label gameOverLabel;
    private Label subtextLabel;
    private Button quitToMenuButton;

    private void OnEnable()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
        gameOverLabel = document.rootVisualElement.Q<Label>("GameOverLabel");
        subtextLabel = document.rootVisualElement.Q<Label>("SubtextLabel");
        quitToMenuButton = document.rootVisualElement.Q<Button>("QuitToMenuButton");

        GameManager.Instance.OnPlayerDeath += GameManager_OnPlayerDeath;
        GameManager.Instance.OnReachedGoal += GameManager_OnGameFinishedSuccessfully;
        quitToMenuButton.RegisterCallback<ClickEvent>(QuitToMenu);
    }

    private void QuitToMenu(ClickEvent evt)
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadScene(Loader.Scene.MainMenuScene);
    }

    private void GameManager_OnGameFinishedSuccessfully(object sender, System.EventArgs e)
    {
        string gameOverLabelText = "CONGRATULATIONS!";
        string subtextLabelText = "Thank you for playing!\nI hope you have enjoyed this demo!\n^o^";
        SetLabelsText(gameOverLabelText, subtextLabelText);
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void GameManager_OnPlayerDeath(object sender, System.EventArgs e)
    {
        string gameOverLabelText = "GAME OVER!";
        string subtextLabelText = "Better luck next time!\n>_<";
        SetLabelsText(gameOverLabelText, subtextLabelText);
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void SetLabelsText(string gameOverLabelText, string subtextLabelText)
    {
        gameOverLabel.text = gameOverLabelText;
        subtextLabel.text = subtextLabelText;
    }
}
