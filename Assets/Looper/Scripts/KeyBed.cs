using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class KeyBed : MonoBehaviour
{
    public int startNote = 60;
    public int totalOctavesToSpawn = 1;
    public int minOctaves = 1;
    public int maxOctaves = 4;
    public NotationGenerator notationGenerator;
    public GameObject keyLine;
    public Key key;
    private float screenWidth;
    private float lineWidth;
    private int totalButtons;
    private float dist;
    private List<GameObject> noteObjs = new List<GameObject>();
    private List<Key> keys = new List<Key>();

    private void OnEnable()
    {
        NotationGenerator.UpdateScale += UpdateScale;
        MIDIInputManager.NoteOn += HighlightNote;
    }

    private void OnDisable()
    {
        NotationGenerator.UpdateScale -= UpdateScale;
        MIDIInputManager.NoteOn -= HighlightNote;
    }

    private void Start()
    {
        //the width of the key line
        lineWidth = keyLine.transform.localScale.x;
        //screen width is the size of the UI canvas minus the line width (*100 to get back to world space)
        screenWidth = transform.root.GetComponent<CanvasScaler>().referenceResolution.x - lineWidth * 100;
        UpdateScale();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
            UpdateOctaveCount(false);
        if (Input.GetKeyUp(KeyCode.DownArrow))
            UpdateOctaveCount(true);
    }

    public void UpdateOctaveCount(bool minus)
    {
        if (minus)
        {
            if (totalOctavesToSpawn - 1 >= 1)
            {
                totalOctavesToSpawn--;
                UpdateScale();
            }
            else
                Debug.Log("already at min octaves");
        }
        else
        {
            if (totalOctavesToSpawn + 1 <= maxOctaves)
            {
                totalOctavesToSpawn++;
                UpdateScale();
            }
            else
                Debug.Log("already at max octaves");
        }
    }

    private void UpdateScale()
    {
        //before spawning anything, delete everything that is already spawned
        foreach (GameObject noteObj in noteObjs)
            Destroy(noteObj);
        noteObjs.Clear();

        totalButtons = notationGenerator.GetScaleFromRoot().Length * totalOctavesToSpawn + 1;
        dist = screenWidth / totalButtons;
        SpawnButtons();
        SpawnLines();
    }



    private void SpawnButtons()
    {
        keys.Clear();
        //the size of the buttons is always equal to the distance between lines (/100 to return to local space)
        float sizeX = dist / 100;
        for (int i = 0; i < totalButtons; i++)
        {
            Key newButton = Instantiate(key, transform);
            if (i > 0)
            {
                //the first button is always at 0 so only the ones above will change pos
                newButton.transform.localPosition = new Vector3(dist * i, 0, 0);
            }

            //divide the index by the length of the scale - if this is 0 we are on the root note so show this with a different colour 
            if (i % notationGenerator.GetScaleFromRoot().Length == 0)
            {
                Button b = newButton.button;
                ColorBlock cb = b.colors;
                cb.normalColor = Color.black;
                cb.selectedColor = Color.black;
                b.colors = cb;
            }

            var tempX = newButton.transform.localScale;
            tempX.x = sizeX;
            newButton.transform.localScale = tempX;

            newButton.note = startNote + i;
            newButton.notationGenerator = notationGenerator;

            noteObjs.Add(newButton.gameObject);
            keys.Add(newButton);
        }
    }

    private void SpawnLines()
    {
        //this is +1 as we want to spawn the end line to complete the whole keyboard
        for (int i = 0; i < totalButtons + 1; i++)
        {
            GameObject newLine = Instantiate(keyLine, transform);

            if (i > 0)
            {
                //as with the buttons, anything above 0 will be moved
                newLine.transform.localPosition = new Vector3(dist * i, 0, 0);
            }
            noteObjs.Add(newLine);
        }
    }

    private void HighlightNote(int note, float vel)
    {
        //if a button exisits in the list between the start note and total buttons spawned we can highlight it
        if (note - startNote <= keys.Count - 1 && note >= 60)
            StartCoroutine(FadeButton(note));
    }
    IEnumerator FadeButton(int note)
    {
        Button b = keys[note - 60].button;
        b.image.CrossFadeColor(b.colors.pressedColor, b.colors.fadeDuration, true, true);
        yield return new WaitForSeconds(b.colors.fadeDuration);
        b.image.CrossFadeColor(b.colors.normalColor, b.colors.fadeDuration, true, true);
    }
}
