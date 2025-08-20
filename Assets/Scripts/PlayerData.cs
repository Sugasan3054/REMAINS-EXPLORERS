using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string PlayerId { get; private set; }
    public Role Role { get; set; }
    public bool IsAlive { get; set; } = true;
    public int Score { get; set; } = 0; // TotalMovesÇ©ÇÁScoreÇ…ïœçX
    public int TurnMoves { get; set; } = 0;
    public int TurnScore { get; set; } = 0;
    public Dictionary<string, object> SpecialStates { get; set; } = new Dictionary<string, object>();

    public PlayerData(string playerId, Role role)
    {
        PlayerId = playerId;
        Role = role;
    }
}