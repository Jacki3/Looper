using AudioHelm;
using MoreMountains.Feedbacks;
using UnityEngine;

[RequireComponent(typeof(HelmController))]
public class Keyboard : MonoBehaviour
{
    public int startNote = 60; //this is found twice (in keybed also so we should create a global var instead)
    public HelmController helm;
    public NotationGenerator notationGenerator;
    public MMFeedbacks feedbacks;
    public bool playInScale;
    private int[] MIDINotes;
    private int octaveShift;
    public delegate void PlayMIDINoteHandler(int note, float velocity);

    public static event PlayMIDINoteHandler MIDIPlayed;

    private void OnEnable()
    {
        MIDIInputManager.NoteOn += PlaySound;
        MIDIInputManager.NoteOff += SoundOff;
        NotationGenerator.UpdateScale += GenerateScale;
        Key.NoteOn += PlaySound;
        Key.NoteOff += SoundOff;
    }

    private void OnDisable()
    {
        MIDIInputManager.NoteOn -= PlaySound;
        MIDIInputManager.NoteOff -= SoundOff;
        NotationGenerator.UpdateScale -= GenerateScale;
        Key.NoteOn -= PlaySound;
        Key.NoteOff -= SoundOff;
    }

    private void Start()
    {
        helm = GetComponent<HelmController>();
        GenerateScale();
    }

    private void PlaySound(int note, float vel)
    {
        int noteToPlay = playInScale ? MIDINotes[(note - octaveShift) + (int)notationGenerator.rootNote] : note;
        helm.NoteOn(noteToPlay);
        MIDIPlayed(noteToPlay, vel);
        feedbacks?.PlayFeedbacks();
    }

    private void SoundOff(int note)
    {
        int noteToPlay = playInScale ? MIDINotes[(note - octaveShift) + (int)notationGenerator.rootNote] : note;
        helm.NoteOff(noteToPlay);
    }
    private void GenerateScale()
    {
        MIDINotes = notationGenerator.GetMIDINotes();
        //as we start from middle c (this could be changed if you like) we find what note is played when middle c is pressed and then make up the difference to ensure we are in the right octave
        int middleC = System.Array.IndexOf(MIDINotes, startNote);
        octaveShift = startNote - middleC;
    }
}
