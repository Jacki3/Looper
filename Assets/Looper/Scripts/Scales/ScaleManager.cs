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

    private void Start()
    {
        SetScaleText();
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
}
