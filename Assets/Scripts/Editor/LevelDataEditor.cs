using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LevelData levelData = (LevelData)target;
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shooter Blocks Quick Actions", EditorStyles.boldLabel);
        
        SerializedProperty shooterBlockCountProperty = serializedObject.FindProperty("shooterBlockCount");
        if (shooterBlockCountProperty != null)
        {
            EditorGUILayout.PropertyField(shooterBlockCountProperty);
        }
        
        SerializedProperty shooterBlocksProperty = serializedObject.FindProperty("shooterBlocks");
        if (shooterBlocksProperty != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shooter Blocks:", EditorStyles.boldLabel);
            
            if (shooterBlockCountProperty != null)
            {
                int targetSize = shooterBlockCountProperty.intValue;
                if (shooterBlocksProperty.arraySize != targetSize)
                {
                    shooterBlocksProperty.arraySize = targetSize;
                }
            }
            
            for (int i = 0; i < shooterBlocksProperty.arraySize; i++)
            {
                SerializedProperty shooterBlockProperty = shooterBlocksProperty.GetArrayElementAtIndex(i);
                SerializedProperty colorProperty = shooterBlockProperty.FindPropertyRelative("color");
                SerializedProperty bulletCountProperty = shooterBlockProperty.FindPropertyRelative("bulletCount");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Shooter {i + 1}:", GUILayout.Width(80));
                
                if (colorProperty != null)
                {
                    EditorGUILayout.PropertyField(colorProperty, GUIContent.none, GUILayout.Width(80));
                }
                
                if (bulletCountProperty != null)
                {
                    EditorGUILayout.PropertyField(bulletCountProperty, GUIContent.none, GUILayout.Width(60));
                }
                
                if (GUILayout.Button("Y", GUILayout.Width(25)))
                {
                    colorProperty.enumValueIndex = 0;
                }
                if (GUILayout.Button("B", GUILayout.Width(25)))
                {
                    colorProperty.enumValueIndex = 1;
                }
                if (GUILayout.Button("R", GUILayout.Width(25)))
                {
                    colorProperty.enumValueIndex = 2;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Line Quick Actions", EditorStyles.boldLabel);
        
        SerializedProperty levelLayoutProperty = serializedObject.FindProperty("levelLayout");
        if (levelLayoutProperty != null)
        {
            for (int lineIndex = 0; lineIndex < levelLayoutProperty.arraySize; lineIndex++)
            {
                SerializedProperty lineProperty = levelLayoutProperty.GetArrayElementAtIndex(lineIndex);
                SerializedProperty blocksProperty = lineProperty.FindPropertyRelative("blocks");
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"Line {lineIndex}:", GUILayout.Width(60));
                
                if (GUILayout.Button("Yellow", GUILayout.Width(60)))
                {
                    FillLineWithColor(blocksProperty, BlockColor.Yellow);
                }
                
                if (GUILayout.Button("Blue", GUILayout.Width(50)))
                {
                    FillLineWithColor(blocksProperty, BlockColor.Blue);
                }
                
                if (GUILayout.Button("Red", GUILayout.Width(50)))
                {
                    FillLineWithColor(blocksProperty, BlockColor.Red);
                }
                
                if (GUILayout.Button("Random", GUILayout.Width(70)))
                {
                    RandomFillLine(blocksProperty);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void FillLineWithColor(SerializedProperty blocksProperty, BlockColor color)
    {
        for (int i = 0; i < blocksProperty.arraySize; i++)
        {
            SerializedProperty blockProperty = blocksProperty.GetArrayElementAtIndex(i);
            blockProperty.enumValueIndex = (int)color;
        }
        blocksProperty.serializedObject.ApplyModifiedProperties();
    }
    
    void RandomFillLine(SerializedProperty blocksProperty)
    {
        for (int i = 0; i < blocksProperty.arraySize; i++)
        {
            SerializedProperty blockProperty = blocksProperty.GetArrayElementAtIndex(i);
            blockProperty.enumValueIndex = Random.Range(0, 3);
        }
        blocksProperty.serializedObject.ApplyModifiedProperties();
    }
    
}

[CustomPropertyDrawer(typeof(BlockColorLine))]
public class BlockColorLineDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        string lineNumber = label.text.Replace("Element ", "Line ");
        GUIContent newLabel = new GUIContent(lineNumber);
        
        EditorGUI.PropertyField(position, property, newLabel, true);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

[CustomPropertyDrawer(typeof(BlockColor))]
public class BlockColorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        string blockLabel = label.text;
        if (blockLabel.StartsWith("Element "))
        {
            string parentPath = property.propertyPath;
            string lineNumber = GetLineNumberFromPath(parentPath);
            string blockNumber = blockLabel.Replace("Element ", "");
            blockLabel = $"Block {lineNumber}-{blockNumber}";
        }
        
        GUIContent newLabel = new GUIContent(blockLabel);
        EditorGUI.PropertyField(position, property, newLabel, true);
        
        EditorGUI.EndProperty();
    }
    
    private string GetLineNumberFromPath(string path)
    {
        if (path.Contains("levelLayout.Array.data["))
        {
            int startIndex = path.IndexOf("levelLayout.Array.data[") + "levelLayout.Array.data[".Length;
            int endIndex = path.IndexOf("]", startIndex);
            if (endIndex > startIndex)
            {
                return path.Substring(startIndex, endIndex - startIndex);
            }
        }
        return "0";
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
