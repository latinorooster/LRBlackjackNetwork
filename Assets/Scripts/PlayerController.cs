using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public int money = 1000;
    public int currentBet = 0;
    public int seatPosition;

    public bool bettingOnCurrentHand = false;

    public CardStack cards;

    public int cardScore;

    public override void OnStartServer()
    {
        GameController.singleton.AddPlayer(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        GameController.singleton.localPlayer = this;
    }

    [Server]
    public void Push(Card card)
    {
        cards.Push(card);
        RpcAddCard(card);
    }

    void RpcAddCard(Card card)
    {
        if (!isServer)
        {
            cards.Push(card);
        }
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
        currentBet += 10;
        money -= 10;
        bettingOnCurrentHand = true;

        GameController.singleton.ServerCheckAllBets();
    }

    #endregion

}
