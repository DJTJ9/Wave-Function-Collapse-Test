using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public TileSO[] tileOptions;

    public void CreateCell(bool collapseState, TileSO[] tiles) {
        this.collapsed = collapseState;

        // Fallback, falls tiles leer oder null ist
        if (tiles == null || tiles.Length == 0) {
            Debug.LogError("Tile-Array ist leer oder null! Standard-Array wird gesetzt.");
            this.tileOptions = new TileSO[0]; // Alternativ: mit Standardwerten initialisieren
        } else {
            this.tileOptions = tiles;
        }
    }

    public void RecreateCell(TileSO[] tiles) {
        tileOptions = tiles;
    }
}
