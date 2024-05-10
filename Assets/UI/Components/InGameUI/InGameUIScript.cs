using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIScript : MonoBehaviour
{
    [Header("User Interface References")]
    [SerializeField] private UIDocument userInterface;
    private Label playerName;
    private VisualElement playerPortrait;
    private VisualElement otherPlayerInfo;
    private VisualElement otherPlayerPortrait;
    private ProgressBar healthBar;
    private ProgressBar otherPlayerHealthBar;
    [Header("References to other objects")]
    [SerializeField] private PlayerSpritesReferences playerSpritesReferences;


    private void OnEnable()
    {
        VisualElement root = userInterface.rootVisualElement;
        playerName = root.Q<Label>("PlayerName");
        playerPortrait = root.Q<VisualElement>("PlayerPortrait");
        otherPlayerInfo = root.Q<VisualElement>("OtherPlayerInfo");
        otherPlayerPortrait = root.Q<VisualElement>("OtherPlayerPortrait");
        healthBar = root.Q<ProgressBar>("HealthPoints");
        otherPlayerHealthBar = root.Q<ProgressBar>("OtherPlayerHealthPoints");
    }

    public void InitializeUserInterface()
    {
        
        GameObject localPlayerGameObject;
        GameObject remotePlayerGameObject;

        // fetch the Game Objects of the players
        localPlayerGameObject = GameObject.FindGameObjectWithTag("Player_1");
        remotePlayerGameObject = GameObject.FindGameObjectWithTag("Player_2");
    
        if(localPlayerGameObject == null)
        {
            Debug.LogError("Error in InitializeUserInterface method: could not find Player 1 GameObject");
            return;
        }
        // check if the Player_2 has been found, if not, then the game is in single player mode
        if(remotePlayerGameObject == null)
        {
            Debug.Log("Player_2 Not Found: initializing interface in single player mode");
            otherPlayerInfo.style.display = DisplayStyle.None;
            playerPortrait.style.backgroundImage = Background.FromSprite(playerSpritesReferences.Player1Sprite);
            playerName.text = localPlayerGameObject.GetComponent<PlayerController>().PlayerName;
        } else
        {
            Debug.Log("Player_2 Found: inizializing interface in multiplayer mode");
            otherPlayerInfo.style.display = DisplayStyle.Flex;
            NetworkObject localPlayerNetworkObject = localPlayerGameObject.GetComponent<NetworkObject>();
            if(localPlayerNetworkObject == null)
            {
                Debug.LogError("Error: couldn't get local player Network Object");
                return;
            }

            if(localPlayerNetworkObject.IsOwner) // localPlayer is Player_1
            {
                // set the UI part of the local player - Player_1
                playerPortrait.style.backgroundImage = Background.FromSprite(playerSpritesReferences.Player1Sprite);
                playerName.text = localPlayerGameObject.GetComponent<PlayerController>().PlayerName;
                
                // set the UI part of the remote player - Player_2
                otherPlayerPortrait.style.backgroundImage = Background.FromSprite(playerSpritesReferences.Player2Sprite);
            }
            else // localPlayer is Player_2
            {
                // change who is the localPlayer and who is the remotePlayer
                localPlayerGameObject = GameObject.FindGameObjectWithTag("Player_2");
                remotePlayerGameObject = GameObject.FindGameObjectWithTag("Player_1");

                // set the UI part of the local player - Player_2
                playerPortrait.style.backgroundImage = Background.FromSprite(playerSpritesReferences.Player2Sprite);
                playerName.text = localPlayerGameObject.GetComponent<PlayerController>().PlayerName;

                // set the UI part of the remote player - Player_1
                otherPlayerPortrait.style.backgroundImage = Background.FromSprite(playerSpritesReferences.Player1Sprite);
            }
        }
    }

    public void updateHealthBar(int healthPoints, int maxHealth)
    {
        healthBar.value = healthPoints;
        healthBar.highValue = maxHealth;
        healthBar.title = healthPoints + "/" + maxHealth;
    }

    public void updateOtherPlayerHealthBar(int otherPlayerHealthPoints, int otherPlayerMaxHealth)
    {
        otherPlayerHealthBar.value = otherPlayerHealthPoints;
        otherPlayerHealthBar.highValue = otherPlayerMaxHealth;
        otherPlayerHealthBar.title = otherPlayerHealthPoints + "/" + otherPlayerMaxHealth;
    }

}
