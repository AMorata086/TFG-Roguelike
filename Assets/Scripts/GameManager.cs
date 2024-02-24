using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
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

        public Wave(int numberOfWaves, int[] enemiesInWave)
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

    private void InitializeRooms()
    {
        wavesInRoom = new Wave[rooms.Length][];
        // iterate each room in the array 
        for (int i = 0; i < rooms.Length; i++)
        {
            // retrieve the max number of enemies per room
            int maxNumberOfEnemies = rooms[i].spawnPoints.Length;
            // fill in the wave with enemies
            int[] enemiesInWave = new int[maxNumberOfEnemies];
            for (int k = 0; k < enemiesInWave.Length; k++)
            {
                enemiesInWave[k] = Random.Range(1, rooms[i].enemiesAllowed + 1);
            }
            wavesInRoom[i] = new Wave[enemiesInWave.Length];
            // iterate each wave in that specific room
            for (int j = 0; j < rooms[i].numberOfWaves; j++)
            {
                wavesInRoom[i][j] = new Wave(rooms[i].numberOfWaves, enemiesInWave);
            }
        }
    }

    public void DecrementCurrentEnemies()
    {
        currentEnemies--;
        Debug.Log("Enemies decreased; current enemies = " + currentEnemies);
    }

    // when called spawn the waves of enemies of that room in an order
    public IEnumerator SpawnEnemies(int roomNumber)
    {
        currentState = State.InBattle;
        // close the doors when the player enters the room
        doorManager.SwitchDoorState();
        
        for(int i = 0; i < rooms[roomNumber].numberOfWaves; i++)
        {
            currentEnemies = wavesInRoom[roomNumber][i].enemiesInWave.Length;
            // Spawn the enemies of the "i" wave
            for (int j = 0; j < wavesInRoom[roomNumber][i].enemiesInWave.Length; j++)
            {
                switch (wavesInRoom[roomNumber][i].enemiesInWave[j])
                {
                    case 1:
                        Instantiate(enemyMelee, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        break;
                    case 2:
                        Instantiate(enemyRanged, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
                        break;
                    case 3:
                        Instantiate(enemyFlying, (Vector3)rooms[roomNumber].spawnPoints[j], Quaternion.identity);
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
        doorManager.SwitchDoorState();
        currentState = State.Idle;

        yield return null;
    }

    private void Awake()
    {
        currentState = State.Idle;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeRooms();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
