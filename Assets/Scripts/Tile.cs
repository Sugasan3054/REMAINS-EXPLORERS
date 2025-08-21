using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static Enums;
using TMPro;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public TileType TileType { get; private set; } = TileType.EMPTY;
    public bool IsDigged { get; private set; } = false;
    public int AdjacentBombCount { get; private set; }
    private GameManager mGameManager;
    private MarkState mMarkState = MarkState.NO_MARK;
    private int mIndex;

    [Header("UI References")]
    [SerializeField] private GameObject mCover;
    [SerializeField] private GameObject mFlag;
    [SerializeField] private GameObject mQuestion;
    [SerializeField] private GameObject mRedBG;
    [SerializeField] private GameObject mRedCross;
    [SerializeField] private TMP_Text mCount;
    [SerializeField] private GameObject mBoom;
    [SerializeField] private GameObject mBaseImage;
    [SerializeField] private Color[] mCountCols = new Color[8];

    private void Awake()
    {
        if (mCover != null) mCover.SetActive(true);
        if (mBaseImage != null) mBaseImage.SetActive(true);
        if (mFlag != null) mFlag.SetActive(false);
        if (mQuestion != null) mQuestion.SetActive(false);
        if (mRedBG != null) mRedBG.SetActive(false);
        if (mRedCross != null) mRedCross.SetActive(false);
        if (mCount != null) mCount.gameObject.SetActive(false);
        if (mBoom != null) mBoom.gameObject.SetActive(false);
    }

    public void Initialize(GameManager manager, int index)
    {
        mGameManager = manager;
        mIndex = index;
    }

    public int GetIndex() => mIndex;

    // �N���b�N���������̃��\�b�h�Ɉ�{��
    // Tile.cs �̒�

    public void OnPointerClick(PointerEventData eventData)
    {
        if (mGameManager == null || mGameManager.currentGameData == null) return;

        // ������ ����if���̏��������C�� ������
        // �Q�[�����v���C���łȂ��A���A����ȃ��[�h�ł��Ȃ��ꍇ�́A�N���b�N���u���b�N����
        if (!mGameManager.currentGameData.isPlaying && !mGameManager.IsInProphecyMode && !mGameManager.IsInSwapMode)
        {
            return;
        }
        // ������ �����܂ŏC�� ������

        // �����ꂽ�}�E�X�̃{�^���ŏ����𕪊�
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnDigged();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (mGameManager.currentGameData.isPlaying)
            {
                OnRightClick();
            }
        }
    }

    // ���N���b�N�i�܂��̓^�b�v�j���̏���
    private void OnDigged()
    {
        if (IsDigged || mMarkState == MarkState.FLAG) return;
        mGameManager.OnTileClicked(mIndex);
    }

    // �E�N���b�N���̏���
    private void OnRightClick()
    {
        if (IsDigged) return;

        switch (mMarkState)
        {
            case MarkState.NO_MARK:
                mMarkState = MarkState.FLAG;
                if (mFlag != null) mFlag.SetActive(true);
                break;
            case MarkState.FLAG:
                mMarkState = MarkState.QUESTION;
                if (mFlag != null) mFlag.SetActive(false);
                if (mQuestion != null) mQuestion.SetActive(true);
                break;
            case MarkState.QUESTION:
                mMarkState = MarkState.NO_MARK;
                if (mQuestion != null) mQuestion.SetActive(false);
                break;
        }
    }

    public void OpenTile()
    {
        if (IsDigged) return;
        IsDigged = true;
        if (mCover != null) mCover.SetActive(false);
        if (TileType == TileType.BOOM && mRedBG != null)
        {
            mRedBG.SetActive(true);
        }
    }

    public void SetCount(int count)
    {
        this.AdjacentBombCount = count;
        TileType = (count == 0) ? TileType.EMPTY : TileType.COUNT;

        if (mCount != null)
        {
            if (TileType == TileType.COUNT)
            {
                mCount.gameObject.SetActive(true);
                mCount.text = count.ToString();

                if (count > 0 && count <= mCountCols.Length)
                {
                    // ������ �ȉ���3�s���������F�̐ݒ菈���ł� ������
                    Color textColor = mCountCols[count - 1];
                    textColor.a = 1f; // �����x��1�i�s�����j�ɐݒ�
                    mCount.color = textColor;
                    // ������ �����܂� ������
                }
            }
            else
            {
                mCount.gameObject.SetActive(false);
            }
        }
    }

    public void SetBoom()
    {
        TileType = TileType.BOOM;
        if (mBoom != null)
        {
            mBoom.SetActive(true);
        }
    }

    public void ResetAndSetType(TileType type, int count = 0)
    {
        if (mBoom != null) mBoom.SetActive(false);
        if (mCount != null) mCount.gameObject.SetActive(false);

        this.TileType = type;
        if (type == TileType.BOOM)
        {
            SetBoom();
        }
        else
        {
            SetCount(count);
        }
    }

    public void UpdateIndex(int newIndex)
    {
        mIndex = newIndex;
    }
}