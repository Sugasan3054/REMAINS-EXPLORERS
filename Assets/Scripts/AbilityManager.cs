using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Panels")]
    [SerializeField] private GameObject investigatorPanel;
    [SerializeField] private GameObject studentPanel;

    [Header("Main Ability Buttons")]
    [SerializeField] private Button player1AbilityButton;
    [SerializeField] private Button player2AbilityButton;

    [Header("Student Panel Buttons")]
    [SerializeField] private Button[] studentChoiceButtons;

    void Update()
    {
        // 参照先を currentGameData に変更
        if (GameManager.Instance == null || !GameManager.Instance.currentGameData.isPlaying)
        {
            if (player1AbilityButton != null) player1AbilityButton.interactable = false;
            if (player2AbilityButton != null) player2AbilityButton.interactable = false;
            return;
        }

        HandleAbilityButtonState(player1AbilityButton, "player1");
        HandleAbilityButtonState(player2AbilityButton, "player2");
    }

    private void HandleAbilityButtonState(Button abilityButton, string playerId)
    {
        if (abilityButton == null) return;

        // 参照先を currentGameData に変更
        PlayerData playerData = GameManager.Instance.currentGameData.players[playerId];
        bool canUse = false;

        // 参照先を currentGameData に変更
        if (GameManager.Instance.currentGameData.currentPlayerId == playerId)
        {
            canUse = !playerData.Role.isDisabled &&
                     playerData.Role.usedAbilities < playerData.Role.abilityUses;

            if (playerData.Role.id == 4) // Priest
            {
                canUse = false;
            }
            if (playerData.Role.id == 10) // Crown Prince
            {
                canUse = false;
            }

            int remainingSafeTiles = GameManager.Instance.Board.GetTotalSafeTileCount() - GameManager.Instance.Board.GetDiggedTileCount();

            if (canUse && playerData.Role.id == 2 && remainingSafeTiles <= 10) // Gambler
            {
                canUse = false;
            }
            if (canUse && playerData.Role.id == 5 && remainingSafeTiles < 12) // Dog Master
            {
                canUse = false;
            }
            if (canUse && playerData.Role.id == 6 && remainingSafeTiles < 10) // Duelist
            {
                canUse = false;
            }
            if (canUse && playerData.Role.id == 7 && remainingSafeTiles < 30)
            {
                canUse = false;
            }
            if (canUse && playerData.Role.id == 8 && remainingSafeTiles < 16)
            {
                canUse = false;
            }
            if (canUse && playerData.Role.id == 9)
            {
                if (playerData.Score < 15)
                {
                    canUse = false;
                }
            }
        }

        abilityButton.interactable = canUse;
    }

    public void OnAbilityButtonPressed()
    {
        // 参照先を currentGameData に変更
        int currentRoleId = GameManager.Instance.currentGameData.players[GameManager.Instance.currentGameData.currentPlayerId].Role.id;
        switch (currentRoleId)
        {
            case 1:
                OpenInvestigatorPanel();
                break;
            case 2:
                GameManager.Instance.UseAbility(2);
                break;
            case 3:
                GameManager.Instance.UseAbility(3);
                break;
            case 5:
                GameManager.Instance.UseAbility(5);
                break;
            case 6:
                GameManager.Instance.UseAbility(6);
                break;
            case 7:
                GameManager.Instance.UseAbility(7);
                break;
            case 8:
                GameManager.Instance.UseAbility(8);
                break;
            case 9:
                GameManager.Instance.UseAbility(9);
                break;
            default:
                Debug.Log("この役職には、ボタンで発動する能力はありません。");
                break;
        }
    }

    private void OpenInvestigatorPanel()
    {
        if (investigatorPanel != null)
        {
            investigatorPanel.SetActive(true);
        }
    }

    public void OnGuessRoleSelected(int guessedRoleId)
    {
        GameManager.Instance.UseAbility(1, -1, guessedRoleId);
        if (investigatorPanel != null)
        {
            investigatorPanel.SetActive(false);
        }
    }

    public void OpenStudentPanel(List<Role> roleChoices)
    {
        if (studentPanel == null || studentChoiceButtons.Length < 3) return;
        studentPanel.SetActive(true);

        for (int i = 0; i < studentChoiceButtons.Length; i++)
        {
            if (i < roleChoices.Count)
            {
                studentChoiceButtons[i].gameObject.SetActive(true);
                studentChoiceButtons[i].GetComponentInChildren<TMP_Text>().text = roleChoices[i].name;
                studentChoiceButtons[i].onClick.RemoveAllListeners();
                int roleId = roleChoices[i].id;
                studentChoiceButtons[i].onClick.AddListener(() => OnStudentRoleSelected(roleId));
            }
            else
            {
                studentChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnStudentRoleSelected(int newRoleId)
    {
        GameManager.Instance.ChangePlayerRole(newRoleId);
        if (studentPanel != null)
        {
            studentPanel.SetActive(false);
        }
    }
}