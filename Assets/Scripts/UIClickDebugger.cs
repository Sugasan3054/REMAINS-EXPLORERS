using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// マウスカーソル下のUI要素を検出してログに出力するデバッグ用スクリプト
public class UIClickDebugger : MonoBehaviour
{
    // Inspectorから設定する
    public GraphicRaycaster m_Raycaster;
    public EventSystem m_EventSystem;

    private PointerEventData m_PointerEventData;

    void Update()
    {
        // マウスが動いていればチェック
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // マウスカーソル下のUIを検出する
            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointerEventData, results);

            // 検出されたUI要素の名前をすべてログに出力
            if (results.Count > 0)
            {
                Debug.Log("---------- マウスカーソルの下にあるUI ----------");
                foreach (RaycastResult result in results)
                {
                    Debug.Log("ヒット: " + result.gameObject.name);
                }
            }
        }
    }
}