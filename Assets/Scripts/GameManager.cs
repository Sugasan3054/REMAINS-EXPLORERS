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
    public GameData currentGameData;

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

    private string otherPlayerId => currentGameData.currentPlayerId == "player1" ? "player2" : "player1";
    private bool isInPredictionMode = false;
    private bool isInProphecyMode = false;
    private bool isInSwapMode = false;
    private List<int> tilesToSwap = new List<int>();
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
            SceneManager.sceneLoaded += OnSceneLoaded;

            InitializeRoles(); // Roleデータベースは最初に一度だけ読み込む

            // Initializerシーンの役目はここまで。すぐにTitleシーンへ移動する。
            SceneManager.LoadScene("Title");
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mainシーンがロードされた時だけ、以下の準備処理を行う
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

            if (mGameOverPanel != null)
            {
                mGameOverPanel.SetActive(false);
                Button restartButton = mGameOverPanel.transform.Find("RestartButton")?.GetComponent<Button>();
                if (restartButton != null)
                {
                    restartButton.onClick.RemoveAllListeners();
                    restartButton.onClick.AddListener(ReturnToTitle);
                }
            }

            StartGame();
        }
    }

    // Selectシーンから呼ばれる、唯一のゲーム開始命令
    public void StartGameWithRole(int selectedRoleIndex)
    {
        // 新しいゲームデータを作成して、ゲームの状態を完全にリセット
        currentGameData = new GameData();

        // 選択された役職をプレイヤー1に設定
        currentGameData.players["player1"].Role = new Role(roleDatabase[selectedRoleIndex]);

        // プレイヤー2にランダムな役職を設定
        List<int> availableRoles = new List<int>(roleDatabase.Keys);
        int enemyRoleId = availableRoles[UnityEngine.Random.Range(0, availableRoles.Count)];
        currentGameData.players["player2"].Role = new Role(roleDatabase[enemyRoleId]);

        // Mainシーンをロード
        SceneManager.LoadScene("Main");
    }

    // OnSceneLoadedから呼ばれ、Mainシーンでのゲームプレイを開始する
    public void StartGame()
    {
        // ゲームの状態を初期化
        currentGameData.isPlaying = true;
        currentGameData.turnCount = 0;
        currentGameData.currentPlayerId = "player1";
        duelistId = null;

        foreach (var player in currentGameData.players.Values)
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

    public void ReturnToTitle()
    {
        // Titleシーンに戻るだけ。リセットは次のStartGameWithRoleが担当する
        SceneManager.LoadScene("Title");
    }

    // --- 以下、OnTileClickedや他のゲームロジックメソッド ---
    // (currentGameDataを参照するように修正済み)

    public void OnTileClicked(int index, bool isDirectClick = true)
    {
        // isPlayingのチェックは、直接クリックの場合のみゲーム開始後に行う
        if (isDirectClick && !currentGameData.isPlaying) return;

        string currentPlayerId = currentGameData.currentPlayerId;

        if (isInProphecyMode)
        {
            Tile prophecyTile = mBoardManager.GetTile(index);
            if (prophecyTile == null) return;
            string feedback = (prophecyTile.TileType == TileType.BOOM) ? "Bomb" : "Safe";
            isInProphecyMode = false;
            currentGameData.players[currentPlayerId].Role.usedAbilities++;
            currentGameData.isPlaying = true;
            ShowFeedbackText(feedback + "!", () => { UpdateUI(); });
            return;
        }

        if (isInPredictionMode)
        {
            currentGameData.players[currentPlayerId].SpecialStates["gambler_prediction"] = index;
            isInPredictionMode = false;
            currentGameData.players[currentPlayerId].Role.usedAbilities++;
            ShowFeedbackText($"Predicted tile {index}. Now open a tile.");
            return;
        }

        if (isInSwapMode)
        {
            HandleSwapSelection(index);
            return;
        }

        if (isDirectClick)
        {
            CheckGamblerPrediction(index);
            if (!currentGameData.isPlaying) return;
        }

        Tile tile = mBoardManager.GetTile(index);
        if (tile == null || tile.IsDigged) return;

        if (tile.TileType == TileType.BOOM &&
            currentGameData.players.ContainsKey(currentPlayerId) &&
            currentGameData.players[currentPlayerId].Role.id == 4 &&
            currentGameData.players[currentPlayerId].IsAlive == true)
        {
            currentGameData.players[currentPlayerId].IsAlive = false;
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
                    currentGameData.players[currentPlayerId].Score += tile.AdjacentBombCount;
                    currentGameData.players[currentPlayerId].TurnScore += tile.AdjacentBombCount;
                }
                currentGameData.players[currentPlayerId].TurnMoves++;
            }
            CheckForGameClear();
            if (currentGameData.isPlaying && isDirectClick)
            {
                EndTurn();
            }
        }
    }

    public void OnMineHit()
    {
        GameOver(otherPlayerId);
    }

    public void EndTurn()
    {
        string currentPlayerId = currentGameData.currentPlayerId;
        if (duelistId != null)
        {
            if (currentPlayerId == duelistId)
            {
                duelistTurnScore = currentGameData.players[currentPlayerId].TurnScore;
                ProceedToNextTurn();
            }
            else
            {
                PlayerData duelist = currentGameData.players[duelistId];
                PlayerData opponent = currentGameData.players[currentPlayerId];
                int opponentTurnScore = opponent.TurnScore;

                duelistId = null;
                currentGameData.isPlaying = false;

                ShowFeedbackText($"Duel! {duelist.PlayerId.ToUpper()}({duelistTurnScore}) vs {opponent.PlayerId.ToUpper()}({opponentTurnScore})", () =>
                {
                    if (duelistTurnScore > opponentTurnScore) { GameOver(duelist.PlayerId.ToUpper()); }
                    else if (opponentTurnScore > duelistTurnScore) { GameOver(opponent.PlayerId.ToUpper()); }
                    else
                    {
                        ShowFeedbackText("Duel is a Draw!", () => {
                            currentGameData.isPlaying = true;
                            ProceedToNextTurn();
                        });
                    }
                });
            }
        }
        else
        {
            ProceedToNextTurn();
        }
    }

    private void ProceedToNextTurn()
    {
        currentGameData.turnCount++;
        currentGameData.currentPlayerId = (currentGameData.turnCount % 2 == 0) ? "player1" : "player2";
        foreach (var player in currentGameData.players.Values)
        {
            player.TurnMoves = 0;
            player.TurnScore = 0;
        }
        UpdateUI();
    }

    public void GameOver(string winnerId)
    {
        currentGameData.isPlaying = false;
        if (mGameOverPanel != null) mGameOverPanel.SetActive(true);
        if (mGameOverMessage != null) mGameOverMessage.text = $"{winnerId} WIN!";
    }

    private void UpdateUI()
    {
        if (currentGameData == null || !currentGameData.players.ContainsKey("player1")) return;

        if (mTurnText != null) mTurnText.text = $"TURN: {currentGameData.currentPlayerId.ToUpper()}";
        if (mPlayer1ScoreText != null) mPlayer1ScoreText.text = $"PLAYER1\nScore: {currentGameData.players["player1"].Score}";
        if (mPlayer2ScoreText != null) mPlayer2ScoreText.text = $"PLAYER2\nScore: {currentGameData.players["player2"].Score}";
    }

    private void DetermineWinnerByScore()
    {
        currentGameData.isPlaying = false;
        int score1 = currentGameData.players["player1"].Score;
        int score2 = currentGameData.players["player2"].Score;

        if (score1 > score2) { GameOver("PLAYER1"); }
        else if (score2 > score1) { GameOver("PLAYER2"); }
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
            PlayerData player1 = currentGameData.players["player1"];
            PlayerData player2 = currentGameData.players["player2"];
            bool player1IsPrince = player1.Role.id == 10;
            bool player2IsPrince = player2.Role.id == 10;

            if (player1IsPrince != player2IsPrince)
            {
                string winnerId = player1IsPrince ? player1.PlayerId : player2.PlayerId;
                GameOver(winnerId.ToUpper());
            }
            else
            {
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
        string currentPlayerId = currentGameData.currentPlayerId;
        PlayerData player = currentGameData.players[currentPlayerId];

        if (player.Role.isDisabled || player.Role.usedAbilities >= player.Role.abilityUses)
        {
            ShowFeedbackText("Cannot use ability");
            return;
        }

        int remainingSafeTiles = mBoardManager.GetTotalSafeTileCount() - mBoardManager.GetDiggedTileCount();
        if (abilityId == 2 && remainingSafeTiles <= 10) { ShowFeedbackText("Cannot predict with 10 or fewer tiles remaining"); return; }
        if (abilityId == 5 && remainingSafeTiles < 12) { ShowFeedbackText("Cannot use with fewer than 12 tiles remaining"); return; }
        if (abilityId == 6 && remainingSafeTiles < 10) { ShowFeedbackText("Cannot duel with fewer than 10 tiles remaining"); return; }
        if (abilityId == 7 && remainingSafeTiles < 30) { ShowFeedbackText("Cannot change job with with fewer than 30 tiles remaining"); return; }
        if (abilityId == 8 && remainingSafeTiles < 16) { ShowFeedbackText("Cannot magic with fewer than 16 tiles remaining"); return; }
        if (abilityId == 9 && player.Score < 15) { ShowFeedbackText("Score must be 15 ore higher"); return; }

        player.Role.usedAbilities++;

        switch (abilityId)
        {
            case 1: InvestigatorAbility(currentGameData.players[otherPlayerId], data1); break;
            case 2: StartGamblerPrediction(); break;
            case 3: StartProphecy(); break;
            case 5: DogMasterAbility(); break;
            case 6: StartDuel(); break;
            case 7: StudentAbility(); break;
            case 8: StartSwapMode(); break;
            case 9: ExecutionerAbility(currentGameData.players[otherPlayerId]); break;
        }
        UpdateUI();
    }

    private void InvestigatorAbility(PlayerData targetPlayer, int guessedRoleId)
    {
        string currentPlayerId = currentGameData.currentPlayerId;
        currentGameData.isPlaying = false;
        if (targetPlayer.Role.id == guessedRoleId && targetPlayer.Role.id != 4)
        {
            ShowFeedbackText("Correct!", () => { GameOver(currentPlayerId.ToUpper()); });
        }
        else
        {
            ShowFeedbackText("Miss", () => {
                currentGameData.players[currentPlayerId].Role.usedAbilities++;
                currentGameData.isPlaying = true;
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
        currentGameData.isPlaying = false;
        isInProphecyMode = true;
        ShowFeedbackText("Select a tile to check...");
    }

    private void DogMasterAbility()
    {
        string currentPlayerId = currentGameData.currentPlayerId;
        if (mBoardManager == null) return;
        List<int> safeTileIndices = mBoardManager.GetRandomSafeTiles(3);
        if (safeTileIndices.Count == 0)
        {
            ShowFeedbackText("No safe tiles found.");
            currentGameData.players[currentPlayerId].Role.usedAbilities++;
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
                OnTileClicked(index, false);
                feedback += index + " ";
                if (tile.TileType == TileType.COUNT)
                {
                    scoreFromAbility += tile.AdjacentBombCount;
                }
            }
        }

        currentGameData.players[currentPlayerId].Score += scoreFromAbility;
        currentGameData.players[currentPlayerId].Role.usedAbilities++;

        ShowFeedbackText(feedback, () => {
            CheckForGameClear();
            if (currentGameData.isPlaying)
            {
                EndTurn();
            }
        });
    }

    private void StartDuel()
    {
        duelistId = currentGameData.currentPlayerId;
        currentGameData.players[duelistId].Role.usedAbilities++;
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
            if (role.id != 7 && role.id != currentGameData.players[otherPlayerId].Role.id)
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
        string currentPlayerId = currentGameData.currentPlayerId;
        Role newRole = new Role(roleDatabase[newRoleId - 1]);
        currentGameData.players[currentPlayerId].Role = newRole;

        ShowFeedbackText($"You are now a {newRole.name}!");
        UpdateUI();
    }

    private void StartSwapMode()
    {
        currentGameData.isPlaying = false;
        isInSwapMode = true;
        tilesToSwap.Clear();
        ShowFeedbackText("Select the first tile to swap.");
    }

    private void HandleSwapSelection(int index)
    {
        string currentPlayerId = currentGameData.currentPlayerId;
        Tile tile = mBoardManager.GetTile(index);

        if (tile == null || tile.IsDigged || tilesToSwap.Contains(index))
        {
            ShowFeedbackText("You cannot select this tile.");
            return;
        }

        tilesToSwap.Add(index);

        if (tilesToSwap.Count == 1) { ShowFeedbackText("Select the second tile to swap."); }
        else if (tilesToSwap.Count == 2)
        {
            mBoardManager.SwapTiles(tilesToSwap[0], tilesToSwap[1]);
            isInSwapMode = false;
            currentGameData.players[currentPlayerId].Role.usedAbilities++;
            currentGameData.isPlaying = true;
            ShowFeedbackText($"Tiles {tilesToSwap[0]} and {tilesToSwap[1]} have been swapped.", () =>
            {
                UpdateUI();
            });
            tilesToSwap.Clear();
        }
    }

    private void ExecutionerAbility(PlayerData targetPlayer)
    {
        string currentPlayerId = currentGameData.currentPlayerId;
        if (targetPlayer.Role.id == 10)
        {
            currentGameData.isPlaying = false;
            ShowFeedbackText("Kill the Prince", () => {
                GameOver(currentPlayerId.ToUpper());
            });
        }
        else
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

    private void CheckGamblerPrediction(int clickedTileIndex)
    {
        PlayerData opponent = currentGameData.players[otherPlayerId];
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