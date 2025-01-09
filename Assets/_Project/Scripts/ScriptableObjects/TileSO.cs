using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "WFC/Tile")]
public class TileSO : ScriptableObject
{
    public Tile Tile;
    public int Weight;
    
    public TileSO[] upNeighbors;
    public TileSO[] downNeighbors;
    public TileSO[] leftNeighbors;
    public TileSO[] rightNeighbors;
}

[System.Serializable]
public struct TileWeigthBundle
{
    public Tile Tile;
    public int weigth;
}