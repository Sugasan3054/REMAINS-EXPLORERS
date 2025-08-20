using UnityEngine;
using UnityEngine.UI;

// このスクリプトは、アタッチされたオブジェクトのImageとButtonコンポーネントを強制的に有効化し続ける
public class ForceEnableUI : MonoBehaviour
{
    private Image imageComponent;
    private Button buttonComponent;

    void Awake()
    {
        // 最初にコンポーネントを取得しておく
        imageComponent = GetComponent<Image>();
        buttonComponent = GetComponent<Button>();
    }

    // 毎フレーム呼ばれるUpdateで、有効状態を監視・強制する
    void Update()
    {
        // もしImageが無効になっていたら、有効に戻す
        if (imageComponent != null && !imageComponent.enabled)
        {
            imageComponent.enabled = true;
        }

        // もしButtonが無効になっていたら、有効に戻す
        if (buttonComponent != null && !buttonComponent.enabled)
        {
            buttonComponent.enabled = true;
        }
    }
}