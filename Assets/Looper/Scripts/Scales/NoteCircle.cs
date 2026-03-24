using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NoteCircle : MonoBehaviour
{
    public NotationGenerator notationGenerator;
    public float totalNotes = 12;
    public string[] noteLetters;
    public NoteLine noteLine;
    public Color rootColor;
    public NoteText noteText;
    public Transform noteLetterParent;
    public Sprite activeLetterSprite;
    public List<NoteLine> currentLines = new List<NoteLine>();
    public List<NoteText> currentTexts = new List<NoteText>();

    private float angle;
    private Sprite defaultLetterSprite;

    private void OnEnable()
    {
        NotationGenerator.UpdateScale += CreateLines;
        NotationGenerator.UpdateScale += UpdateScaleLetters;
    }

    private void OnDisable()
    {
        NotationGenerator.UpdateScale -= CreateLines;
        NotationGenerator.UpdateScale -= UpdateScaleLetters;
    }

    void Start()
    {
        defaultLetterSprite = noteText.button.GetComponent<Image>().sprite;

        angle = 360 / totalNotes;
        CreateLines();
        CreateLetters();
        UpdateRoot(0);
    }

    private void Update()
    {
        foreach (char letter in Input.inputString)
        {
            if (Char.IsDigit(letter))
            {
                //convert our char to an int then pass this into the method
                UpdateRoot(letter - '0');
            }
        }
    }

    public void CreateLetters()
    {
        foreach (NoteText text in currentTexts)
            Destroy(text.gameObject);
        currentTexts.Clear();

        for (int i = 0; i < noteLetters.Length; i++)
        {
            Transform parent = transform.root;
            if (noteLetterParent != null)
                parent = noteLetterParent;

            NoteText newText = Instantiate(noteText, parent);
            newText.transform.rotation = Quaternion.Euler(0, 0, (angle * i) * -1);
            //ensure the text rotation is the opposite of the object it is attached to ensure it appears upright
            newText.letter.rectTransform.localRotation = Quaternion.Euler(0, 0, angle * i);
            newText.letter.text = noteLetters[i];
            newText.noteIndex = i;
            newText.button.onClick.AddListener(() => UpdateRoot(newText.noteIndex));

            currentTexts.Add(newText);
        }
    }

    public void CreateLines()
    {
        foreach (NoteLine line in currentLines)
            Destroy(line.gameObject);
        currentLines.Clear();

        int[] notes = notationGenerator.GetScaleFromRoot();
        foreach (int note in notes)
        {
            NoteLine newLine = Instantiate(noteLine, transform);
            newLine.transform.rotation = Quaternion.Euler(0, 0, (angle * note) * -1);
            currentLines.Add(newLine);
            Color currentColour = ColourCycler.currentColour;
            if (currentColour == Color.white)
                currentColour = Color.black;

            newLine.line.color = currentColour;
            if (currentLines.Count <= 1)
                newLine.line.color = rootColor;
        }
    }

    public void UpdateRoot(int index)
    {
        notationGenerator.UpdateRoot(index);
        transform.rotation = Quaternion.Euler(0, 0, ((int)notationGenerator.rootNote * angle) * -1);

        UpdateScaleLetters();
    }

    public void UpdateScaleLetters()
    {
        if (!activeLetterSprite) return;

        foreach (NoteText text in currentTexts)
        {
            text.letter.color = Color.white;
            Image letterCircle = text.button.GetComponent<Image>();
            letterCircle.sprite = defaultLetterSprite;
            letterCircle.color = Color.black;
        }

        int totalNotesInScale = notationGenerator.GetScaleFromRoot().Length;

        for (int i = 0; i < totalNotesInScale; i++)
        {
            int index = notationGenerator.GetScaleFromRoot()[i] % 12;
            Color currentColour = ColourCycler.currentColour;
            if (currentColour == Color.white)
                currentColour = Color.black;

            currentTexts[index].letter.color = currentColour;
            Image letterCircle = currentTexts[index].button.GetComponent<Image>();
            if (!letterCircle)
                continue;

            letterCircle.sprite = activeLetterSprite;
            letterCircle.color = Color.white;
        }

        // get all the texts based on the scale
        // update the sprite
        // update the text colour via colour cycler - if its white then make it black
    }
}
