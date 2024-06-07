using UnityEngine;

public sealed class Row : MonoBehaviour
{
    [field: SerializeField] public Tile[] RowTiles { get; private set; }
}
