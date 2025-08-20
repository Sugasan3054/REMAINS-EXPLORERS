using UnityEngine;
using UnityEngine.UI;

// �^�C���̏�����Ԃ������I�ɐ������ݒ肷�邽�߂̃X�N���v�g
public class TileStateFixer : MonoBehaviour
{
    public void FixAllTileStates(Tile[,] tiles)
    {
        if (tiles == null) return;

        // ���ׂẴ^�C�������[�v�Ń`�F�b�N
        foreach (Tile tile in tiles)
        {
            // --- �e�I�u�W�F�N�g���̂��܂��A�N�e�B�u�ɂ��� ---
            tile.gameObject.SetActive(true);

            // --- �e�p�[�c��T���o���ď�Ԃ������I�ɐݒ� ---

            // Image_Base (�n��) ��T���āA�\����Ԃɂ���
            Transform imageBase = tile.transform.Find("Image_Base");
            if (imageBase != null)
            {
                imageBase.gameObject.SetActive(true);
                Image img = imageBase.GetComponent<Image>();
                if (img != null) img.enabled = true;
            }

            // Image_Cover (�t�^) ��T���āA�\����Ԃɂ��A�N���b�N�\�ɂ���
            Transform imageCover = tile.transform.Find("Image_Cover");
            if (imageCover != null)
            {
                imageCover.gameObject.SetActive(true);
                Image img = imageCover.GetComponent<Image>();
                if (img != null) img.enabled = true;
                Button btn = imageCover.GetComponent<Button>();
                if (btn != null) btn.enabled = true;
            }

            // Image_Boom (���g) ��T���āA��\����Ԃɂ���
            Transform imageBoom = tile.transform.Find("Image_Base/Image_Boom"); // �K�w�ɍ��킹�ďC��
            if (imageBoom != null)
            {
                imageBoom.gameObject.SetActive(false);
            }

            // Text (TMP) (���g) ��T���āA��\����Ԃɂ���
            Transform textMesh = tile.transform.Find("Image_Base/Text (TMP)"); // �K�w�ɍ��킹�ďC��
            if (textMesh != null)
            {
                textMesh.gameObject.SetActive(false);
            }
        }
    }
}