using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Weight Bundle", menuName = "WFC/Tile Weight Bundle")]
public class TileWeightBundleSO : ScriptableObject
{
    public TileWeigthBundle Tile;
    
    public TileWeigthBundle[] upNeighbors;
    public TileWeigthBundle[] downNeighbors;
    public TileWeigthBundle[] leftNeighbors;
    public TileWeigthBundle[] rightNeighbors;
}

[System.Serializable]
public struct TileWeigthBundle
{
    public Tile Tile;
    public int Weight;
}