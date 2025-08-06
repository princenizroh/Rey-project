#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NarratorDay2))]
public class NarratorHelperEditor : Editor
{
    private NarratorDay2 narrator;
    
    private int motherSpawnIndex = 0;
    private int fatherSpawnIndex = 0;
    private int babySpawnIndex = 0;
    private int bidanSpawnIndex = 0;
    
    private int batchSpawnIndex = 0;
    
    void OnEnable()
    {
        narrator = (NarratorDay2)target;
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Position Setup Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate Spawn Positions", GUILayout.Height(20)))
        {
            bool isValid = narrator.ValidateSpawnPositions(5); 
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", "All characters have sufficient spawn positions!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Individual Character Snapping:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Mother:", GUILayout.Width(60));
        motherSpawnIndex = EditorGUILayout.IntSlider(motherSpawnIndex, 0, narrator.GetMaxSpawnIndex(CharacterType.Mother));
        if (GUILayout.Button($"Snap to {motherSpawnIndex}", GUILayout.Width(80)))
        {
            narrator.SnapCharacterToSpawn(CharacterType.Mother, motherSpawnIndex);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Father:", GUILayout.Width(60));
        fatherSpawnIndex = EditorGUILayout.IntSlider(fatherSpawnIndex, 0, narrator.GetMaxSpawnIndex(CharacterType.Father));
        if (GUILayout.Button($"Snap to {fatherSpawnIndex}", GUILayout.Width(80)))
        {
            narrator.SnapCharacterToSpawn(CharacterType.Father, fatherSpawnIndex);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Baby:", GUILayout.Width(60));
        babySpawnIndex = EditorGUILayout.IntSlider(babySpawnIndex, 0, narrator.GetMaxSpawnIndex(CharacterType.Baby));
        if (GUILayout.Button($"Snap to {babySpawnIndex}", GUILayout.Width(80)))
        {
            narrator.SnapCharacterToSpawn(CharacterType.Baby, babySpawnIndex);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Bidan:", GUILayout.Width(60));
        bidanSpawnIndex = EditorGUILayout.IntSlider(bidanSpawnIndex, 0, narrator.GetMaxSpawnIndex(CharacterType.Bidan));
        if (GUILayout.Button($"Snap to {bidanSpawnIndex}", GUILayout.Width(80)))
        {
            narrator.SnapCharacterToSpawn(CharacterType.Bidan, bidanSpawnIndex);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Multi-Character Control:", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("Snap All Characters to Selected Indices", GUILayout.Height(25)))
        {
            narrator.SnapCharactersToMultipleSpawns(motherSpawnIndex, fatherSpawnIndex, babySpawnIndex, bidanSpawnIndex);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Batch Operations:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Batch Index:", GUILayout.Width(80));
        batchSpawnIndex = EditorGUILayout.IntSlider(batchSpawnIndex, 0, 4); 
        if (GUILayout.Button($"Snap All to {batchSpawnIndex}", GUILayout.Width(100)))
        {
            narrator.SnapAllCharactersToSpawn(batchSpawnIndex);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("All to Spawn 0"))
        {
            motherSpawnIndex = fatherSpawnIndex = babySpawnIndex = bidanSpawnIndex = 0;
            narrator.SnapAllCharactersToSpawn(0);
        }
        if (GUILayout.Button("All to Spawn 1"))
        {
            motherSpawnIndex = fatherSpawnIndex = babySpawnIndex = bidanSpawnIndex = 1;
            narrator.SnapAllCharactersToSpawn(1);
        }
        if (GUILayout.Button("All to Spawn 2"))
        {
            motherSpawnIndex = fatherSpawnIndex = babySpawnIndex = bidanSpawnIndex = 2;
            narrator.SnapAllCharactersToSpawn(2);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Time-specific Setup:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Morning Setup"))
        {
            narrator.SetupMorningPositions();
            motherSpawnIndex = 0;
            fatherSpawnIndex = 0;
            babySpawnIndex = 0;
            bidanSpawnIndex = 1;
        }
        if (GUILayout.Button("Afternoon Setup"))
        {
            narrator.SetupAfternoonPositions();
            motherSpawnIndex = 1;
            fatherSpawnIndex = 2;
            babySpawnIndex = 1;
            bidanSpawnIndex = 3;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Evening Setup"))
        {
            narrator.SetupEveningPositions();
            motherSpawnIndex = 2;
            fatherSpawnIndex = 2;
            babySpawnIndex = 2;
            bidanSpawnIndex = 4;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Reset All Characters to Spawn 0", GUILayout.Height(25)))
        {
            narrator.ResetAllCharacterPositions();
            motherSpawnIndex = fatherSpawnIndex = babySpawnIndex = bidanSpawnIndex = 0;
        }
        
        EditorGUILayout.Space(5);
        
        
        EditorGUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
        EditorGUILayout.Space(5);
        
        DrawDefaultInspector();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(narrator);
        }
    }
}
#endif
