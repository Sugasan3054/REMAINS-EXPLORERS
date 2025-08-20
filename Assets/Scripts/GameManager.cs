using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Enums;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Board")]
    [SerializeField] private BoardManager mBoardManager;

    [Header("Ability UI")]
    [SerializeField] private AbilityManager mAbilityManager;

    [Header("UI References")]
    [SerializeField] private TMP_Text mTurnText;
    [SerializeField] private TMP_Text mPlayer1ScoreText;
    [SerializeField] private TMP_Text mPlayer2ScoreText;
    [SerializeField] private GameObject mGameOverPanel;
    [SerializeField] private TMP_Text mGameOverMessage;
    [SerializeField] private TMP_Text mFeedbackText;

    public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public string currentPlayerId { get; private set; }
    private string otherPlayerId => currentPlayerId == "player1" ? "player2" : "player1";
    private int turnCount = 0;
    public bool isPlaying { get; private set; } = false;

    private bool isInPredictionMode = false;
    private bool isInProphecyMode = false;
    private bool isInSwapMode = false; // 魔術師のタイル交換
    private List<int> tilesToSwap = new List<int>(); // 交換対象のタイルの記録リスト

    private string duelistId = null;
    private int duelistTurnScore = 0;

    private Dictionary<int, Role> roleDatabase;
    public BoardManager Board => mBoardManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRoles();
            InitializePlayers();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private Button mRestartButton;


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main")
        {
            mBoardManager = FindObjectOfType<BoardManager>();
            mAbilityManager = FindObjectOfType<AbilityManager>();
            mTurnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();
            mPlayer1ScoreText = GameObject.Find("Player1ScoreText")?.GetComponent<TMP_Text>();
            mPlayer2ScoreText = GameObject.Find("Player2ScoreText")?.GetComponent<TMP_Text>();
            mGameOverPanel = GameObject.Find("GameOverPanel");
            mGameOverMessage = mGameOverPanel?.transform.Find("GameOverMessageText")?.GetComponent<TMP_Text>();
            mFeedbackText = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();

            Transform restartButtonTransform = mGameOverPanel?.transform.Find("RestartButton");
            if (restartButtonTransform != null)
            {
                mRestartButton = restartButtonTransform.GetComponent<Button>();
                if (mRestartButton != null)
                {
                    // 以前のリスナーを念のため全てクリア
                    mRestartButton.onClick.RemoveAllListeners();
                    // このスクリプト（GameManager_本物）のReturnToTitleメソッドを登録
                    mRestartButton.onClick.AddListener(ReturnToTitle);
                }
            }

            StartGame();
        }
    }

    public void StartGameWithRole(int selectedRoleIndex)
    {
        players["player1"].Role = roleDatabase[selectedRoleIndex];
        List<int> availableRoles = new List<int>(roleDatabase.Keys);
        availableRoles.Remove(selectedRoleIndex);
        int enemyRoleId = availableRoles[UnityEngine.Random.Range(0, availableRoles.Count)];
        //Debug
        enemyRoleId = 0;
        players["player2"].Role = roleDatabase[enemyRoleId];
        SceneManager.LoadScene("Main");
    }

    public void StartGame()
    {
        isPlaying = true;
        turnCount = 0;
        currentPlayerId = "player1";
        duelistId = null;
        if (mGameOverPanel != null) mGameOverPanel.SetActive(false);
        if (mFeedbackText != null) mFeedbackText.gameObject.SetActive(false);

        foreach (var player in players.Values)
        {
            player.Score = 0;
            player.TurnMoves = 0;
            player.TurnScore = 0;
            player.IsAlive = true;
        }

        if (mBoardManager != null)
        {
            const int maxRetries = 100;
            bool clusterFound = false;
            for (int i = 0; i < maxRetries; i++)
            {
                mBoardManager.Initialize(this);
                List<Tile> cluster = mBoardManager.FindInitialCluster();
                if (cluster != null)
                {
                    foreach (Tile tile in cluster)
                    {
                        OnTileClicked(tile.GetIndex(), false);
                    }
                    clusterFound = true;
                    break;
                }
            }
            if (!clusterFound)
            {
                mBoardManager.Initialize(this);
            }
        }
        UpdateUI();
    }

    public void OnTileClicked(int index, bool isDirectClick = true)
    {
        if (isInProphecyMode)
        {
            Tile prophecyTile = mBoardManager.GetTile(index);
            if (prophecyTile == null) return;
            string feedback = (prophecyTile.TileType == TileType.BOOM) ? "Bomb" : "Safe";
            isInProphecyMode = false;
            players[currentPlayerId].Role.usedAbilities++;
            isPlaying = true;
            ShowFeedbackText(feedback + "!", () => { UpdateUI(); });
            return;
        }

        if (isInPredictionMode)
        {
            players[currentPlayerId].SpecialStates["gambler_prediction"] = index;
            isInPredictionMode = false;
            players[currentPlayerId].Role.usedAbilities++;
            ShowFeedbackText($"Predicted tile {index}. Now open a tile.");
            return;
        }

        if (isInSwapMode)
        {
            HandleSwapSelection(index);
            return;
        }


        if (!isPlaying) return;
        if (isDirectClick)
        {
            CheckGamblerPrediction(index);
            if (!isPlaying) return;
        }

        Tile tile = mBoardManager.GetTile(index);
        if (tile == null || tile.IsDigged) return;

        if (tile.TileType == TileType.BOOM &&
            players.ContainsKey(currentPlayerId) &&
            players[currentPlayerId].Role.id == 4 &&
            players[currentPlayerId].IsAlive == true)
        {
            players[currentPlayerId].IsAlive = false;
            ShowFeedbackText("God's protection saved you!");
            return;
        }

        tile.OpenTile();

        if (tile.TileType == TileType.BOOM)
        {
            OnMineHit();
        }
        else
        {
            if (tile.TileType == TileType.EMPTY)
            {
                mBoardManager.DigAdjacentTiles(index % 10, index / 10);
            }
            if (isDirectClick)
            {
                if (tile.TileType == TileType.COUNT)
                {
                    players[currentPlayerId].Score += tile.AdjacentBombCount;
                    players[currentPlayerId].TurnScore += tile.AdjacentBombCount;
                }
                players[currentPlayerId].TurnMoves++;
            }
            CheckForGameClear();
            if (isPlaying && isDirectClick)
            {
                EndTurn();
            }
        }
    }

    public void OnMineHit()
    {
        GameOver(otherPlayerId);
    }

    // ▼▼▼ 決闘ロジックを完全に書き換えた最終版 ▼▼▼
    public void EndTurn()
    {
        // 決闘が進行中かチェック
        if (duelistId != null)
        {
            // 今ターンを終えるのが「決闘者」本人か？
            if (currentPlayerId == duelistId)
            {
                // スコアを記録し、通常のターン交代へ
                duelistTurnScore = players[currentPlayerId].TurnScore;
                ProceedToNextTurn();
            }
            else // 今ターンを終えるのが「相手」の場合
            {
                // ここで決闘の結果判定を行う
                PlayerData duelist = players[duelistId];
                PlayerData opponent = players[currentPlayerId];
                int opponentTurnScore = opponent.TurnScore;

                // 決闘状態を先にリセット
                duelistId = null;

                isPlaying = false; // 判定中は操作不能に
                ShowFeedbackText($"Duel! {duelist.PlayerId.ToUpper()}({duelistTurnScore}) vs {opponent.PlayerId.ToUpper()}({opponentTurnScore})", () =>
                {
                    if (duelistTurnScore > opponentTurnScore)
                    {
                        GameOver(duelist.PlayerId.ToUpper());
                    }
                    else if (opponentTurnScore > duelistTurnScore)
                    {
                        GameOver(opponent.PlayerId.ToUpper());
                    }
                    else // 引き分け
                    {
                        ShowFeedbackText("Duel is a Draw!", () => {
                            isPlaying = true;
                            ProceedToNextTurn(); // ターン進行を再開
                        });
                    }
                });
            }
        }
        else
        {
            // 決闘中でなければ、通常のターン終了処理
            ProceedToNextTurn();
        }
    }

    private void ProceedToNextTurn()
    {
        turnCount++;
        currentPlayerId = (turnCount % 2 == 0) ? "player1" : "player2";
        foreach (var player in players.Values)
        {
            player.TurnMoves = 0;
            player.TurnScore = 0;
        }
        UpdateUI();
    }

    public void GameOver(string winnerId)
    {
        isPlaying = false;
        if (mGameOverPanel != null) mGameOverPanel.SetActive(true);
        if (mGameOverMessage != null) mGameOverMessage.text = $"{winnerId} WIN!";
    }

    public void ReturnToTitle()
    {
        // "Title"という名前のシーンをロードする
        Debug.Log("ReturnToTitleメソッドが呼ばれました！これからTitleシーンに移動します。");
        SceneManager.LoadScene("Title");
    }


    private void UpdateUI()
    {
        if (mTurnText != null) mTurnText.text = $"TURN: {currentPlayerId.ToUpper()}";
        if (mPlayer1ScoreText != null) mPlayer1ScoreText.text = $"PLAYER1\nScore: {players["player1"].Score}";
        if (mPlayer2ScoreText != null) mPlayer2ScoreText.text = $"PLAYER2\nScore: {players["player2"].Score}";
    }

    private void DetermineWinnerByScore()
    {
        isPlaying = false;
        int score1 = players["player1"].Score;
        int score2 = players["player2"].Score;

        if (score1 > score2)
        {
            GameOver("PLAYER1");
        }
        else if (score2 > score1)
        {
            GameOver("PLAYER2");
        }
        else
        {
            if (mGameOverPanel != null) mGameOverPanel.SetActive(true);
            if (mGameOverMessage != null) mGameOverMessage.text = "DRAW";
        }
    }

    private void CheckForGameClear()
    {
        if (mBoardManager == null) return;
        if (mBoardManager.GetDiggedTileCount() == mBoardManager.GetTotalSafeTileCount())
        {
            PlayerData player1 = players["player1"];
            PlayerData player2 = players["player2"];

            bool player1IsPrince = player1.Role.id == 10;
            bool player2IsPrince = player2.Role.id == 10;

            // 1. プレイヤーのうち片方だけが皇太子の場合
            if (player1IsPrince != player2IsPrince) // (p1が皇太子 && p2が違う) OR (p1が違う && p2が皇太子)
            {
                // 皇太子であるプレイヤーの勝利
                string winnerId = player1IsPrince ? player1.PlayerId : player2.PlayerId;
                GameOver(winnerId.ToUpper());
            }
            // 2. 両プレイヤーが皇太子の場合、または両方とも皇太子でない場合
            else
            {
                // 通常通りスコアで勝敗を決定する
                DetermineWinnerByScore();
            }
        }
    }

    #region Role Abilities

    public void ShowFeedbackText(string message, Action onComplete = null, float duration = 1.5f)
    {
        StartCoroutine(FeedbackCoroutine(message, duration, onComplete));
    }

    private IEnumerator FeedbackCoroutine(string message, float duration, Action onComplete)
    {
        if (mFeedbackText != null)
        {
            mFeedbackText.text = message;
            mFeedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            mFeedbackText.gameObject.SetActive(false);
        }
        onComplete?.Invoke();
    }

    public void UseAbility(int abilityId, int targetPlayerId = -1, int data1 = -1, int data2 = -1)
    {
        PlayerData player = players[currentPlayerId];
        if (player.Role.isDisabled || player.Role.usedAbilities >= player.Role.abilityUses)
        {
            ShowFeedbackText("Cannot use ability");
            return;
        }

        int remainingSafeTiles = mBoardManager.GetTotalSafeTileCount() - mBoardManager.GetDiggedTileCount();

        if (abilityId == 2 && remainingSafeTiles <= 10)
        {
            ShowFeedbackText("Cannot predict with 10 or fewer tiles remaining");
            return;
        }

        if (abilityId == 5 && remainingSafeTiles < 12)
        {
            ShowFeedbackText("Cannot use with fewer than 12 tiles remaining");
            return;
        }

        if (abilityId == 6 && remainingSafeTiles < 10)
        {
            ShowFeedbackText("Cannot duel with fewer than 10 tiles remaining");
            return;
        }

        if (abilityId == 7 && remainingSafeTiles < 30)
        {
            ShowFeedbackText("Cannot change job with with fewer than 30 tiles remaining");
            return;
        }

        if (abilityId == 8 && remainingSafeTiles <16)
        {
            ShowFeedbackText("Cannot magic with fewer than 16 tiles remaining");
            return;
        }

        if (abilityId == 9 && player.Score < 15)
        {
            ShowFeedbackText("Score must be 15 ore higher");
            return;
        }

        //能力使用を先にカウントする。
        player.Role.usedAbilities++;



        switch (abilityId)
        {
            case 1:
                InvestigatorAbility(players[otherPlayerId], data1);
                break;
            case 2:
                StartGamblerPrediction();
                break;
            case 3:
                StartProphecy();
                break;
            case 5:
                DogMasterAbility();
                break;
            case 6:
                StartDuel();
                break;
            case 7:
                StudentAbility();
                break;
            case 8:
                StartSwapMode();
                break;
            case 9:
                ExecutionerAbility(players[otherPlayerId]);
                break;
        }
        UpdateUI();
    }

    private void InvestigatorAbility(PlayerData targetPlayer, int guessedRoleId)
    {
        isPlaying = false;
        if (targetPlayer.Role.id == guessedRoleId && targetPlayer.Role.id != 4)
        {
            ShowFeedbackText("Correct!", () => {
                GameOver(currentPlayerId.ToUpper());
            });
        }
        else
        {
            ShowFeedbackText("Miss", () => {
                players[currentPlayerId].Role.usedAbilities++;
                isPlaying = true;
                UpdateUI();
            });
        }
    }

    private void StartGamblerPrediction()
    {
        isInPredictionMode = true;
        ShowFeedbackText("Predict which tile the opponent will open...");
    }

    private void StartProphecy()
    {
        isPlaying = false;
        isInProphecyMode = true;
        ShowFeedbackText("Select a tile to check...");
    }

    private void DogMasterAbility()
    {
        if (mBoardManager == null) return;
        List<int> safeTileIndices = mBoardManager.GetRandomSafeTiles(3);
        if (safeTileIndices.Count == 0)
        {
            ShowFeedbackText("No safe tiles found.");
            players[currentPlayerId].Role.usedAbilities++;
            EndTurn();
            return;
        }

        string feedback = "Safe tiles found at: ";
        int scoreFromAbility = 0;
        foreach (int index in safeTileIndices)
        {
            Tile tile = mBoardManager.GetTile(index);
            if (tile != null && !tile.IsDigged)
            {
                OnTileClicked(index, false); // isDirectClickをfalseに
                feedback += index + " ";
                if (tile.TileType == TileType.COUNT)
                {
                    scoreFromAbility += tile.AdjacentBombCount;
                }
            }
        }

        players[currentPlayerId].Score += scoreFromAbility; // スコアをここで加算
        players[currentPlayerId].Role.usedAbilities++;

        ShowFeedbackText(feedback, () => {
            CheckForGameClear();
            if (isPlaying)
            {
                EndTurn();
            }
        });
    }

    private void StartDuel()
    {
        duelistId = currentPlayerId;
        players[currentPlayerId].Role.usedAbilities++;
        ShowFeedbackText("Duel Started!");
    }

    private void StudentAbility()
    {
        if (mAbilityManager == null) return;

        List<Role> choices = GetStudentRoleChoices();
        if (choices.Count > 0)
        {
            mAbilityManager.OpenStudentPanel(choices);
        }
        else
        {
            ShowFeedbackText("No roles available to choose.");
        }
    }

    public List<Role> GetStudentRoleChoices()
    {
        List<int> availableRoleIds = new List<int>();
        foreach (var role in roleDatabase.Values)
        {
            if (role.id != 7 && role.id != players[otherPlayerId].Role.id)
            {
                availableRoleIds.Add(role.id);
            }
        }

        for (int i = 0; i < availableRoleIds.Count; i++)
        {
            int temp = availableRoleIds[i];
            int randomIndex = UnityEngine.Random.Range(i, availableRoleIds.Count);
            availableRoleIds[i] = availableRoleIds[randomIndex];
            availableRoleIds[randomIndex] = temp;
        }

        List<Role> choices = new List<Role>();
        int count = Mathf.Min(3, availableRoleIds.Count);
        for (int i = 0; i < count; i++)
        {
            choices.Add(roleDatabase[availableRoleIds[i] - 1]);
        }
        return choices;
    }

    public void ChangePlayerRole(int newRoleId)
    {
        Role newRole = roleDatabase[newRoleId - 1];
        players[currentPlayerId].Role = newRole;
        players[currentPlayerId].Role.usedAbilities = 0;

        ShowFeedbackText($"You are now a {newRole.name}!");
        UpdateUI();
    }

    private void StartSwapMode()
    {
        isPlaying = false;
        isInSwapMode = true;
        tilesToSwap.Clear();
        ShowFeedbackText("Select the first tile to swap.");
    }

    private void HandleSwapSelection(int index)
    {
        Tile tile = mBoardManager.GetTile(index);

        if(tile == null || tile.IsDigged || tilesToSwap.Contains(index))
        {
            ShowFeedbackText("You cannot select this tile.");
            return;
        }

        tilesToSwap.Add(index);

        if(tilesToSwap.Count == 1)
        {
            //1つ目のタイルを選択
            ShowFeedbackText("Select the second tile to swap.");
        }
        else if (tilesToSwap.Count == 2)
        {
            //2つ目のタイルを選択したので、交換を実行
            mBoardManager.SwapTiles(tilesToSwap[0], tilesToSwap[1]);

            isInSwapMode = false;
            players[currentPlayerId].Role.usedAbilities++;
            isPlaying = true; // 操作可能に戻す

            ShowFeedbackText($"Tiles {tilesToSwap[0]} and {tilesToSwap[1]} have been swapped.", () =>
            {
                UpdateUI();
            });
            tilesToSwap.Clear();
        }
    }

    private void ExecutionerAbility(PlayerData targetPlayer)
    {
        //特別な勝利条件
        if(targetPlayer.Role.id == 10)
        {
            isPlaying = false;
            ShowFeedbackText("Kill the Prince", () =>
            {
                GameOver(currentPlayerId.ToUpper());
            });
        }
        else // 通常効果
        {
            targetPlayer.Role.isDisabled = true;
            ShowFeedbackText($"{targetPlayer.PlayerId.ToUpper()}'s ability has been disabled");
        }
    }

    #endregion

    #region Other Logic

    private void InitializeRoles()
    {
        roleDatabase = new Dictionary<int, Role>
        {
            { 0, new Role(1, "Investigator", "Guess the opponent's role. If correct, you win.", 1) },
            { 1, new Role(2, "Gambler", "Predict the tile your opponent will open. If correct, you win.", 1) },
            { 2, new Role(3, "Prophet", "Check if a selected tile is a bomb.", 1) },
            { 3, new Role(4, "Priest", "Survive one bomb explosion.", 1) },
            { 4, new Role(5, "Dog Master", "Reveal 3 random safe tiles.", 1) },
            { 5, new Role(6, "Duelist", "The player who scores more in their turn wins the duel.", 1) },
            { 6, new Role(7, "Student", "Choose a new role from 3 random options during the game.", 1) },
            { 7, new Role(8, "Magician", "Swap the positions of two tiles.", 1) },
            { 8, new Role(9, "Executioner", "Disable the opponent's ability.", 1) },
            { 9, new Role(10, "Crown Prince", "Win the game if all non-bomb tiles are cleared.", 1) }
        };
    }

    private void InitializePlayers()
    {
        players.Clear();
        players.Add("player1", new PlayerData("player1", new Role(-1, "Not Selected", "")));
        players.Add("player2", new PlayerData("player2", new Role(-1, "Not Selected", "")));
    }

    private void CheckGamblerPrediction(int clickedTileIndex)
    {
        PlayerData opponent = players[otherPlayerId];
        if (opponent.SpecialStates.ContainsKey("gambler_prediction"))
        {
            int predictedIndex = (int)opponent.SpecialStates["gambler_prediction"];
            if (predictedIndex == clickedTileIndex)
            {
                ShowFeedbackText("PREDICTED!", () => {
                    GameOver(opponent.PlayerId.ToUpper());
                });
            }
            opponent.SpecialStates.Remove("gambler_prediction");
        }
    }
    #endregion
}