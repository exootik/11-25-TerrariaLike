using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(BreakableTile))]
public class BreakableTileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BreakableTile tile = (BreakableTile)target;

        EditorGUILayout.LabelField("Breakable Tile Settings", EditorStyles.boldLabel);

        tile.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", tile.sprite, typeof(Sprite), false);
        tile.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Collider Type", tile.colliderType);

        EditorGUILayout.Space();
        tile.isBreakable = EditorGUILayout.Toggle("Is Breakable", tile.isBreakable);
        tile.HasGravity = EditorGUILayout.Toggle("Has Gravity", tile.HasGravity);

        if (tile.isBreakable)
        {
            tile.breakTime = EditorGUILayout.FloatField("Break Time", tile.breakTime);
            tile.requiredTool = (BreakableTile.ToolType)EditorGUILayout.EnumPopup("Required Tool", tile.requiredTool);
            tile.dropPrefab = (GameObject)EditorGUILayout.ObjectField("Drop Prefab", tile.dropPrefab, typeof(GameObject), false);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tile);
        }
    }
}