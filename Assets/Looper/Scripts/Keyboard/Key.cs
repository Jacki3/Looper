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

    public override void OnPointerClick(PointerEventData eventData)
    {
        //click goes here
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        NoteOn(note, 1f);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        NoteOff(note);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            NoteOn(note, 1f);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        NoteOff(note);
    }
}
