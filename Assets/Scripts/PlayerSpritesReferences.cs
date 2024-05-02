using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpritesReferences : ScriptableObject
{
    [Header("Player 1 Assets")]
    public Sprite Player1Sprite;
    public RuntimeAnimatorController Player1Animator;
    public Sprite Player1GunSprite;
    public Sprite Player2BulletSprite;
    public Material Player1BulletMaterial;
    [Header("Player 2 Assets")]
    public Sprite Player2Sprite;
    public RuntimeAnimatorController Player2Animator;
    public Sprite Player2GunSprite;
    public Sprite Player1BulletSprite;
    public Material Player2BulletMaterial;
}
