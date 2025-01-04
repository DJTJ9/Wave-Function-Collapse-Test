using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PrefabSelectorWindow : EditorWindow
{
    private GameObject selectedPrefab; // Aktuell ausgewähltes Prefab
    private List<GameObject> tilePrefabs = new List<GameObject>(); // Alle Prefabs mit Tile-Komponente
    private Vector2 scrollPos; // Scrollposition für die lange Liste von Prefabs
    private List<GameObject> availablePrefabs = new List<GameObject>(); // Verfügbare Prefabs
    private List<bool> selectedStates = new List<bool>(); // Auswahlstatus der Prefabs (Checkbox-Status)

    [MenuItem("Tools/Prefab Selector")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabSelectorWindow>("Prefab Selector");
        window.minSize = new Vector2(400, 600); // Initiale Fenstergröße
    }

    private void OnEnable()
    {
        // Prefabs aus dem Ordner `Assets/_Project/Prefabs` laden
        LoadPrefabs("Assets/_Project/Prefabs");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefab Selector", EditorStyles.boldLabel);

        // Prefabs aus Ordner auswählen lassen
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Wähle ein Prefab mit einer 'Tile'-Komponente aus der Liste:");

        // Prefab-Auswahlliste im scrollbaren Bereich
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250)); // Begrenze Höhe des Scrollbereichs
        foreach (var prefab in tilePrefabs)
        {
            EditorGUILayout.BeginHorizontal();

            // Button für jede Prefab-Wahl
            if (GUILayout.Button(prefab.name, EditorStyles.miniButton))
            {
                SetSelectedPrefab(prefab); // Prefab auswählen
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // Prüfen, ob ein Prefab ausgewählt wurde
        if (selectedPrefab != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Aktuell ausgewähltes Prefab:", EditorStyles.boldLabel);

            // Zeige das ausgewählte Prefab
            EditorGUILayout.ObjectField("Prefab:", selectedPrefab, typeof(GameObject), false);

            // Zeige Liste der aktuell zugeordneten Prefabs innerhalb des ausgewählten Prefabs
            Tile tileComponent = selectedPrefab.GetComponent<Tile>();
            if (tileComponent != null)
            {
                EditorGUILayout.LabelField("Aktuelle Prefab-Liste:", EditorStyles.boldLabel);
                if (tileComponent.prefabs != null)
                {
                    foreach (var prefab in tileComponent.prefabs)
                    {
                        EditorGUILayout.ObjectField("Prefab:", prefab, typeof(GameObject), false);
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Verfügbare Prefabs zum Hinzufügen:", EditorStyles.boldLabel);

                // Scrollbare Liste für verfügbare Prefabs
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
                for (int i = 0; i < availablePrefabs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    selectedStates[i] = EditorGUILayout.Toggle(selectedStates[i], GUILayout.Width(20));
                    EditorGUILayout.ObjectField(availablePrefabs[i], typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                // Fixierte Buttons für Aktionen
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                // Button: Ausgewählte Prefabs hinzufügen
                if (GUILayout.Button("Ausgewählte Prefabs hinzufügen", GUILayout.Height(30)))
                {
                    AddSelectedPrefabsToTile(tileComponent);
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Das ausgewählte Prefab besitzt keine Tile-Komponente.", MessageType.Warning);
            }
        }
    }

    private void LoadPrefabs(string path)
    {
        tilePrefabs.Clear();

        // Lade alle Prefabs aus dem Ordner
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            // Filter: Nur Prefabs mit Tile-Komponente
            if (prefab != null && prefab.GetComponent<Tile>() != null)
            {
                tilePrefabs.Add(prefab);
            }
        }

        Debug.Log($"Es wurden {tilePrefabs.Count} Prefabs mit 'Tile'-Komponente aus '{path}' geladen.");
    }

    private void SetSelectedPrefab(GameObject prefab)
    {
        selectedPrefab = prefab;

        // Verfügbare Prefabs und Auswahlstatus aktualisieren
        Tile tile = selectedPrefab.GetComponent<Tile>();
        if (tile != null)
        {
            availablePrefabs = tilePrefabs.Where(p => !tile.prefabs.Contains(p)).ToList();
            selectedStates = new List<bool>(new bool[availablePrefabs.Count]);
        }
        else
        {
            availablePrefabs.Clear();
            selectedStates.Clear();
        }

        Debug.Log($"Prefab '{selectedPrefab.name}' ausgewählt.");
    }

    private void AddSelectedPrefabsToTile(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Das ausgewählte Prefab besitzt keine Tile-Komponente.");
            return;
        }

        // Hinzufügen der ausgewählten Prefabs
        for (int i = 0; i < availablePrefabs.Count; i++)
        {
            if (selectedStates[i])
            {
                GameObject prefabToAdd = availablePrefabs[i];
                if (!tile.prefabs.Contains(prefabToAdd))
                {
                    tile.prefabs.Add(prefabToAdd);
                }
            }
        }

        // Änderungen speichern
        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();

        Debug.Log("Ausgewählte Prefabs wurden hinzugefügt.");
    }
}