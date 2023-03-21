using UnityEngine;
using UnityEngine.InputSystem;
using AudioHelm;
using System;
using System.Collections.Generic;

public class MIDIInputManager : MonoBehaviour
{
    public bool debugMode;
    public NotationGenerator notationGenerator;

    public delegate void NoteOnEventHandler(int note, float velocity);

    public static event NoteOnEventHandler NoteOn;

    public delegate void NoteOffEventHandler(int note);

    public static event NoteOffEventHandler NoteOff;
    private int[] MIDINotes;
    private int octaveShift;
    private List<char> keysDown = new List<char>();

    private void OnEnable()
    {
        NotationGenerator.UpdateScale += GenerateScale;
    }

    private void OnDisable()
    {
        NotationGenerator.UpdateScale -= GenerateScale;
    }

    private void Start()
    {
        GenerateScale();

        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                // Note that you can't use note.velocity because the state
                // hasn't been updated yet (as this is "will" event). The note
                // object is only useful to specify the target note (note
                // number, channel number, device name, etc.) Use the velocity
                // argument as an input note velocity.
                // Debug.Log(string.Format(
                //     "Note On #{0} ({1}) vel:{2:0.00} ch:{3} dev:'{4}'",
                //     note.noteNumber,
                //     note.shortDisplayName,
                //     velocity,
                //     (note.device as Minis.MidiDevice)?.channel,
                //     note.device.description.product
                // ));

                // if (CharacterGridMovement.readingMode)
                // {
                //     RhythmManager.i.HitBeat();
                //     NotationManager.i.PlayNote(note.noteNumber);
                //     helmController.NoteOn(noteToPlay);
                // }

                // int noteToPlay = notationGenerator.useScale ? MIDINotes[(note.noteNumber - octaveShift) + (int)notationGenerator.rootNote] : note.noteNumber;
                NoteOn(note.noteNumber, note.velocity);
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                // Debug.Log(string.Format(
                //     "Note Off #{0} ({1}) ch:{2} dev:'{3}'",
                //                                             note.noteNumber,
                //                                             note.shortDisplayName,
                //                                             (note.device as Minis.MidiDevice)?.channel,
                //                                             note.device.description.product
                // ));

                // int noteToPlay = notationGenerator.useScale ? MIDINotes[(note.noteNumber - octaveShift) + (int)notationGenerator.rootNote] : note.noteNumber;
                NoteOff(note.noteNumber);
            };
        };
    }

    private void GenerateScale()
    {
        MIDINotes = notationGenerator.GetMIDINotes();
        //as we start from middle c (this could be changed if you like) we find what note is played when middle c is pressed and then make up the difference to ensure we are in the right octave
        int middleC = System.Array.IndexOf(MIDINotes, 60);
        octaveShift = 60 - middleC;
    }

    void Update()
    {
        //playing with keyboard is a little hacky but works for now - mostly because some keys get stuck and we are hardcoding a the relative midis
        if (debugMode)
        {
            if (Input.anyKeyDown)
            {
                if (Input.inputString != "")
                {
                    int note = KeyboardBinding.GetKeyFromString(Input.inputString.ToCharArray()[0]);
                    if (note >= 0)
                    {
                        NoteOn(note, 1f);
                        keysDown.Add(Input.inputString.ToCharArray()[0]);
                    }
                }
            }

            foreach (char key in keysDown)
            {
                if (Input.GetKeyUp(key.ToString()))
                {
                    int note = KeyboardBinding.GetKeyFromString(key);
                    NoteOff(note);
                }
            }
        }
        // NotationManager.i.PlayNote(letter - '0' + 60);
        // RhythmManager.i.HitBeat();
    }
}


