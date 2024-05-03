using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // Define a simple state machine to control the current state
    private enum State
    {
        Idle,
        InBattle,
        ReachedGoal
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
    private State currentState;
    private int currentEnemies = 0;
    [Space]
    [Header("Reference to the players")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [Space]
    [Header("References to prefabs")]
    [SerializeField] private GameObject enemyMelee;
    [SerializeField] private GameObject enemyRanged;
    [SerializeField] private GameObject enemyFlying;
    [Space]
    [Header("References to other objects")]
    [SerializeField] private DoorManager doorManager;
    [SerializeField] private RoomManager[] rooms;
    private Wave[][] wavesInRoom;
    [SerializeField] private GameObject healthPackPrefab;
    private GameObject[] healthPacks = new GameObject[2];
    [SerializeField] private Vector3[] healthPackPositions = new Vector3[2];

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
                    enemiesInWave[k] = Random.Range(1, rooms[i].enemiesAllowed + 1);
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
        currentState = State.InBattle;
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
                        GameObject enemyMeleeSpawned = Instantiate(enemyMelee, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        NetworkObject enemyMeleeSpawnedNetworkObject = enemyMeleeSpawned.GetComponent<NetworkObject>();
                        enemyMeleeSpawnedNetworkObject.Spawn(true);
                        break;
                    case 2:
                        GameObject enemyRangedSpawned = Instantiate(enemyRanged, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        NetworkObject enemyRangedSpawnedNetworkObject = enemyRangedSpawned.GetComponent<NetworkObject>();
                        enemyRangedSpawnedNetworkObject.Spawn(true);
                        break;
                    case 3:
                        GameObject enemyFlyingSpawned = Instantiate(enemyFlying, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
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
        currentState = State.Idle;

        yield return null;
    }

    [ClientRpc]
    private void SwitchDoorsStateClientRpc()
    {
        doorManager.SwitchDoorState();
    }
    

    private void Awake()
    {
        currentState = State.Idle;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }
        InitializeRooms();
        for(int i = 0; i < healthPacks.Length; i++)
        {
            healthPacks[i] = Instantiate(healthPackPrefab);
            healthPacks[i].transform.position = healthPackPositions[i];
            NetworkObject healthPackNetworkObject = healthPacks[i].GetComponent<NetworkObject>();
            healthPackNetworkObject.Spawn(true);
        }
    }
}
