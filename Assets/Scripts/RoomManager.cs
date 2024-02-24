using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private int roomNumber;
    [SerializeField] private GameManager gameManager;
    public int numberOfWaves;
    public Vector2[] spawnPoints;
    public int enemiesAllowed;      // 1 if only Melee, 2 if Melee and Ranged, 3 if Melee, Ranged and Flying
    private Coroutine spawnEnemiesCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spawnEnemiesCoroutine = StartCoroutine(gameManager.SpawnEnemies(roomNumber));

            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
