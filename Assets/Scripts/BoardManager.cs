using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Enums;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private Tile mTilePrefab;
    [SerializeField] private Vector2Int mFieldSize = new Vector2Int(10, 10);
    [SerializeField] private int mTotalBoomCount = 15;
    [SerializeField] private GameObject mTilesRoot;

    private Tile[,] mTiles;
    private GameManager mGameManager;

    public void Initialize(GameManager gameManager)
    {
        mGameManager = gameManager;
        GenerateBoard();
        PlaceBooms();
        SetCounts();
    }

    private void GenerateBoard()
    {
        if (mTilesRoot != null)
        {
            foreach (Transform child in mTilesRoot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        mTiles = new Tile[mFieldSize.x, mFieldSize.y];

        for (int y = 0; y < mFieldSize.y; y++)
        {
            for (int x = 0; x < mFieldSize.x; x++)
            {
                Tile tile = Instantiate(mTilePrefab, mTilesRoot.transform);
                tile.Initialize(mGameManager, y * mFieldSize.x + x);
                mTiles[x, y] = tile;
            }
        }
    }

    private void PlaceBooms()
    {
        List<int> allIndices = new List<int>();
        for (int i = 0; i < mFieldSize.x * mFieldSize.y; i++)
        {
            allIndices.Add(i);
        }

        for (int i = 0; i < mTotalBoomCount; i++)
        {
            if (allIndices.Count == 0) break;
            int randomIndex = Random.Range(0, allIndices.Count);
            int index = allIndices[randomIndex];
            allIndices.RemoveAt(randomIndex);
            GetTile(index)?.SetBoom();
        }
    }

    private void SetCounts()
    {
        for (int y = 0; y < mFieldSize.y; y++)
        {
            for (int x = 0; x < mFieldSize.x; x++)
            {
                if (mTiles[x, y].TileType != TileType.BOOM)
                {
                    int count = GetAdjacentBoomCount(x, y);
                    mTiles[x, y].SetCount(count);
                }
            }
        }
    }

    private int GetAdjacentBoomCount(int x, int y)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < mFieldSize.x && ny >= 0 && ny < mFieldSize.y)
                {
                    if (mTiles[nx, ny].TileType == TileType.BOOM)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    public void DigAdjacentTiles(int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < mFieldSize.x && ny >= 0 && ny < mFieldSize.y)
                {
                    // isDirectClick引数にfalseを渡し、これが連鎖であることを伝える
                    mGameManager.OnTileClicked(mTiles[nx, ny].GetIndex(), false);
                }
            }
        }
    }

    public Tile GetTile(int index)
    {
        if (index < 0 || index >= mFieldSize.x * mFieldSize.y) return null;
        int x = index % mFieldSize.x;
        int y = index / mFieldSize.y;
        return mTiles[x, y];
    }

    public List<Tile> GetNumberedTiles()
    {
        List<Tile> numberedTiles = new List<Tile>();
        if (mTiles == null) return numberedTiles;

        foreach (Tile tile in mTiles)
        {
            if (tile.TileType == TileType.COUNT)
            {
                numberedTiles.Add(tile);
            }
        }
        return numberedTiles;
    }

    public List<Tile> GetAdjacentTiles(Tile centerTile)
    {
        List<Tile> neighbors = new List<Tile>();
        int index = centerTile.GetIndex();
        int x = index % mFieldSize.x;
        int y = index / mFieldSize.x;

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < mFieldSize.x && ny >= 0 && ny < mFieldSize.y)
                {
                    neighbors.Add(mTiles[nx, ny]);
                }
            }
        }
        return neighbors;
    }

    public List<Tile> FindInitialCluster()
    {
        List<Vector2Int> startingPoints = new List<Vector2Int>();
        for (int y = 0; y < mFieldSize.y - 1; y++)
        {
            for (int x = 0; x < mFieldSize.x - 1; x++)
            {
                startingPoints.Add(new Vector2Int(x, y));
            }
        }

        for (int i = 0; i < startingPoints.Count; i++)
        {
            Vector2Int temp = startingPoints[i];
            int randomIndex = Random.Range(i, startingPoints.Count);
            startingPoints[i] = startingPoints[randomIndex];
            startingPoints[randomIndex] = temp;
        }

        foreach (Vector2Int point in startingPoints)
        {
            Tile topLeft = mTiles[point.x, point.y];
            Tile topRight = mTiles[point.x + 1, point.y];
            Tile bottomLeft = mTiles[point.x, point.y + 1];
            Tile bottomRight = mTiles[point.x + 1, point.y + 1];

            if (topLeft.TileType == TileType.COUNT &&
                topRight.TileType == TileType.COUNT &&
                bottomLeft.TileType == TileType.COUNT &&
                bottomRight.TileType == TileType.COUNT)
            {
                return new List<Tile> { topLeft, topRight, bottomLeft, bottomRight };
            }
        }

        return null;
    }

    public int GetDiggedTileCount()
    {
        int count = 0;
        if (mTiles == null) return 0;
        foreach (var tile in mTiles)
        {
            if (tile.IsDigged) count++;
        }
        return count;
    }

    public int GetTotalSafeTileCount()
    {
        return mFieldSize.x * mFieldSize.y - mTotalBoomCount;
    }

    public void SwapTiles(int index1, int index2)
    {
        Tile tile1 = GetTile(index1);
        Tile tile2 = GetTile(index2);

        if (tile1 == null || tile2 == null) return;

        // 1. 交換する2つのタイルの「中身」を一時的に記憶する
        TileType tempType1 = tile1.TileType;
        int tempBombCount1 = tile1.AdjacentBombCount;

        TileType tempType2 = tile2.TileType;
        int tempBombCount2 = tile2.AdjacentBombCount;

        // 2. それぞれのタイルに、相手の中身を「上書き」する
        //    (Tile.csに以前追加したResetAndSetTypeメソッドを使用)
        tile1.ResetAndSetType(tempType2, tempBombCount2);
        tile2.ResetAndSetType(tempType1, tempBombCount1);

        // 3. 爆弾の位置が変わった可能性があるので、ボード全体の数字を再計算する
        //SetCounts();
    }

    public List<int> GetRandomSafeTiles(int count)
    {
        List<int> safeUnopenedIndices = new List<int>();
        // 1. 開かれていない安全なマスをすべて探す
        for (int i = 0; i < mFieldSize.x * mFieldSize.y; i++)
        {
            Tile tile = GetTile(i);
            if (tile != null && tile.TileType != TileType.BOOM && !tile.IsDigged)
            {
                safeUnopenedIndices.Add(i);
            }
        }

        // 2. 見つかったマスのリストをシャッフルする
        for (int i = 0; i < safeUnopenedIndices.Count; i++)
        {
            int temp = safeUnopenedIndices[i];
            int randomIndex = Random.Range(i, safeUnopenedIndices.Count);
            safeUnopenedIndices[i] = safeUnopenedIndices[randomIndex];
            safeUnopenedIndices[randomIndex] = temp;
        }

        // 3. 指定された数だけ、リストの先頭から取り出して返す
        int resultCount = Mathf.Min(count, safeUnopenedIndices.Count);
        return safeUnopenedIndices.GetRange(0, resultCount);
    }
}