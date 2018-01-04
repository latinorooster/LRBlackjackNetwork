using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CardStack : NetworkBehaviour
{

    List<Card> cards;
    public bool isDeck;
    public bool isDealer;

    [SyncVar]
    public int cardScore;

    void Awake()
    {
        cards = new List<Card>();
    }

    public bool HasCards
    {
        get { return cards != null && cards.Count > 0; }
    }

    public IEnumerable<Card> GetCards()
    {
        foreach (Card c in cards)
        {
            yield return c;
        }
    }

    public int CardCount
    {
        get
        {
            if (cards == null)
            {
                return 0;
            }
            else
            {
                return cards.Count;
            }
        }
    }

    public void CreateDeck(int numberOfDecks)
    {
        Card card;
        cards.Clear();

        int numberOfCards = numberOfDecks * 52;

        for (int i = 0; i < numberOfCards; i++)
        {
            card = new Card(i);
            cards.Add(card);
        }

        Shuffle(5);
    }

    void Shuffle(int numberOfShuffles)
    {
        for (int i = 0; i < numberOfShuffles; i++)
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                Card temp = cards[k];
                cards[k] = cards[n];
                cards[n] = temp;
            }
        }
    }

    public Card Pop()
    {
        Card temp = cards[0];
        cards.RemoveAt(0);

        return temp;
    }

    public void ServerAddCard(Card card)
    {
        cards.Add(card);
        CalculateScore();
    }

    [ClientRpc]
    public void RpcAddCard(Card card)
    {
        if (!isServer)
        {
            cards.Add(card);
            CalculateScore();
        }
    }

    public void CalculateScore()
    {
        int total = 0;
        int aces = 0;

        foreach (Card card in GetCards())
        {
            if (card.Value <= 10)
            {
                total += card.Value;
            }
            else if (card.Value > 10 && card.Value <= 13)
            {
                total += 10;
            }
            else
            {
                aces++;
            }

        }

        if (total + aces + 10 <= 21 && aces > 0)
        {
            total += aces + 10;
        }
        else
        {
            total += aces;
        }

        cardScore = total;
    }

    public bool IsBusted()
    {
        return cardScore > 21;
    }

    public void Reset()
    {
        cards.Clear();
    }
}
