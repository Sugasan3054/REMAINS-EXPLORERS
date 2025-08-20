using UnityEngine;
// System.Diagnostics ���O��Ԃ��g�p���邽�߂ɁA���̍s��ǉ����܂�
using System.Diagnostics;

// ���̃X�N���v�g���A�^�b�`���ꂽ�I�u�W�F�N�g����\���ɂ��ꂽ�u�ԂɁA
// �N����������s�������̗��������O�ɏo�͂���
public class DisableViewDetector : MonoBehaviour
{
    // ���̃I�u�W�F�N�g����A�N�e�B�u�ɂ��ꂽ�u�ԂɁAUnity�ɂ���Ď����I�ɌĂ΂�閽��
    private void OnDisable()
    {
        // ���݂̌Ăяo�������i�R�[���X�^�b�N�j���擾
        StackTrace stackTrace = new StackTrace(true); // true�ōs�ԍ��Ȃǂ̏ڍ׏����擾

        // �擾�����������A���₷���悤�ɐ��`���ă��O�ɏo�͂���
        // Debug.Log���̂������Ɋ܂܂�邽�߁A1�t���[�����X�L�b�v���ĕ\��
        UnityEngine.Debug.Log(gameObject.name + " was disabled! Call Stack:\n" + stackTrace.ToString());
    }
}