using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CardView : MonoBehaviour
{

    public GameObject Card { get; private set; }
    SpriteRenderer spriteRenderer;
    public Sprite[] faces;
    public int cardIndex;

    public CardView(GameObject card)
    {
        Card = card;
    }

    public void ShowCard()
    {
        spriteRenderer.sprite = faces[cardIndex % 52];
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

}