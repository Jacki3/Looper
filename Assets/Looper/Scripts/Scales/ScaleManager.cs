using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Handles the UI for the scale text and adds functionality to the arrow buttons
public class ScaleManager : MonoBehaviour
{
    public NotationGenerator notationGenerator;
    public TextMeshProUGUI scaleText;
    public Text scaleCardText;
    public Text scaleDescText;
    public Image colourPaletteIcon;
    public Image gearIcon;
    public Image exitIcon;
    public Color darkArrowColour;
    public Color lightArrowColour;
    public Image[] arrows;

    private void OnEnable()
    {
        NotationGenerator.UpdateScale += UpdateColours;
    }

    private void OnDisable()
    {
        NotationGenerator.UpdateScale -= UpdateColours;
    }

    private void Start()
    {
        SetScaleText();
        UpdateColours();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
            ChangeScale(false);
        if (Input.GetKeyUp(KeyCode.LeftArrow))
            ChangeScale(true);
    }

    public void ChangeScale(bool left)
    {
        int currentIndex = (int)notationGenerator.scaleChoice;
        int totalScales = notationGenerator.scales.Count - 1;

        if (!left)
        {
            if (currentIndex >= totalScales)
                currentIndex = 0;
            else
                currentIndex++;
        }
        else
        {
            if (currentIndex <= 0)
                currentIndex = totalScales;
            else
                currentIndex--;
        }
        notationGenerator.UpdateScaleChoice(currentIndex);
        SetScaleText();
    }

    private void SetScaleText()
    {
        string scaleName = notationGenerator.GetScaleName();
        string scaleDesc = notationGenerator.GetScaleDesc();

        scaleText.text = scaleName;
        scaleCardText.text = scaleName;
        scaleDescText.text = scaleDesc;
    }

    private void UpdateColours()
    {
        Color currentColour = lightArrowColour;
        if (ColourCycler.currentColour != Color.black)
        {
            currentColour = darkArrowColour;
        }

        if (colourPaletteIcon != null)
            colourPaletteIcon.color = currentColour;

        if (scaleText != null)
            scaleText.color = currentColour;

        if (gearIcon != null)
            gearIcon.color = currentColour;

        if (exitIcon != null)
            exitIcon.color = currentColour;

        foreach (var arrow in arrows)
        {
            arrow.color = currentColour;
        }
    }
}
