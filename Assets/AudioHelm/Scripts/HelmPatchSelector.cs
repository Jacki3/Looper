using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace AudioHelm
{
    public class HelmPatchSelector : MonoBehaviour
    {
        [Header("References")]
        public HelmController helmController;
        public TMP_Dropdown folderDropdown;
        public TMP_Dropdown patchDropdown;
        public HelmPatchLibrary library;

        [Header("Default Patch (matches Editor selection)")]
        public string defaultFolder = "Keys";
        public string defaultPatch = "Piano";

        public static HelmPatch currentPatch;

        void Start()
        {
            folderDropdown.onValueChanged.AddListener(OnFolderChanged);
            patchDropdown.onValueChanged.AddListener(OnPatchChanged);

            PopulateFolders();

            string folder = PlayerPrefs.GetString("HelmFolder", defaultFolder);
            string patch = PlayerPrefs.GetString("HelmPatch", defaultPatch);
            SyncDropdownsToSavedPatch(folder, patch);

            StartCoroutine(ApplyInitialPatch());
        }

        IEnumerator ApplyInitialPatch()
        {
            yield return new WaitForSeconds(0.1f);
            OnPatchChanged(patchDropdown.value);
        }

        void PopulateFolders()
        {
            folderDropdown.ClearOptions();

            if (library == null || library.folders == null || library.folders.Length == 0)
            {
                Debug.LogWarning("HelmPatchLibrary is not assigned or empty.");
                return;
            }

            folderDropdown.AddOptions(
                library.folders.Select(f => f.folderName).ToList()
            );
            ResizeDropdownToFit(folderDropdown);

            if (library.folders.Length > 0)
                PopulatePatches(0);
        }

        void PopulatePatches(int folderIndex)
        {
            patchDropdown.ClearOptions();

            var patches = library.folders[folderIndex].patches;
            patchDropdown.AddOptions(
                patches.Select(p => p.name.Replace(".helm", "")).ToList()
            );
            ResizeDropdownToFit(patchDropdown);
        }

        void OnFolderChanged(int index)
        {
            PopulatePatches(index);

            if (library.folders[index].patches.Length > 0)
                OnPatchChanged(0);
        }

        void OnPatchChanged(int index)
        {
            var patches = library.folders[folderDropdown.value].patches;
            if (index < 0 || index >= patches.Length)
                return;

            string json = patches[index].text;
            HelmPatchFormat patchData = JsonUtility.FromJson<HelmPatchFormat>(json);

            GameObject temp = new GameObject("TempPatch");
            HelmPatch patch = temp.AddComponent<HelmPatch>();
            patch.patchData = patchData;

            currentPatch = patch;
            helmController.LoadPatch(patch);
            Destroy(temp);

            PlayerPrefs.SetString("HelmFolder", folderDropdown.options[folderDropdown.value].text);
            PlayerPrefs.SetString("HelmPatch", patchDropdown.options[index].text);
        }

        void SyncDropdownsToSavedPatch(string folderName, string patchName)
        {
            for (int i = 0; i < folderDropdown.options.Count; i++)
            {
                if (folderDropdown.options[i].text == folderName)
                {
                    folderDropdown.SetValueWithoutNotify(i);
                    PopulatePatches(i);
                    break;
                }
            }

            for (int i = 0; i < patchDropdown.options.Count; i++)
            {
                if (patchDropdown.options[i].text == patchName)
                {
                    patchDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }
        }

        void ResizeDropdownToFit(TMP_Dropdown dropdown)
        {
            float maxWidth = 0f;
            TMP_Text label = dropdown.captionText;

            foreach (var option in dropdown.options)
            {
                float width = label.GetPreferredValues(option.text).x;
                if (width > maxWidth)
                    maxWidth = width;
            }

            maxWidth += 50f;

            RectTransform rt = dropdown.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
    }
}