﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public int money = 1000;
    //public int currentBet = 0;

    [SyncVar (hook ="OnChangeSeatPosition")]
    public int seatPosition=-1;
    public Vector3[] seatPositions = new[] { new Vector3(3.726f, -.43f, 0.468f), new Vector3(3.05f, -0.4312742f, 0.2f), new Vector3(2.4f, -0.4412742f, 0.6f) };

    //public bool bettingOnCurrentHand = false;

    public CardStack cards;

    public override void OnStartServer()
    {
        GameController.singleton.AddPlayer(this);
    }

    public override void OnStartClient()
    {
        cards.GetComponent<CardStackView>().startPosition = seatPositions[seatPosition];
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        GameController.singleton.localPlayer = this;
    }

    [Server]
    public void ServerAddCard(Card card)
    {
        cards.ServerAddCard(card);
        RpcAddCard(card);

        if (cards.cardScore > 21)
        {
            GameController.singleton.ServerNextPlayer();
        }
    }

    [ClientRpc]
    void RpcAddCard(Card card)
    {
        if (!isServer)
        {
            cards.ServerAddCard(card);
        }
    }

    [Server]
    public void ServerClearCards()
    {
        cards.GetComponent<CardStackView>().ServerClearCards();
        cards.cardScore = 0;

        RpcClearCards();
    }

    [ClientRpc]
    private void RpcClearCards()
    {
        if (!isServer)
        {
            cards.GetComponent<CardStackView>().ServerClearCards();
        }
        cards.cardScore = 0;
    }

    [Server]
    public void ServerClearBet()
    {
        //currentBet = 0;
        RpcClearBet();
    }

    [ClientRpc]
    private void RpcClearBet()
    {
        if (!isServer)
        {
            //currentBet = 0;
        }
    }

    [ClientRpc]
    public void RpcYourTurn(bool isYourTurn)
    {
        if (isYourTurn && isLocalPlayer)
        {
            GameController.singleton.EnablePlayHandButtons();
        }
        else
        {
            GameController.singleton.DisbalePlayHandButtons();
        }
    }

    void OnChangeSeatPosition(int seat)
    {
        seatPosition = seat;
        cards.GetComponent<CardStackView>().startPosition = seatPositions[seat];
    }

    #region Commands

    [Command]
    public void CmdPlaceBet()
    {
        if (GameController.singleton.turnState != GameState.GameTurnState.WaitingForBets)
        {
            Debug.LogError("cannot place bet now");
            return;
        }

        Debug.Log("CmdPlaceBet");
        //currentBet += 10;
        money -= 10;
        //bettingOnCurrentHand = true;

        GameController.singleton.ServerCheckAllBets();
    }

    [Command]
    public void CmdHit()
    {
        if(GameController.singleton.turnState != GameState.GameTurnState.PlayingPlayerHand)
        {
            Debug.LogError("Cannot hit me now");
            return;
        }
        if (GameController.singleton.currentPlayer != this)
        {
            Debug.LogError("Not your turn");
            return;
        }
        if (cards.cardScore > 21)
        {
            Debug.LogError("Cannot hit over 21");
            return;
        }

        Debug.Log("CmdHit");

        ServerAddCard(GameController.singleton.deck.Pop());
    }

    [Command]
    public void CmdStand()
    {
        if (GameController.singleton.turnState != GameState.GameTurnState.PlayingPlayerHand)
        {
            Debug.LogError("Cannot stand now");
            return;
        }
        if (GameController.singleton.currentPlayer != this)
        {
            Debug.LogError("Not your turn");
            return;
        }

        Debug.Log("CmdStand");

        GameController.singleton.ServerNextPlayer();
    }

    #endregion

}
