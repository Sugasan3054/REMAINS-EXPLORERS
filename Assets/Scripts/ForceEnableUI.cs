using UnityEngine;
using UnityEngine.UI;

// ���̃X�N���v�g�́A�A�^�b�`���ꂽ�I�u�W�F�N�g��Image��Button�R���|�[�l���g�������I�ɗL������������
public class ForceEnableUI : MonoBehaviour
{
    private Image imageComponent;
    private Button buttonComponent;

    void Awake()
    {
        // �ŏ��ɃR���|�[�l���g���擾���Ă���
        imageComponent = GetComponent<Image>();
        buttonComponent = GetComponent<Button>();
    }

    // ���t���[���Ă΂��Update�ŁA�L����Ԃ��Ď��E��������
    void Update()
    {
        // ����Image�������ɂȂ��Ă�����A�L���ɖ߂�
        if (imageComponent != null && !imageComponent.enabled)
        {
            imageComponent.enabled = true;
        }

        // ����Button�������ɂȂ��Ă�����A�L���ɖ߂�
        if (buttonComponent != null && !buttonComponent.enabled)
        {
            buttonComponent.enabled = true;
        }
    }
}