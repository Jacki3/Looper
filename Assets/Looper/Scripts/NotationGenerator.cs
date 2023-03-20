using System.Collections.Generic;
using UnityEngine;

//TODO:
//finish off the generator based on octave ranges
//get the audio helm new samples - plug in midi and find errors with playing specific notes in octave ranges
//figure out how to generate patterns both in scales and outside of?
//build luke player looper then UI in a new scene (with the updated keyboard graphic + metronome should be here too)
//begin to add notation images you could also update the text to reflect the exact note
//have patterns and note choices be visual (choose by keyboard for all notes and choose by name for patterns)
//polish up and try to tidy up the play in any scale logic
//this is called scale generator 
public class NotationGenerator : MonoBehaviour
{
    public int[] notes;
    public bool useScale;
    public ScaleNames scaleChoice;
    public RootNotes rootNote;
    public int octavesAbove = 5;
    //so far we generate a scale within a given root note however we can choose notes from this octave and ones higher if required (e.g., middle C major goes from middle C up to middle B but this could be extended to higher octaves)
    public bool spanMultipleOctaves;
    //how many higher octaves can we choose from?
    public int octaveRange;
    public int totalMIDIKeys = 88;

    public List<Scale> scales = new List<Scale>();

    private static int[] noteList;

    public delegate void ChangeScaleEvent();

    public static event ChangeScaleEvent UpdateScale;

    [System.Serializable]
    public class Scale
    {
        public string name;
        public ScaleNames scaleEnum;
        public string description;
        public int[] intervals;
    }

    public enum ScaleNames
    {
        Aeolian, Major, Hirajōshi, Minyō, Chromatic, Algerian, Altered, Bayātī, Blues, Dorian, HarmonicMinor, Hungarian, Insen, Iwato, Korsakovian, Lydian, MinorPentatonic, Pentatonic, Persian, Romanian, TriadMajor, TriadMinor, Tuvan, WholeTone,
    };

    public enum RootNotes
    {
        C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    };

    private void Awake()
    {
        if (useScale)
            noteList = GetScaleFromRoot();
        else

            noteList = notes;
    }

    public static int[] GenerateNotes(int length)
    {
        int[] newPhrase = new int[length];

        for (int i = 0; i < length; i++)
        {
            int randNote = noteList[Random.Range(0, noteList.Length)];
            newPhrase[i] = randNote;
        }
        return newPhrase;
    }

    public int[] GetScaleFromRoot()
    {
        foreach (Scale scale in scales)
        {
            if (scale.scaleEnum == scaleChoice)
            {
                int[] rootScale = (int[])scale.intervals.Clone(); //this will need to be added by 12 by however many octaves we go above (e.g., 0, 2, 4, 5, 7, 9, 11 would become: 0, 2, 4, 5, 7, 9, 11, 12, 14, 16, 17, 21, 23) -- could you have this for a full 88 keybed (7 octaves) -- if note is below a certain range, then bass clef must be used or crazy ledger lines used
                int rootNoteNumber = (int)rootNote;
                int octaveMultiplier = 12 * octavesAbove;
                for (int i = 0; i < rootScale.Length; i++)
                {
                    rootScale[i] += rootNoteNumber + octaveMultiplier;
                }
                return rootScale;
            }
        }
        Debug.LogError("No matching scale to scale choice: select a new scale or add the chosen one to the list!");
        return null;
    }

    public int[] CurrentScale()
    {
        foreach (Scale scale in scales)
            if (scale.scaleEnum == scaleChoice)
                return scale.intervals;

        Debug.LogError("No matching scale to scale choice: select a new scale or add the chosen one to the list!");
        return null;
    }

    //this is what needs to be more intelligent - it has a pattern but we are quite using it right by creating lots of them
    public int[] GetMIDINotes()
    {
        var scale = CurrentScale();
        int index = 0;
        int modifier = 0;
        int[] MIDINotes = new int[totalMIDIKeys];

        //for all the notes on the keybed generate a scale from the first note and then repeat once you have completed the scale
        for (int i = 0; i < totalMIDIKeys; i++)
        {
            if (index >= scale.Length)
            {
                //once the scale has been filled in, reset it and then add an octave to the next series (+12)
                index = 0;
                modifier += 12;
            }
            MIDINotes[i] = scale[index] + modifier;
            index++;
        }

        return MIDINotes;
    }

    public RootNotes UpdateRoot(int index)
    {
        rootNote = (RootNotes)index;
        return rootNote;
    }

    public ScaleNames UpdateScaleChoice(int index)
    {
        scaleChoice = (ScaleNames)index;
        UpdateScale();
        return scaleChoice;
    }

    public string GetScaleDesc()
    {
        foreach (Scale scale in scales)
            if (scale.scaleEnum == scaleChoice)
                return scale.description;

        Debug.LogError("No matching scale to scale choice: select a new scale or add the chosen one to the list!");
        return null;
    }

    public string GetScaleName()
    {
        foreach (Scale scale in scales)
            if (scale.scaleEnum == scaleChoice)
                return scale.name;

        Debug.LogError("No matching scale to scale choice: select a new scale or add the chosen one to the list!");
        return null;
    }
}