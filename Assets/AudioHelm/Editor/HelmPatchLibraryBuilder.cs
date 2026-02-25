#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioHelm
{
    public class HelmPatchLibraryBuilder
    {
        [MenuItem("Helm/Build Patch Library")]
        public static void BuildLibrary()
        {
            // Update this path if your patches are elsewhere
            string patchRoot = "Assets/StreamingAssets/HelmPatches";

            if (!Directory.Exists(patchRoot))
            {
                // Try Resources path in case already moved
                patchRoot = "Assets/Resources/HelmPatches";
            }

            if (!Directory.Exists(patchRoot))
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find HelmPatches folder.\n\n" +
                    "Expected at:\n" +
                    "  Assets/StreamingAssets/HelmPatches\n" +
                    "  Assets/Resources/HelmPatches",
                    "OK");
                return;
            }

            // Ensure patches are in Resources so they can be TextAssets
            string resourcesPath = "Assets/Resources/HelmPatches";
            if (patchRoot != resourcesPath)
            {
                if (Directory.Exists(resourcesPath))
                    Directory.Delete(resourcesPath, true);

                CopyDirectory(patchRoot, resourcesPath);
                RenameHelmFilesToBytes(resourcesPath);
                AssetDatabase.Refresh();
            }
            else
            {
                // Make sure files have .bytes extension
                RenameHelmFilesToBytes(resourcesPath);
                AssetDatabase.Refresh();
            }

            // Build the ScriptableObject
            HelmPatchLibrary library = ScriptableObject.CreateInstance<HelmPatchLibrary>();

            string[] folders = Directory.GetDirectories(resourcesPath)
                .OrderBy(d => Path.GetFileName(d))
                .ToArray();

            List<HelmPatchLibrary.PatchFolder> patchFolders = new List<HelmPatchLibrary.PatchFolder>();

            foreach (string folder in folders)
            {
                string folderName = Path.GetFileName(folder);
                string resourceFolder = "HelmPatches/" + folderName;

                TextAsset[] patches = Resources.LoadAll<TextAsset>(resourceFolder)
                    .OrderBy(p => p.name)
                    .ToArray();

                if (patches.Length > 0)
                {
                    // Use asset paths for proper serialization
                    List<TextAsset> assetRefs = new List<TextAsset>();
                    string[] files = Directory.GetFiles(folder, "*.bytes")
                        .OrderBy(f => f)
                        .ToArray();

                    foreach (string file in files)
                    {
                        string assetPath = file.Replace("\\", "/");
                        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                        if (asset != null)
                            assetRefs.Add(asset);
                    }

                    patchFolders.Add(new HelmPatchLibrary.PatchFolder
                    {
                        folderName = folderName,
                        patches = assetRefs.ToArray()
                    });
                }
            }

            library.folders = patchFolders.ToArray();

            // Save the asset
            string libraryPath = "Assets/Resources/PatchLibrary.asset";
            AssetDatabase.CreateAsset(library, libraryPath);
            AssetDatabase.SaveAssets();

            int totalPatches = patchFolders.Sum(f => f.patches.Length);
            EditorUtility.DisplayDialog("Success",
                $"Built Patch Library:\n" +
                $"  {patchFolders.Count} folders\n" +
                $"  {totalPatches} patches\n\n" +
                $"Saved to: {libraryPath}",
                "OK");

            Selection.activeObject = library;
        }

        static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (string file in Directory.GetFiles(source))
            {
                if (file.EndsWith(".meta")) continue;
                string destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(source))
            {
                string destDir = Path.Combine(dest, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        static void RenameHelmFilesToBytes(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.helm", SearchOption.AllDirectories))
            {
                string newPath = file + ".bytes";
                if (!File.Exists(newPath))
                    File.Move(file, newPath);
            }
        }
    }
}
#endif