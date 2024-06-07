using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class Tile : MonoBehaviour
{
    private Item _item;
    public Item Item
    {
        get => _item;

        set
        {
            if (_item == value)
                return;

            _item = value;

            Icon.sprite = _item.Sprite;
        }
    }

    [field: SerializeField] public int PositionX { get; private set; }
    [field: SerializeField] public int PositionY { get; private set; }
    [field: SerializeField] public Image Icon { get; private set; }

    public Tile Left => PositionX > 0 ? Board.Instance.GridTiles[PositionX - 1, PositionY] : null;
    public Tile Right => PositionX + 1 < Board.Instance.Width ? Board.Instance.GridTiles[PositionX + 1, PositionY] : null;
    public Tile Top => PositionY > 0 ? Board.Instance.GridTiles[PositionX, PositionY - 1] : null;
    public Tile Bottom => PositionY + 1 < Board.Instance.Height ? Board.Instance.GridTiles[PositionX, PositionY + 1] : null;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom
    };

    [SerializeField] private Button _button;


    private void Start() => _button.onClick.AddListener(() => Board.Instance.SelectTile(this));

    public void SetTilePosition(Vector2Int position)
    {
        PositionX = position.x;
        PositionY = position.y;
    }

    public void SetTileIcon(Image icon)
    {
        Icon = icon;
    }

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        List<Tile> result = new List<Tile> { this, };

        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (Tile neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item)
                continue;

            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }

        return result;
    }

}
