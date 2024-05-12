using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    // Define a simple state machine to control the current state
    public enum State
    {
        SelectingGamemode,
        WaitingToStart,
        InGame,
        GameFinished
    }

    private struct Wave
    {
        // public int numberOfWaves;
        public int[] enemiesInWave;

        public Wave(int[] enemiesInWave)
        {
            // this.numberOfWaves = numberOfWaves;
            this.enemiesInWave = enemiesInWave;
        }
    }

    [Space]
    [Header("Objects of this script")]
    public NetworkVariable<State> currentState = new NetworkVariable<State>(State.SelectingGamemode);
    private bool haveAllPlayersSpawned = false;
    private int currentEnemies = 0;
    [Space]
    [Header("Reference to the players")]
    private PlayerController hostPlayerController;
    private PlayerController clientPlayerController;
    [Space]
    [Header("References to prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyMeleePrefab;
    [SerializeField] private GameObject enemyRangedPrefab;
    [SerializeField] private GameObject enemyFlyingPrefab;
    [Space]
    [Header("References to other objects")]
    [SerializeField] private GameObject waitingToStartOverlay;
    [SerializeField] private DoorManager doorManager;
    [SerializeField] private RoomManager[] rooms;
    private Wave[][] wavesInRoom;
    [SerializeField] private GameObject healthPackPrefab;
    private GameObject[] healthPacks = new GameObject[2];
    [SerializeField] private Vector3[] healthPackPositions = new Vector3[2];

    public bool IsWaitingToStart()
    {
        return currentState.Value == State.WaitingToStart;
    }

    private void InitializeRooms()
    {
        wavesInRoom = new Wave[rooms.Length][];
        // iterate each room in the array 
        for (int i = 0; i < rooms.Length; i++)
        {
            // retrieve the max number of enemies per room
            int maxNumberOfEnemies = rooms[i].spawnPoints.Length;
            // fill in the wave with enemies
            wavesInRoom[i] = new Wave[rooms[i].numberOfWaves];
            // iterate each wave in that specific room
            for (int j = 0; j < rooms[i].numberOfWaves; j++)
            {
                int[] enemiesInWave = new int[maxNumberOfEnemies];
                for (int k = 0; k < enemiesInWave.Length; k++)
                {
                    enemiesInWave[k] = UnityEngine.Random.Range(1, rooms[i].enemiesAllowed + 1);
                }
                wavesInRoom[i][j] = new Wave(enemiesInWave);
            }
        }
    }

    public void DecrementCurrentEnemies()
    {
        DecrementCurrentEnemiesServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecrementCurrentEnemiesServerRpc()
    {
        currentEnemies--;
    }

    // when called spawn the waves of enemies of that room in an order
    public IEnumerator SpawnEnemies(int roomNumber)
    {
        // close the doors when the player enters the room
        SwitchDoorsStateClientRpc();
        
        for(int i = 0; i < rooms[roomNumber].numberOfWaves; i++)
        {
            currentEnemies = wavesInRoom[roomNumber][i].enemiesInWave.Length;
            // Spawn the enemies of the "i" wave
            for (int j = 0; j < wavesInRoom[roomNumber][i].enemiesInWave.Length; j++)
            {
                switch (wavesInRoom[roomNumber][i].enemiesInWave[j])
                {
                    case 1:
                        GameObject enemyMeleeSpawned = Instantiate(enemyMeleePrefab, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        NetworkObject enemyMeleeSpawnedNetworkObject = enemyMeleeSpawned.GetComponent<NetworkObject>();
                        enemyMeleeSpawnedNetworkObject.Spawn(true);
                        break;
                    case 2:
                        GameObject enemyRangedSpawned = Instantiate(enemyRangedPrefab, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        NetworkObject enemyRangedSpawnedNetworkObject = enemyRangedSpawned.GetComponent<NetworkObject>();
                        enemyRangedSpawnedNetworkObject.Spawn(true);
                        break;
                    case 3:
                        GameObject enemyFlyingSpawned = Instantiate(enemyFlyingPrefab, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        NetworkObject enemyFlyingSpawnedNetworkObject = enemyFlyingSpawned.GetComponent<NetworkObject>();
                        enemyFlyingSpawnedNetworkObject.Spawn(true);
                        break;
                }
            }

            // don't spawn the next wave till all enemies of the current one have perished
            while (currentEnemies > 0)
            {
                yield return null;
            }
        }

        // open the doors when all enemies have been killed
        SwitchDoorsStateClientRpc();

        yield return null;
    }

    [ClientRpc]
    private void SwitchDoorsStateClientRpc()
    {
        doorManager.SwitchDoorState();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }

        currentState.OnValueChanged += State_OnValueChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

        InitializeRooms();
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerGameObject = Instantiate(playerPrefab);
            playerGameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, false); // we don't want to destroy on load since there might be other levels
        }

        haveAllPlayersSpawned = true;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
        Debug.Log(previousValue + " --> " + newValue);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(IsServer)
        {
            for (int i = 0; i < healthPacks.Length; i++)
            {
                healthPacks[i] = Instantiate(healthPackPrefab);
                healthPacks[i].transform.position = healthPackPositions[i];
                NetworkObject healthPackNetworkObject = healthPacks[i].GetComponent<NetworkObject>();
                healthPackNetworkObject.Spawn(true);
            }
        }
        
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }

        switch(currentState.Value)
        {
            case State.SelectingGamemode:
                if (haveAllPlayersSpawned)
                {
                    currentState.Value = State.WaitingToStart;
                }
                break;
            case State.WaitingToStart:
                // things that should execute before starting the game
                SetWaitingToStartOverlayActiveStateClientRpc(true);
                hostPlayerController = GameObject.FindGameObjectWithTag("Player_1").GetComponent<PlayerController>();
                GameObject clientPlayerGameObject = GameObject.FindGameObjectWithTag("Player_2");
                if (clientPlayerGameObject == null)
                {
                    if(hostPlayerController.CanMove.Value)
                    {
                        currentState.Value = State.InGame;
                    }
                }
                else
                {
                    clientPlayerController = clientPlayerGameObject.GetComponent<PlayerController>();
                    if (hostPlayerController.CanMove.Value && clientPlayerController.CanMove.Value)
                    {
                        currentState.Value = State.InGame;
                    }
                }
                break;
            case State.InGame:
                SetWaitingToStartOverlayActiveStateClientRpc(false);
                // things that should execute while playing
                break;
            case State.GameFinished:
                // things that should execute upun Game Over
                break;
        }
    }

    [ClientRpc]
    private void SetWaitingToStartOverlayActiveStateClientRpc(bool isActive)
    {
        waitingToStartOverlay.SetActive(isActive);
    }
}
