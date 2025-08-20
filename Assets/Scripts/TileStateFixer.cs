using UnityEngine;
using UnityEngine.UI;

// タイルの初期状態を強制的に正しく設定するためのスクリプト
public class TileStateFixer : MonoBehaviour
{
    public void FixAllTileStates(Tile[,] tiles)
    {
        if (tiles == null) return;

        // すべてのタイルをループでチェック
        foreach (Tile tile in tiles)
        {
            // --- 親オブジェクト自体をまずアクティブにする ---
            tile.gameObject.SetActive(true);

            // --- 各パーツを探し出して状態を強制的に設定 ---

            // Image_Base (地面) を探して、表示状態にする
            Transform imageBase = tile.transform.Find("Image_Base");
            if (imageBase != null)
            {
                imageBase.gameObject.SetActive(true);
                Image img = imageBase.GetComponent<Image>();
                if (img != null) img.enabled = true;
            }

            // Image_Cover (フタ) を探して、表示状態にし、クリック可能にする
            Transform imageCover = tile.transform.Find("Image_Cover");
            if (imageCover != null)
            {
                imageCover.gameObject.SetActive(true);
                Image img = imageCover.GetComponent<Image>();
                if (img != null) img.enabled = true;
                Button btn = imageCover.GetComponent<Button>();
                if (btn != null) btn.enabled = true;
            }

            // Image_Boom (中身) を探して、非表示状態にする
            Transform imageBoom = tile.transform.Find("Image_Base/Image_Boom"); // 階層に合わせて修正
            if (imageBoom != null)
            {
                imageBoom.gameObject.SetActive(false);
            }

            // Text (TMP) (中身) を探して、非表示状態にする
            Transform textMesh = tile.transform.Find("Image_Base/Text (TMP)"); // 階層に合わせて修正
            if (textMesh != null)
            {
                textMesh.gameObject.SetActive(false);
            }
        }
    }
}