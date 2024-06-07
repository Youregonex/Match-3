using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;
using System;

[Serializable]
public sealed class Board : MonoBehaviour
{
    private static Board _instance;
    public static Board Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Board();

            return _instance;
        }
    }

    [SerializeField] private Row[] _rows;

    [SerializeField] private List<Tile> _selection = new();

    private const float TWEEN_DURATION = .25f;

    public Tile[,] GridTiles { get; private set; }

    public int Width => GridTiles.GetLength(0);
    public int Height => GridTiles.GetLength(1);

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        GridTiles = new Tile[_rows.Max(row => row.RowTiles.Length), _rows.Length];

        FillGridTiles();
    }

    private void FillGridTiles()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = _rows[y].RowTiles[x];

                tile.SetTilePosition(new Vector2Int(x, y));

                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                GridTiles[x, y] = tile;
            }
        }
    }

    public async void SelectTile(Tile tile)
    {
        if(!_selection.Contains(tile))
        {
            if(_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) == -1)
                    _selection.Clear();
                else
                    _selection.Add(tile);
                
            }
            else
            {
                _selection.Add(tile);
            }
        }


        if (_selection.Count < 2) return;

        await SwapTiles(_selection[0], _selection[1]);

        if(CanPop())
        {
            Pop();
        }
        else
        {
            await SwapTiles(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task SwapTiles(Tile firstTile, Tile secondTile)
    {
        Image firstIcon = firstTile.Icon;
        Image secondIcon = secondTile.Icon;

        Transform firstIconTransform = firstIcon.transform;
        Transform secondIconTransform = secondIcon.transform;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(firstIconTransform.DOMove(secondIconTransform.position, TWEEN_DURATION))
                .Join(secondIconTransform.DOMove(firstIconTransform.position, TWEEN_DURATION));

        await sequence.Play().AsyncWaitForCompletion();

        firstIconTransform.SetParent(secondTile.transform);
        secondIconTransform.SetParent(firstTile.transform);

        firstTile.SetTileIcon(secondIcon);
        secondTile.SetTileIcon(firstIcon);

        Item firstTileItem = firstTile.Item;

        firstTile.Item = secondTile.Item;
        secondTile.Item = firstTileItem;
    }

    private bool CanPop()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (GridTiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                    return true;
            }
        }

        return false;
    }

    private async void Pop()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = GridTiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2)
                    continue;

                Sequence deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.Icon.transform.DOScale(Vector3.zero, TWEEN_DURATION));
                }

                ScoreCounter.Instance.Score += tile.Item.ItemValue * connectedTiles.Count();

                await deflateSequence.Play().AsyncWaitForCompletion();

                Sequence inflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                    inflateSequence.Join(connectedTile.Icon.transform.DOScale(Vector3.one, TWEEN_DURATION));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }

}
