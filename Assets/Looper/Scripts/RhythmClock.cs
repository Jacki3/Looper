using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(AudioSource))]
public class RhythmClock : MonoBehaviour
{
    [Header("Settings")]
    public float BPM;
    public float rhythmAllowance = .25f;
    //the number of beats in each loop
    public float beatsPerLoop;
    public bool muteMetronome = false;
    public AudioClip metronomeTick;
    public float secPerBeat;
    public float songPos;
    public float dspSongTime;

    [Header("Info")]
    //the total number of loops completed since the looping clip first started
    public int completedLoops = 0;
    //The current position of the song within the loop in beats.
    public float loopPositionInBeats;
    //Current song position, in beats
    public float songPositionInBeats;
    //The current relative position of the song within the loop measured between 0 and 1.
    public float loopPositionInAnalog;
    public bool onBeat;

    public GameObject[] beatBoxes;
    public Transform rhythmBar;

    //Conductor instance
    public static RhythmClock i;

    private AudioSource audioSource;

    private void Awake()
    {
        i = this;
    }

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        //Calculate the number of seconds in each beat
        secPerBeat = 60f / BPM;

        rhythmAllowance = secPerBeat / 4;

        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;
    }

    void Update()
    {
        //determine how many seconds since the song started
        songPos = (float)(AudioSettings.dspTime - dspSongTime);

        //determine how many beats since the song started
        songPositionInBeats = songPos / secPerBeat;

        loopPositionInAnalog = loopPositionInBeats / beatsPerLoop;

        //calculate the loop position
        if (songPositionInBeats >= (completedLoops + 1) * beatsPerLoop)
        {
            completedLoops++;
            TickClock();
            StartCoroutine(RhythmPause());
        }

        if (songPositionInBeats >= (completedLoops + 1) * beatsPerLoop - rhythmAllowance)
            onBeat = true;

        loopPositionInBeats = songPositionInBeats - completedLoops * beatsPerLoop;

        audioSource.mute = muteMetronome;
    }

    private void TickClock()
    {
        audioSource?.PlayOneShot(metronomeTick);
        foreach (GameObject beatBox in beatBoxes)
        {
            GameObject beatBoxObj = Instantiate(beatBox, rhythmBar);
        }
    }

    private IEnumerator RhythmPause()
    {
        yield return new WaitForSeconds(rhythmAllowance);
        onBeat = false;
    }

    public void MuteMetronome()
    {
        if (muteMetronome)
            muteMetronome = false;
        else
            muteMetronome = true;
    }
}