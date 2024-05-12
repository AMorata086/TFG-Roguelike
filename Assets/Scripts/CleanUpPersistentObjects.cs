using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CleanUpPersistentObjects : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (MultiplayerBehavior.Instance != null)
        {
            Destroy(MultiplayerBehavior.Instance.gameObject);
        }

        if (GameLobby.Instance != null)
        {
            Destroy(GameLobby.Instance.gameObject);
        }
    }
}
