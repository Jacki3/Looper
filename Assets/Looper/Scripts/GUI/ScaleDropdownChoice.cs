using System.Linq;
using TMPro;
using UnityEngine;

public class ScaleDropdownChoice : MonoBehaviour
{
    public TMP_Dropdown scaleDropdown;
    public ScaleManager scaleManager;

    private void Start()
    {
        scaleDropdown?.onValueChanged.AddListener(OnScaleChanged);
        PopulateScales();
    }

    private void PopulateScales()
    {
        if (!scaleManager) return;

        scaleDropdown?.ClearOptions();
        scaleDropdown?.AddOptions(scaleManager.notationGenerator.scales.Select(s => s.name).ToList());

        UpdateScaleChoice();
    }

    private void UpdateScaleChoice()
    {
        scaleDropdown.value = scaleManager.notationGenerator.GetScaleAtIndex();
    }

    private void OnScaleChanged(int index)
    {
        string selectedText = scaleDropdown.options[index].text;

        scaleManager?.ChangeScale(index);
    }
}