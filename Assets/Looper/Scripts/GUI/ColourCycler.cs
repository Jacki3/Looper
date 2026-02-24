using UnityEngine;
using UnityEngine.UI;

public class ColourCycler : MonoBehaviour
{
    [SerializeField] private Image targetImage;

    private int currentIndex = 0;

    private static readonly Color[] colours = new Color[]
    {
        HexToColor("FF6978"), // Pastel Red/Pink (default)
        HexToColor("FFFFFF"),
        HexToColor("000000"),
        HexToColor("FFB347"), // Pastel Orange
        HexToColor("77DD77"), // Pastel Green
        HexToColor("89CFF0"), // Pastel Blue
        HexToColor("B39EB5"), // Pastel Purple
        HexToColor("FF6961"), // Pastel Red
        HexToColor("FFB7CE"), // Pastel Pink
        HexToColor("99C5C4"), // Pastel Teal
        HexToColor("CB99C9"), // Pastel Violet
        HexToColor("FFDAB9"), // Peach
        HexToColor("A0D6B4"), // Pastel Mint
        HexToColor("B4D8E7"), // Pastel Sky
        HexToColor("F4BFBF"), // Pastel Rose
        HexToColor("AAD8B0"), // Pastel Sage
        HexToColor("C3B1E1"), // Pastel Lavender
        HexToColor("F8D568"), // Pastel Gold
        HexToColor("85E3FF"), // Pastel Cyan
        HexToColor("E8A0BF"), // Pastel Magenta
        HexToColor("D4A373"), // Pastel Caramel
        HexToColor("BFCBA8"), // Pastel Olive
        HexToColor("A8D8EA"), // Pastel Powder Blue
    };

    public static Color currentColour;

    private void Awake()
    {
        if (targetImage != null)
            targetImage.color = colours[0];

        currentColour = colours[currentIndex];
    }

    public void CycleColour()
    {
        currentIndex = (currentIndex + 1) % colours.Length;
        if (targetImage != null)
            targetImage.color = colours[currentIndex];

        currentColour = colours[currentIndex];

        NotationGenerator.TriggerUpdateScale();
    }

    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}