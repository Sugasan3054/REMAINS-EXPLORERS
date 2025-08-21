using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public int turnCount = 0;
    public string currentPlayerId = "player1";
    public bool isPlaying = false;

    public GameData()
    {
        // このログはデバッグ用に残しておいても良いでしょう
        Debug.LogWarning("--- 新しいGameDataオブジェクトが作成されました ---");

        players.Clear();
        players.Add("player1", new PlayerData("player1", new Role(-1, "Not Selected", "")));
        players.Add("player2", new PlayerData("player2", new Role(-1, "Not Selected", "")));
    }
}