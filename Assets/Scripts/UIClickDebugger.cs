using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// �}�E�X�J�[�\������UI�v�f�����o���ă��O�ɏo�͂���f�o�b�O�p�X�N���v�g
public class UIClickDebugger : MonoBehaviour
{
    // Inspector����ݒ肷��
    public GraphicRaycaster m_Raycaster;
    public EventSystem m_EventSystem;

    private PointerEventData m_PointerEventData;

    void Update()
    {
        // �}�E�X�������Ă���΃`�F�b�N
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // �}�E�X�J�[�\������UI�����o����
            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointerEventData, results);

            // ���o���ꂽUI�v�f�̖��O�����ׂă��O�ɏo��
            if (results.Count > 0)
            {
                Debug.Log("---------- �}�E�X�J�[�\���̉��ɂ���UI ----------");
                foreach (RaycastResult result in results)
                {
                    Debug.Log("�q�b�g: " + result.gameObject.name);
                }
            }
        }
    }
}