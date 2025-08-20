using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RoleSelectionManager : MonoBehaviour
{
    public GameManager gameManager;
    public Button[] roleButtons;
    private int selectedRoleId = -1;

    public void SelectRole(int roleId)
    {
        Debug.Log("Selected Role ID: " + roleId);
        //��̓{�^���̃N���b�N������ɂ���Ă��邩�m�F�p
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
            gameManager.StartGameWithRole(selectedRoleId);
            SceneManager.LoadScene("Main");
        }
        else
        {
            Debug.Log("��E��I�����Ă��������B");
        }
    }
}