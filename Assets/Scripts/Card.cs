using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public int CardIndex;
    public int Value;

    public enum SuitType
    {
        Hearts, Diamonds, Clubs, Spades
    }

    public SuitType Suit;

    public Card(int cardIndex)
    {
        CardIndex = cardIndex;
        Suit = (SuitType)((cardIndex % 52) / 13);
        Value = ((cardIndex % 52) % 13) + 2;

    }
}