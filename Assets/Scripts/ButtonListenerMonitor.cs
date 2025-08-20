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
            Debug.Log(targetButton.name + "の初期リスナー数: " + listenerCount);
        }
    }

    void Update()
    {
        if (targetButton != null)
        {
            if (targetButton.onClick.GetPersistentEventCount() < listenerCount)
            {
                Debug.LogError(targetButton.name + "のOnClickリスナーが何らかのスクリプトによって削除されました！", this.gameObject);
                Debug.Break();
                this.enabled = false;
            }
        }
    }
}