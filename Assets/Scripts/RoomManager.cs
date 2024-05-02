using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    [SerializeField] private int roomNumber;
    [SerializeField] private GameManager gameManager;
    public int numberOfWaves;
    public Vector2[] spawnPoints;
    public int enemiesAllowed;      // 1 if only Melee, 2 if Melee and Ranged, 3 if Melee, Ranged and Flying
    private Coroutine spawnEnemiesCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player_Hitbox"))
        {
            StartSpawnEnemiesCoroutineServerRpc();

            DisableRoomColliderServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartSpawnEnemiesCoroutineServerRpc()
    {
        spawnEnemiesCoroutine = StartCoroutine(gameManager.SpawnEnemies(roomNumber));
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableRoomColliderServerRpc()
    {
        DisableRoomColliderClientRpc();
    }

    [ClientRpc]
    private void DisableRoomColliderClientRpc()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
}
