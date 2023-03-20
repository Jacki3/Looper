using System;
using System.Collections.Generic;

using UnityEngine;
public class NoteCircle : MonoBehaviour
{
    public NotationGenerator notationGenerator;
    public float totalNotes = 12;
    public string[] noteLetters;
    public NoteLine noteLine;
    public Color rootColor;
    public NoteText noteText;
    public List<NoteLine> currentLines = new List<NoteLine>();
    public List<NoteText> currentTexts = new List<NoteText>();

    private float angle;

    private void OnEnable()
    {
        NotationGenerator.UpdateScale += CreateLines;
    }

    private void OnDisable()
    {
        NotationGenerator.UpdateScale -= CreateLines;
    }

    void Start()
    {
        angle = 360 / totalNotes;
        CreateLines();
        CreateLetters();
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
            NoteText newText = Instantiate(noteText, transform.root);
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
            if (currentLines.Count <= 1)
                newLine.line.color = rootColor;
        }
    }

    public void UpdateRoot(int index)
    {
        notationGenerator.UpdateRoot(index);
        transform.rotation = Quaternion.Euler(0, 0, ((int)notationGenerator.rootNote * angle) * -1);
    }
}
