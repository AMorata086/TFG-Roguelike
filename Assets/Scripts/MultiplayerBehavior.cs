using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerBehavior : NetworkBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_KEY = "PlayerName";

    public static MultiplayerBehavior Instance { get; private set; }

    public static bool playMultiplayer;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataListChanged;

    private NetworkList<PlayerData> playerDataList;
    private string playerName;

    //[SerializeField] private GameManager gameManager;

    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_KEY, "Player_" + UnityEngine.Random.Range(1, 1000).ToString());

        playerDataList = new NetworkList<PlayerData>();
        playerDataList.OnListChanged += PlayerDataList_OnListChanged;
    }

    private void Start()
    {
        if (!playMultiplayer)
        {
            StartHost();
            Loader.NetworkLoadScene(Loader.Scene.Level1Scene);
        }
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_KEY, playerName);
    }

    private void PlayerDataList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
       OnPlayerDataListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataList.Add(new PlayerData { clientId = clientId });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.PlayerSelectionScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game already started";
            return;
        }

        // control the maximum number of player that can join in a lobby
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2)
        {
            response.Approved = false;
            response.Reason = "Game session is full";
            return;
        }

        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }
}
