using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public Tile[] tileOptions;

    public void CreateCell(bool collapseState, Tile[] tiles) {
        this.collapsed = collapsed;

        // Fallback, falls tiles leer oder null ist
        if (tiles == null || tiles.Length == 0) {
            Debug.LogError("Tile-Array ist leer oder null! Standard-Array wird gesetzt.");
            this.tileOptions = new Tile[0]; // Alternativ: mit Standardwerten initialisieren
        } else {
            this.tileOptions = tiles;
        }
    }

    public void RecreateCell(Tile[] tiles) {
        tileOptions = tiles;
    }
}
