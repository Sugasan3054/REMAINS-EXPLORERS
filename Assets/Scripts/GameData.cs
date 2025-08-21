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
        // ���̃��O�̓f�o�b�O�p�Ɏc���Ă����Ă��ǂ��ł��傤
        Debug.LogWarning("--- �V����GameData�I�u�W�F�N�g���쐬����܂��� ---");

        players.Clear();
        players.Add("player1", new PlayerData("player1", new Role(-1, "Not Selected", "")));
        players.Add("player2", new PlayerData("player2", new Role(-1, "Not Selected", "")));
    }
}