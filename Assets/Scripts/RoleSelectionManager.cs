using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoleSelectionManager : MonoBehaviour
{
    public Button[] roleButtons;
    private int selectedRoleId = -1;

    // public GameManager gameManager; // �� Inspector����̐ݒ�͕s�v�Ȃ̂ō폜

    public void SelectRole(int roleId)
    {
        Debug.Log("Selected Role ID: " + roleId);
        foreach (var button in roleButtons)
        {
            button.GetComponent<Image>().color = Color.white;
        }

        if (roleId >= 0 && roleId < roleButtons.Length)
        {
            roleButtons[roleId].GetComponent<Image>().color = Color.yellow;
            selectedRoleId = roleId;
        }
    }

    public void OnStartGameButtonClicked()
    {
        if (selectedRoleId != -1)
        {
            // ������ �������ŏd�v�̏C���_ ������
            // Inspector����ݒ肷��̂ł͂Ȃ��A�V���O���g����Instance�𒼐ڌĂяo��
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGameWithRole(selectedRoleId);
            }
            else
            {
                Debug.LogError("GameManager��������܂���IInitializer�V�[������Q�[�����J�n���Ă��܂����H");
            }

            // ������ �����܂� ������

            // SceneManager.LoadScene("Main"); // �� ���̍s�͕s�v�Ȃ̂ō폜
        }
        else
        {
            Debug.Log("��E��I�����Ă��������B");
        }
    }
}