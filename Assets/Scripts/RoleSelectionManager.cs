using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoleSelectionManager : MonoBehaviour
{
    public Button[] roleButtons;
    private int selectedRoleId = -1;

    // public GameManager gameManager; // ← Inspectorからの設定は不要なので削除

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
            // ▼▼▼ ここが最重要の修正点 ▼▼▼
            // Inspectorから設定するのではなく、シングルトンのInstanceを直接呼び出す
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGameWithRole(selectedRoleId);
            }
            else
            {
                Debug.LogError("GameManagerが見つかりません！Initializerシーンからゲームを開始していますか？");
            }

            // ▲▲▲ ここまで ▲▲▲

            // SceneManager.LoadScene("Main"); // ← この行は不要なので削除
        }
        else
        {
            Debug.Log("役職を選択してください。");
        }
    }
}