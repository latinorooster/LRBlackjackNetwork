using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    public Button buttonHit;
    public Button buttonStand;

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
        newPlayer.seatPosition = NumberOfPlayers();
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

    int NumberOfPlayers()
    {
        if (players == null)
        {
            return 0;
        }
        return players.Count;
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
            case GameState.GameTurnState.PlayingPlayerHand:
                {
                    ClientState_PlayingPlayerHand();
                    break;
                }
        }
    }

    public void EnablePlayHandButtons()
    {
        buttonHit.interactable = true;
        buttonStand.interactable = true;
    }

    public void DisbalePlayHandButtons()
    {
        buttonHit.interactable = false;
        buttonStand.interactable = false;
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

    [Client]
    public void ClientState_PlayingPlayerHand()
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
            player.ServerAddCard(deck.Pop());
        }

        Card nextCard = deck.Pop();
        Card temp = nextCard;
        dealer.ServerAddCard(nextCard);
        dealer.RpcAddCard(temp);

        // deal second card
        foreach (var player in players)
        {
            if (player.currentBet == 0)
                continue;

            player.ServerAddCard(deck.Pop());
        }

        // TODO: handle dealer blackjack

        ServerNextState("ServerState_PlayHands");
    }

    [Server]
    void ServerState_PlayHands()
    {
        ServerEnterGameState(GameState.GameTurnState.PlayingPlayerHand, "Playing hands");

        currentPlayerIndex = -1;
        Debug.Log(currentPlayerIndex);
        ServerNextPlayer();
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

    [Server]
    void ServerNextPlayer()
    {
        if (currentPlayer != null)
        {
            currentPlayer.RpcYourTurn(false);
        }

        currentPlayerIndex += 1;
        Debug.Log(currentPlayerIndex);
        while (currentPlayerIndex < players.Count)
        {
            currentPlayer = players[currentPlayerIndex];
            if (currentPlayer != null)
            {
                if (currentPlayer.currentBet != 0)
                {
                    currentPlayer.RpcYourTurn(true);
                    break;
                }
            }
            currentPlayerIndex += 1;
        }

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayer.RpcYourTurn(false);
            ServerNextState("ServerState_PlayDealerHand");
        }
    }

    #endregion

    #region Client UI Hooks

    public void UiHit()
    {
        localPlayer.CmdHit();
    }

    #endregion
}
