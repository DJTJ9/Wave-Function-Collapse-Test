using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WaveFunction : MonoBehaviour {
    public int dimensions;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Cell cellObj;

    int iterations = 0;

    void Awake() {
        if (tileObjects == null || tileObjects.Length == 0) {
            Debug.LogError("tileObjects ist leer oder null. Bitte initialisiere die Tile-Objekte im Editor.");
            return;
        }
        
        gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid() {
        for (int y = 0; y < dimensions; y++) {
            for (int x = 0; x < dimensions; x++) {
                Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
                
                if (newCell == null) {
                    Debug.LogError($"Fehler beim Erstellen der Zelle an Position ({x}, {y}).");
                    continue;
                }
                
                newCell.CreateCell(false, tileObjects);
                
                if (newCell.tileOptions == null || newCell.tileOptions.Length == 0) {
                    Debug.LogError($"Zelle an Position ({x}, {y}) hat keine gültigen Tile-Optionen.");
                }
                
                gridComponents.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }


    IEnumerator CheckEntropy() {
        List<Cell> tempGrid = new List<Cell>(gridComponents);

        tempGrid.RemoveAll(c => c.collapsed);
        
        // Prüfen, ob tempGrid leer ist
        if (tempGrid.Count == 0) {
            Debug.LogWarning("Alle Zellen sind bereits kollabiert. Verarbeitung beendet.");
            yield break; // Coroutine vorzeitig beenden
        }
        
        // Filtern: Nur Zellen mit gültigen Optionen behalten
        tempGrid = tempGrid.Where(cell => cell.tileOptions != null && cell.tileOptions.Length > 0).ToList();

        if (tempGrid.Count == 0) {
            Debug.LogError("Keine Zellen mit gültigen tileOptions vorhanden. Es gibt kein weiteres Element zu verarbeiten.");
            yield break; // Fehlerfall vorbeugen
        }

        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++) {
            if (tempGrid[i].tileOptions.Length > arrLength) {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0) {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(0.00001f);

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid) {
        if (tempGrid == null || tempGrid.Count == 0) {
            Debug.LogError("Keine Zellen übrig, die kollabiert werden können!");
            return;
        }
        
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randIndex];
        
        if (cellToCollapse.tileOptions == null || cellToCollapse.tileOptions.Length == 0) {
            Debug.LogError("Fehler: Zelle zum Kollabieren hat keine gültigen Optionen!");
            return;
        }

        cellToCollapse.collapsed = true;
        Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);

        UpdateGeneration();
    }

    void UpdateGeneration() {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++) {
            for (int x = 0; x < dimensions; x++) {
                var index = x + y * dimensions;
                if (gridComponents[index].collapsed) {
                    Debug.Log("called");
                    newGenerationCell[index] = gridComponents[index];
                }
                else {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects) {
                        options.Add(t);
                    }

                    //update above
                    if (y > 0) {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions) {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < dimensions - 1) {
                        Cell right = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions) {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1) {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions) {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0) {
                        Cell left = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions) {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++) {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;
        iterations++;

        if (iterations < dimensions * dimensions) {
            StartCoroutine(CheckEntropy());
        }

    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption) {
        for (int x = optionList.Count - 1; x >= 0; x--) {
            var element = optionList[x];
            if (!validOption.Contains(element)) {
                optionList.RemoveAt(x);
            }
        }
    }
}