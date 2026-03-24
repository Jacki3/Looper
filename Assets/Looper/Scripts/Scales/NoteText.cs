using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteText : MonoBehaviour
{
    private Color defaultTextColour;

    public TextMeshProUGUI letter;
    public Button button;
    public int noteIndex;

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
        defaultTextColour = letter.color;
        UpdateColours();
    }

    private void UpdateColours()
    {
        return;

        if (ColourCycler.currentColour == Color.black)
        {
            letter.color = Color.white;
        }
        else
        {
            letter.color = defaultTextColour;
        }
    }
}
