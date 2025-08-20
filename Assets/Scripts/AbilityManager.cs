using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Panels")]
    [SerializeField] private GameObject investigatorPanel;
    [SerializeField] private GameObject studentPanel; // Student�p�l���ւ̎Q�Ƃ�ǉ�

    [Header("Main Ability Buttons")]
    [SerializeField] private Button player1AbilityButton;
    [SerializeField] private Button player2AbilityButton;

    [Header("Student Panel Buttons")]
    [SerializeField] private Button[] studentChoiceButtons; // 3�̑I�����{�^���̔z��

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
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

        PlayerData playerData = GameManager.Instance.players[playerId];
        bool canUse = false;

        if (GameManager.Instance.currentPlayerId == playerId)
        {
            canUse = !playerData.Role.isDisabled &&
                     playerData.Role.usedAbilities < playerData.Role.abilityUses;

            if (playerData.Role.id == 4) // Priest
            {
                canUse = false;
            }

            if (playerData.Role.id == 10)
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

            // ������ ��w��(ID:7)�̒ǉ����� ������
            if (canUse && playerData.Role.id == 7 && remainingSafeTiles < 30)
            {
                canUse = false; // 30�^�C�������c���Ă���ꍇ�͎g�p�s��
            }

            if (canUse && playerData.Role.id == 8 && remainingSafeTiles < 16)
            {
                canUse = false; // 16�^�C�������c���Ă���ꍇ�͎g�p�s��
            }

            if (canUse && playerData.Role.id == 9)
            {
                // �v���C���[�̃X�R�A��15�����Ȃ�g�p�s�ɂ���
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
        int currentRoleId = GameManager.Instance.players[GameManager.Instance.currentPlayerId].Role.id;
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
            case 7: // ��w��
                GameManager.Instance.UseAbility(7);
                break;
            case 8:
                GameManager.Instance.UseAbility(8);
                break;
            case 9:
                GameManager.Instance.UseAbility(9);
                break;
            default:
                Debug.Log("���̖�E�ɂ́A�{�^���Ŕ�������\�͂͂���܂���B");
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

    // --- ��w���̔\�̓p�l�����J�� ---
    public void OpenStudentPanel(List<Role> roleChoices)
    {
        if (studentPanel == null || studentChoiceButtons.Length < 3) return;

        studentPanel.SetActive(true);

        for (int i = 0; i < studentChoiceButtons.Length; i++)
        {
            if (i < roleChoices.Count)
            {
                studentChoiceButtons[i].gameObject.SetActive(true);
                // �{�^���̃e�L�X�g���E���ɕύX
                studentChoiceButtons[i].GetComponentInChildren<TMP_Text>().text = roleChoices[i].name;

                // �{�^���̃N���b�N�C�x���g����x�N���A���Ă���A�V�����C�x���g��ݒ�
                studentChoiceButtons[i].onClick.RemoveAllListeners();
                int roleId = roleChoices[i].id; // ���[�v���Ŏg�����߁A�ꎞ�ϐ���ID���R�s�[
                studentChoiceButtons[i].onClick.AddListener(() => OnStudentRoleSelected(roleId));
            }
            else
            {
                studentChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ��w���p�l���̖�E�{�^������Ăяo�����
    public void OnStudentRoleSelected(int newRoleId)
    {
        GameManager.Instance.ChangePlayerRole(newRoleId);
        if (studentPanel != null)
        {
            studentPanel.SetActive(false);
        }
    }
}