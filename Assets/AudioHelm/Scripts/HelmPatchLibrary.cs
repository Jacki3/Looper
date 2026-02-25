using UnityEngine;
namespace AudioHelm
{
    [CreateAssetMenu(fileName = "PatchLibrary", menuName = "Helm/Patch Library")]
    public class HelmPatchLibrary : ScriptableObject
    {
        [System.Serializable]
        public class PatchFolder
        {
            public string folderName;
            public TextAsset[] patches;
        }

        public PatchFolder[] folders;
    }
}
