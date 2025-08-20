using UnityEngine;
using UnityEngine.UI;

public class ButtonListenerMonitor : MonoBehaviour
{
    public Button targetButton;
    private int listenerCount = 0;

    void Start()
    {
        if (targetButton != null)
        {
            listenerCount = targetButton.onClick.GetPersistentEventCount();
            Debug.Log(targetButton.name + "�̏������X�i�[��: " + listenerCount);
        }
    }

    void Update()
    {
        if (targetButton != null)
        {
            if (targetButton.onClick.GetPersistentEventCount() < listenerCount)
            {
                Debug.LogError(targetButton.name + "��OnClick���X�i�[�����炩�̃X�N���v�g�ɂ���č폜����܂����I", this.gameObject);
                Debug.Break();
                this.enabled = false;
            }
        }
    }
}