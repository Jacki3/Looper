using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteExporter
{
    [MenuItem("Assets/Export Selected Sprite as PNG")]
    public static void ExportSprite()
    {
        // Get the selected object in the Project window
        Object selectedObject = Selection.activeObject;

        if (selectedObject is Sprite sprite)
        {
            Texture2D texture = sprite.texture;
            Rect rect = sprite.textureRect;

            // Create a new texture with the sprite's dimensions
            Texture2D newTex = new Texture2D((int)rect.width, (int)rect.height);

            // Copy the pixels from the original sheet to the new texture
            Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            newTex.SetPixels(pixels);
            newTex.Apply();

            // Encode to PNG
            byte[] bytes = newTex.EncodeToPNG();
            string path = AssetDatabase.GetAssetPath(sprite);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.Combine(directory, sprite.name + ".png");

            // Save to disk
            File.WriteAllBytes(fileName, bytes);
            AssetDatabase.Refresh();

            Debug.Log($"Successfully exported: {fileName}");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select an individual Sprite slice (under the main texture).", "OK");
        }
    }
}