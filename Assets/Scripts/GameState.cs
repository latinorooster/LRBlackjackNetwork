using System;
using UnityEngine.Networking;

public class GameState {

    public enum GameTurnState
    {
        ShufflingDeck,
        WaitingForBets,
        DealingCards,
        PlayingPlayerHand,
        PlayingDealerHand,
        Complete
    };
}
