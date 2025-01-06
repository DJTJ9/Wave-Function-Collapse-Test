using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Tile))]
public class TileInspector : Editor
{
    private Tile tile; // Referenz auf die aktuelle Tile-Komponente
    private List<GameObject> tilePrefabs = new List<GameObject>(); // Alle verfügbaren Prefabs
    private List<bool> selectedPrefabs = new List<bool>(); // Auswahlstatus der Prefabs
    private Vector2 scrollPos; // Für die lange Prefab-Liste
    private int addCount = 1; // Anzahl, wie oft ein Prefab hinzugefügt werden soll

    private void OnEnable()
    {
        tile = (Tile)target;

        // Prefabs aus dem Ordner Assets/_Project/Prefabs laden
        LoadPrefabs("Assets/_Project/Prefabs", tilePrefabs);

        // Initialisiere die Auswahlstatus-Liste
        selectedPrefabs.Clear();
        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            selectedPrefabs.Add(false);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(); // Abstand
        EditorGUILayout.LabelField("Prefab Selector", EditorStyles.boldLabel);

        // Anzahl der hinzuzufügenden Elemente festlegen
        addCount = EditorGUILayout.IntField("Anzahl hinzuzufügender Prefabs", addCount);

        // Verfügbare Prefabs anzeigen
        if (tile != null && tilePrefabs.Count > 0)
        {
            EditorGUILayout.LabelField("Verfügbare Prefabs (zum Auswählen Haken setzen):");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            for (int i = 0; i < tilePrefabs.Count; i++)
            {
                DrawPrefabSelection(tilePrefabs[i], i);
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Keine verfügbaren Prefabs gefunden. Stelle sicher, dass sich Prefabs im Ordner 'Assets/_Project/Prefabs' befinden.", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Nachbarn hinzufügen", EditorStyles.boldLabel);

        // Buttons: Füge die ausgewählten Prefabs in die entsprechenden Listen ein
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add to UP"))
        {
            AddSelectedPrefabsToArray(ref tile.upNeighbours, addCount);
        }
        if (GUILayout.Button("Add to DOWN"))
        {
            AddSelectedPrefabsToArray(ref tile.downNeighbours, addCount);
        }
        if (GUILayout.Button("Add to LEFT"))
        {
            AddSelectedPrefabsToArray(ref tile.leftNeighbours, addCount);
        }
        if (GUILayout.Button("Add to RIGHT"))
        {
            AddSelectedPrefabsToArray(ref tile.rightNeighbours, addCount);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove from UP"))
        {
            RemoveSelectedPrefabsFromArray(ref tile.upNeighbours, addCount);
        }
        if (GUILayout.Button("Remove from DOWN"))
        {
            RemoveSelectedPrefabsFromArray(ref tile.downNeighbours, addCount);
        }
        if (GUILayout.Button("Remove from LEFT"))
        {
            RemoveSelectedPrefabsFromArray(ref tile.leftNeighbours, addCount);
        }
        if (GUILayout.Button("Remove from RIGHT"))
        {
            RemoveSelectedPrefabsFromArray(ref tile.rightNeighbours, addCount);
        }
        EditorGUILayout.EndHorizontal();

        // Änderungen speichern
        if (GUI.changed)
        {
            EditorUtility.SetDirty(tile);
        }
    }

    private void DrawPrefabSelection(GameObject prefab, int index)
    {
        EditorGUILayout.BeginHorizontal();

        // Checkbox für die Auswahl des Prefabs
        selectedPrefabs[index] = EditorGUILayout.Toggle(selectedPrefabs[index], GUILayout.Width(20));

        // Zeige den Prefab-Namen
        EditorGUILayout.LabelField(prefab.name, GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();
    }

    private void AddSelectedPrefabsToArray(ref Tile[] neighbourArray, int count)
    {
        var newNeighbours = new List<Tile>(neighbourArray);

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            if (selectedPrefabs[i]) // Nur ausgewählte Prefabs hinzufügen
            {
                var prefabTile = tilePrefabs[i].GetComponent<Tile>();
                if (prefabTile == null)
                {
                    Debug.LogError($"Das Prefab {tilePrefabs[i].name} ist keine Tile-Komponente und wurde übersprungen.");
                    continue;
                }

                // Das Prefab mehrfach hinzufügen
                for (int j = 0; j < count; j++)
                {
                    newNeighbours.Add(prefabTile);
                }
            }
        }

        // Aktualisiere das Nachbarn-Array
        neighbourArray = newNeighbours.ToArray();
    }
    
    private void RemoveSelectedPrefabsFromArray(ref Tile[] neighbourArray, int count)
    {
        var newNeighbours = new List<Tile>(neighbourArray);

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            if (selectedPrefabs[i]) // Nur ausgewählte Prefabs hinzufügen
            {
                var prefabTile = tilePrefabs[i].GetComponent<Tile>();
                if (prefabTile == null)
                {
                    Debug.LogError($"Das Prefab {tilePrefabs[i].name} ist keine Tile-Komponente und wurde übersprungen.");
                    continue;
                }

                // Das Prefab mehrfach hinzufügen
                for (int j = 0; j < count; j++)
                {
                    newNeighbours.Remove(prefabTile);
                }
            }
        }

        // Aktualisiere das Nachbarn-Array
        neighbourArray = newNeighbours.ToArray();
    }

    private void LoadPrefabs(string folderPath, List<GameObject> prefabList)
    {
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        prefabList.Clear();

        foreach (string prefabGUID in prefabPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabList.Add(prefab);
            }
        }
    }
}