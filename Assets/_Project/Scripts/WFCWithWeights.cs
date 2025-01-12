// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// public class WFCWithWeights : MonoBehaviour
// {
//     public int dimensions;
//     public TileWeightBundleSO[] tileObjects; // Geändert: TileWeightBundleSO[] anstelle von TileWeigthBundle[]
//     public List<Cell> gridComponents;
//     public Cell cellObj;
//
//     int iterations = 0;
//
//     void Awake() {
//         if (tileObjects == null || tileObjects.Length == 0) {
//             Debug.LogError("tileObjects ist leer oder null. Bitte initialisiere die Tile-Objekte im Editor.");
//             return;
//         }
//         
//         gridComponents = new List<Cell>();
//         InitializeGrid();
//     }
//
//     void InitializeGrid() {
//         for (int y = 0; y < dimensions; y++) {
//             for (int x = 0; x < dimensions; x++) {
//                 Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
//                 
//                 if (newCell == null) {
//                     Debug.LogError($"Fehler beim Erstellen der Zelle an Position ({x}, {y}).");
//                     continue;
//                 }
//                 
//                 // Initialisierung: Übergibt die tileOptions an die Zelle aus den tileObjects
//                 newCell.CreateCell(false, tileObjects.Select(t => t.Tile).ToArray());
//                 
//                 if (newCell.tileOptions == null || newCell.tileOptions.Length == 0) {
//                     Debug.LogError($"Zelle an Position ({x}, {y}) hat keine gültigen Tile-Optionen.");
//                 }
//                 
//                 gridComponents.Add(newCell);
//             }
//         }
//
//         StartCoroutine(CheckEntropy());
//     }
//
//     IEnumerator CheckEntropy() {
//         List<Cell> tempGrid = new List<Cell>(gridComponents);
//
//         tempGrid.RemoveAll(c => c.collapsed);
//         
//         if (tempGrid.Count == 0) {
//             Debug.LogWarning("Alle Zellen sind bereits kollabiert. Verarbeitung beendet.");
//             yield break;
//         }
//         
//         tempGrid = tempGrid.Where(cell => cell.tileOptions != null && cell.tileOptions.Length > 0).ToList();
//
//         if (tempGrid.Count == 0) {
//             Debug.LogError("Keine Zellen mit gültigen tileOptions vorhanden. Es gibt kein weiteres Element zu verarbeiten.");
//             yield break;
//         }
//
//         tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });
//
//         int arrLength = tempGrid[0].tileOptions.Length;
//         int stopIndex = default;
//
//         for (int i = 1; i < tempGrid.Count; i++) {
//             if (tempGrid[i].tileOptions.Length > arrLength) {
//                 stopIndex = i;
//                 break;
//             }
//         }
//
//         if (stopIndex > 0) {
//             tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
//         }
//
//         yield return new WaitForSeconds(0.01f);
//         CollapseCell(tempGrid);
//     }
//
//     void CollapseCell(List<Cell> tempGrid) {
//         if (tempGrid == null || tempGrid.Count == 0) {
//             Debug.LogError("Keine Zellen übrig, die kollabiert werden können!");
//             return;
//         }
//     
//         int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
//         Cell cellToCollapse = tempGrid[randIndex];
//     
//         if (cellToCollapse.tileOptions == null || cellToCollapse.tileOptions.Length == 0) {
//             Debug.LogError("Fehler: Zelle zum Kollabieren hat keine gültigen Optionen!");
//             return;
//         }
//
//         cellToCollapse.collapsed = true;
//
//         TileWeigthBundle selectedTile = GetWeightedRandomTile(cellToCollapse.tileOptions);
//         cellToCollapse.tileOptions = new TileWeigthBundle[] { selectedTile };
//
//         Tile foundTile = cellToCollapse.tileOptions[0].Tile;
//         Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);
//
//         UpdateGeneration();
//     }
//
//     TileWeigthBundle GetWeightedRandomTile(TileWeigthBundle[] tileOptions) {
//         int totalWeight = 0;
//         foreach (var tile in tileOptions) {
//             totalWeight += tile.Weight;
//         }
//
//         int randomValue = UnityEngine.Random.Range(0, totalWeight);
//
//         foreach (var tile in tileOptions) {
//             if (randomValue < tile.Weight) {
//                 return tile;
//             }
//             randomValue -= tile.Weight;
//         }
//
//         Debug.LogError("Fehler bei der gewichteten Zufallsauswahl. Es wurde kein Tile gefunden.");
//         return tileOptions[0];
//     }
//
//     void UpdateGeneration() {
//         List<Cell> newGenerationCell = new List<Cell>(gridComponents);
//
//         for (int y = 0; y < dimensions; y++) {
//             for (int x = 0; x < dimensions; x++) {
//                 var index = x + y * dimensions;
//                 if (gridComponents[index].collapsed) {
//                     newGenerationCell[index] = gridComponents[index];
//                 }
//                 else {
//                     List<TileWeigthBundle> options = new List<TileWeigthBundle>();
//                     foreach (TileWeightBundleSO t in tileObjects) {
//                         options.Add(t.Tile);
//                     }
//
//                     if (y > 0) {
//                         Cell up = gridComponents[x + (y - 1) * dimensions];
//                         List<TileWeigthBundle> validOptions = new List<TileWeigthBundle>();
//
//                         foreach (TileWeigthBundle possibleOptions in up.tileOptions) {
//                             TileWeightBundleSO tileBundle = tileObjects.FirstOrDefault(t => t.Tile.Equals(possibleOptions));
//                             if (tileBundle != null && tileBundle.upNeighbors != null) {
//                                 validOptions.AddRange(tileBundle.upNeighbors);
//                             }
//                         }
//
//                         CheckValidity(options, validOptions);
//                     }
//
//                     if (x < dimensions - 1) {
//                         Cell right = gridComponents[x + 1 + y * dimensions];
//                         List<TileWeigthBundle> validOptions = new List<TileWeigthBundle>();
//
//                         foreach (TileWeigthBundle possibleOptions in right.tileOptions) {
//                             TileWeightBundleSO tileBundle = tileObjects.FirstOrDefault(t => t.Tile.Equals(possibleOptions));
//                             if (tileBundle != null && tileBundle.leftNeighbors != null) {
//                                 validOptions.AddRange(tileBundle.leftNeighbors);
//                             }
//                         }
//
//                         CheckValidity(options, validOptions);
//                     }
//
//                     if (y < dimensions - 1) {
//                         Cell down = gridComponents[x + (y + 1) * dimensions];
//                         List<TileWeigthBundle> validOptions = new List<TileWeigthBundle>();
//
//                         foreach (TileWeigthBundle possibleOptions in down.tileOptions) {
//                             TileWeightBundleSO tileBundle = tileObjects.FirstOrDefault(t => t.Tile.Equals(possibleOptions));
//                             if (tileBundle != null && tileBundle.downNeighbors != null) {
//                                 validOptions.AddRange(tileBundle.downNeighbors);
//                             }
//                         }
//
//                         CheckValidity(options, validOptions);
//                     }
//
//                     if (x > 0) {
//                         Cell left = gridComponents[x - 1 + y * dimensions];
//                         List<TileWeigthBundle> validOptions = new List<TileWeigthBundle>();
//
//                         foreach (TileWeigthBundle possibleOptions in left.tileOptions) {
//                             TileWeightBundleSO tileBundle = tileObjects.FirstOrDefault(t => t.Tile.Equals(possibleOptions));
//                             if (tileBundle != null && tileBundle.rightNeighbors != null) {
//                                 validOptions.AddRange(tileBundle.rightNeighbors);
//                             }
//                         }
//
//                         CheckValidity(options, validOptions);
//                     }
//
//                     TileWeigthBundle[] newTileList = options.ToArray();
//                     newGenerationCell[index].RecreateCell(newTileList);
//                 }
//             }
//         }
//
//         gridComponents = newGenerationCell;
//         iterations++;
//
//         if (iterations < dimensions * dimensions) {
//             StartCoroutine(CheckEntropy());
//         }
//     }
//
//     void CheckValidity(List<TileWeigthBundle> optionList, List<TileWeigthBundle> validOption) {
//         for (int x = optionList.Count - 1; x >= 0; x--) {
//             if (!validOption.Contains(optionList[x])) {
//                 optionList.RemoveAt(x);
//             }
//         }
//     }
// }