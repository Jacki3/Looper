using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        [Header("Settings")]
        public string presetsPath = "HelmPatches";

        [Header("Default Patch (matches Editor selection)")]
        public string defaultFolder = "Keys";
        public string defaultPatch = "Piano";

        string basePath;
        List<string> folderPaths = new List<string>();
        List<string> patchPaths = new List<string>();

        void Start()
        {
            basePath = Path.Combine(Application.streamingAssetsPath, presetsPath);

            folderDropdown.onValueChanged.AddListener(OnFolderChanged);
            patchDropdown.onValueChanged.AddListener(OnPatchChanged);

            PopulateFolders();

            string folder = PlayerPrefs.GetString("HelmFolder", defaultFolder);
            string patch = PlayerPrefs.GetString("HelmPatch", defaultPatch);
            SyncDropdownsToSavedPatch(folder, patch);

            // Apply patch after native plugin has initialized
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
            folderPaths.Clear();

            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"Presets folder not found: {basePath}");
                return;
            }

            string[] directories = Directory.GetDirectories(basePath);
            List<string> folderNames = new List<string>();

            foreach (string dir in directories)
            {
                folderPaths.Add(dir);
                folderNames.Add(Path.GetFileName(dir));
            }

            folderDropdown.AddOptions(folderNames);
            ResizeDropdownToFit(folderDropdown);

            if (folderPaths.Count > 0)
                PopulatePatches(0);
        }

        void PopulatePatches(int folderIndex)
        {
            patchDropdown.ClearOptions();
            patchPaths.Clear();

            string[] files = Directory.GetFiles(folderPaths[folderIndex], "*.helm");
            List<string> patchNames = new List<string>();

            foreach (string file in files)
            {
                patchPaths.Add(file);
                patchNames.Add(Path.GetFileNameWithoutExtension(file));
            }

            patchDropdown.AddOptions(patchNames);
            ResizeDropdownToFit(patchDropdown);
        }

        void OnFolderChanged(int index)
        {
            PopulatePatches(index);

            if (patchPaths.Count > 0)
                OnPatchChanged(0);
        }

        void OnPatchChanged(int index)
        {
            if (index < 0 || index >= patchPaths.Count)
                return;

            string json = File.ReadAllText(patchPaths[index]);
            HelmPatchFormat patchData = JsonUtility.FromJson<HelmPatchFormat>(json);
            ApplyPatch(patchData);

            // Save the selection
            PlayerPrefs.SetString("HelmFolder", folderDropdown.options[folderDropdown.value].text);
            PlayerPrefs.SetString("HelmPatch", patchDropdown.options[index].text);
        }

        void ApplyPatch(HelmPatchFormat patch)
        {
            int channel = helmController.channel;

            Native.HelmClearModulations(channel);

            FieldInfo[] fields = typeof(HelmPatchSettings).GetFields();
            int paramIndex = 1;

            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsArray && !field.IsLiteral)
                {
                    float val = (float)field.GetValue(patch.settings);
                    Native.HelmSetParameterValue(channel, paramIndex, val);
                    paramIndex++;
                }
            }

            if (patch.settings.modulations != null)
            {
                int modIndex = 0;
                foreach (HelmModulationSetting mod in patch.settings.modulations)
                {
                    if (modIndex >= HelmPatchSettings.kMaxModulations)
                        break;

                    Native.HelmAddModulation(
                        channel, modIndex,
                        mod.source,
                        mod.destination,
                        mod.amount
                    );
                    modIndex++;
                }
            }
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
                    OnPatchChanged(i);
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

            // Add padding for the dropdown arrow and margins
            maxWidth += 50f;

            RectTransform rt = dropdown.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
    }
}