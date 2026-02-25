using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Key : ClickDetector
{
    public int note;
    public Button button;
    public NotationGenerator notationGenerator;
    private int octaveShift;

    public delegate void NoteOnEventHandler(int note, float vel);

    public static event NoteOnEventHandler NoteOn;
    public delegate void NoteOffEventHandler(int note);

    public static event NoteOffEventHandler NoteOff;

    static HashSet<int> activeNotes = new HashSet<int>();

    void TriggerNoteOn(int noteToPlay)
    {
        if (!activeNotes.Contains(noteToPlay))
        {
            activeNotes.Add(noteToPlay);
            NoteOn?.Invoke(noteToPlay, 1f);
        }
    }

    void TriggerNoteOff(int noteToStop)
    {
        if (activeNotes.Contains(noteToStop))
        {
            activeNotes.Remove(noteToStop);
            NoteOff?.Invoke(noteToStop);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //click goes here
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        TriggerNoteOn(note);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // Turn off ALL active notes when finger/mouse is released
        foreach (int activeNote in new HashSet<int>(activeNotes))
        {
            TriggerNoteOff(activeNote);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            TriggerNoteOn(note);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            TriggerNoteOff(note);
        }
    }
}
