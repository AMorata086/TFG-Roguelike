using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player_Hitbox")
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            GameManager.Instance.SetGameFinishedState(false);
        }
    }
}
