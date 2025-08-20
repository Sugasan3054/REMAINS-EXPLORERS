using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicManager : MonoBehaviour
{
    // このシーンがロードされた時に自動で実行
    void Start()
    {
        // AudioManagerのインスタンスが存在するか確認
        if (AudioManager.Instance != null)
        {
            // 現在のシーン名を取得して、AudioManagerに渡す
            string currentSceneName = SceneManager.GetActiveScene().name;
            AudioManager.Instance.PlayMusicForScene(currentSceneName);
        }
        else
        {
            Debug.LogWarning("AudioManagerが見つかりません。ゲームオブジェクトにアタッチしてください。");
        }
    }
}