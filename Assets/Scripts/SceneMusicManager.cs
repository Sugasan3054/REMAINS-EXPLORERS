using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicManager : MonoBehaviour
{
    // ���̃V�[�������[�h���ꂽ���Ɏ����Ŏ��s
    void Start()
    {
        // AudioManager�̃C���X�^���X�����݂��邩�m�F
        if (AudioManager.Instance != null)
        {
            // ���݂̃V�[�������擾���āAAudioManager�ɓn��
            string currentSceneName = SceneManager.GetActiveScene().name;
            AudioManager.Instance.PlayMusicForScene(currentSceneName);
        }
        else
        {
            Debug.LogWarning("AudioManager��������܂���B�Q�[���I�u�W�F�N�g�ɃA�^�b�`���Ă��������B");
        }
    }
}