using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    // Die zentrale Prefabs-Liste, die im Inspector angezeigt wird
    [SerializeField]
    public List<GameObject> prefabs = new List<GameObject>();

    public Tile[] upNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;
    public Tile[] rightNeighbours;
    
    public TileWeigthBundle[] upNeighbors;
    public TileWeigthBundle[] downNeighbors;
    public TileWeigthBundle[] leftNeighbors;
    public TileWeigthBundle[] rightNeighbors;
}

[System.Serializable]
public struct TileWeigthBundle
{
    public Tile Tile;
    public int weigth;
}