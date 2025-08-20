using UnityEngine;
using UnityEngine.EventSystems;

public class ClickTester : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // クリックされたら、Consoleにメッセージを表示する
        Debug.Log("クリック成功！");
    }
}