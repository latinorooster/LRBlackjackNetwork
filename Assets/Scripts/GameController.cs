using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

    public static GameController singleton;

    public CardStack deck;
    public CardStack dealer;

    public List<PlayerController> players;

    public PlayerController currentPlayer;
    public int currentPlayerIndex;
    public PlayerController localPlayer;


    [SyncVar]
    public GameState.GameTurnState turnState;

    private void Awake()
    {
        singleton = this;
    }

    public override void OnStartClient()
    {
        ClientHandleState(turnState, turnState.ToString());
    }

    public void AddPlayer(PlayerController newPlayer)
    {
        players.Add(newPlayer);

        if (players.Count == 1)
        {
            ServerNextState("ServerState_StartDeck");
        }
        else
        {
            //TODO: Display the cards of the current game
        }
    }

    [Server]
    void ServerEnterGameState(GameState.GameTurnState newState, string message)
    {
        Debug.Log("Server Enter state:" + newState);
        turnState = newState;
        RpcGameState(newState, message);
    }

    [Server]
    void ServerNextState(string funcName)
    {
        Debug.Log("Server next state:" + funcName);
        Invoke(funcName, 1.0f);
    }

    [ClientRpc]
    private void RpcGameState(GameState.GameTurnState newState, string message)
    {
        ClientHandleState(newState, message);
    }

    [Client]
    void ClientHandleState(GameState.GameTurnState newState, string message)
    {
        turnState = newState;
        string msg = "Client TurnState : " + newState + " - " + message;
        Debug.Log(msg);

        switch (newState)
        {
            case GameState.GameTurnState.ShufflingDeck:
                {
                    ClientState_NewDeck();
                    break;
                }
            case GameState.GameTurnState.WaitingForBets:
                {
                    ClientState_WaitingForBets();
                    break;
                }
            case GameState.GameTurnState.DealingCards:
                {
                    ClientState_DealingCards();
                    break;
                }
        }
    }

    #region Client State Functions

    [Client]
    public void ClientState_NewDeck()
    {
        if (deck.CardCount < 40)
        {
            deck.Reset();
            deck.CreateDeck(6);
        }
    }

    [Client]
    public void ClientState_WaitingForBets()
    {

    }

    [Client]
    public void ClientState_DealingCards()
    {

    }

    #endregion

    #region Server State Functions

    [Server]
    public void ServerState_StartDeck()
    {
        ServerEnterGameState(GameState.GameTurnState.ShufflingDeck, "Shuffling");
        deck.Reset();
        deck.CreateDeck(6);
        //TODO: ServerClearHands();

        ServerNextState("ServerState_WaitingForBets");
    }

    [Server]
    void ServerState_WaitingForBets()
    {
        ServerEnterGameState(GameState.GameTurnState.WaitingForBets, "Waiting for bets");
        ServerClearHands();
    }

    [Server]
    void ServerState_DealingCards()
    {
        ServerEnterGameState(GameState.GameTurnState.DealingCards, "Dealing cards");

        // deal first card
        foreach (var player in players)
        {
            if (player.currentBet == 0)
                continue;
            Card c = deck.Pop();
            player.Push(deck.Pop());
        }
        dealer.Push(deck.Pop());

        // deal second card
        foreach (var player in players)
        {
            if (player.currentBet == 0)
                continue;

            player.Push(deck.Pop());
        }

        // TODO: handle dealer blackjack

        ServerNextState("ServerState_PlayHands");
    }

    [Server]
    void ServerClearHands()
    {
        foreach (PlayerController p in players)
        {
            //p.GetComponent<PlayerController>().Clear();
        }

        //dealer.GetComponent<CardStackView>().Clear();
        //currentTurnPlayer = null;
    }

    #endregion

    #region Server Player Actions

    [Server]
    public void ServerCheckAllBets()
    {
        if (turnState != GameState.GameTurnState.WaitingForBets)
        {
            return;
        }

        int playerCount = 0;
        foreach (var player in players)
        {
            if (!player.bettingOnCurrentHand)
                return;

            if (player.currentBet > 0)
            {
                playerCount += 1;
            }
        }

        if (playerCount == 0)
        {
            // no players bet on this hand. reset it
            ServerNextState("ServerState_WaitingForBets");
        }
        else
        {
            // all players are ready/betting (some may be betting zero)
            ServerNextState("ServerState_DealingCards");
        }
    }

    #endregion
}
