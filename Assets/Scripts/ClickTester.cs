using UnityEngine;
using UnityEngine.EventSystems;

public class ClickTester : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // �N���b�N���ꂽ��AConsole�Ƀ��b�Z�[�W��\������
        Debug.Log("�N���b�N�����I");
    }
}