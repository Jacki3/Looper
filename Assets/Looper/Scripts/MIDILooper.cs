using System;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;
using System.Linq;

public class MIDILooper : MonoBehaviour
{
    public static bool recOutput;
    public static bool recDub;
    public HelmController helmController;
    public List<Recording> MIDIRecordings = new List<Recording>();
    private Recording recorder;

    [Serializable]
    public class Recording
    {
        //the notes that have been recorded for this recording
        public List<Notes> notes = new List<Notes>();
        public HelmController helmController;
        //informs the midi note on what time it was played
        public double songPos;
        //if you are overdubbing, this will be how long the first loop created was; once we reach this, time get resets to 0
        public double loopLength;
        public bool loopComplete;
        //this keeps track of the audio time since the loop began and is used when a midi note is played to keep track of what time it was played during the loop
        public double songTime;
        public MIDILooper looper;
        public IEnumerator PlayNotes;

        public IEnumerator PlayMIDINotes()
        {
            while (true)
            {
                //if we are recording a different loop, wait until it is stopped reocrding and then wait until all loops are complete
                if (looper.IsRecording(this))
                {
                    yield return null;
                }
                else
                {
                    //sort the list by when each note was played
                    List<Notes> SortedList = notes.OrderBy(o => o.timeNotePlayed).ToList();

                    for (int i = 0; i < SortedList.Count; i++)
                    {
                        //wait either for the first note or if the previous note depending on where we are in the loop
                        float waitTime = (float)SortedList[i].timeNotePlayed;
                        if (i > 0)
                            waitTime = (float)SortedList[i].timeNotePlayed - (float)SortedList[i - 1].timeNotePlayed;

                        yield return new WaitForSeconds(waitTime);
                        //here we have hardcoded the velocity and how long the note was played for - velocity is easy to get but note duration will require listening for note off rather than on in MIDI
                        helmController.NoteOn(SortedList[i].noteNumber, 1f, 0.5f);
                    }
                    if (this.loopLength > looper.LongestLoop())
                    {
                        //no need to wait as this is the longest loop                    
                    }
                    else
                    {
                        //wait for the longest loop to finish
                        yield return new WaitForSeconds((float)looper.LongestLoop() - (float)this.loopLength + 1f);
                    }
                }
            }
        }

        public void StartSongTime()
        {
            songTime = AudioSettings.dspTime;
        }

        public IEnumerator StartTime()
        {
            while (true)
            {
                songPos = AudioSettings.dspTime - songTime;
                //if we have recorded a loop (we are overdubbing) then reset the song time back to start once it completes (otherwise notes recorded will be played outside of the loop)
                if (loopComplete && songPos >= loopLength)
                    songTime = AudioSettings.dspTime;

                yield return null;
            }
        }
        public void RecordLoop()
        {
            loopLength = notes[notes.Count - 1].timeNotePlayed;
            StartSongTime();
        }
    }

    [Serializable]
    public class Notes
    {
        //what note was played?
        public int noteNumber;
        //how long was it held for? -- needs to recorded using get time on was played then track when off was played
        public float noteLength;
        //how hard was it pressed?
        public float noteVelocity;
        //the difference in time from when recording started to when this note was played
        public double timeNotePlayed;
    }

    private void OnEnable()
    {
        Keyboard.MIDIPlayed += RecordMIDINote;
    }

    private void OnDisable()
    {
        Keyboard.MIDIPlayed -= RecordMIDINote;
    }

    private void Update()
    {
        //Debug settings for keyboard controls for looper
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     RecordLoop();
        // }

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     Overdub();
        // }
    }

    public void RecordLoop()
    {
        if (!recOutput)
        {
            //recording a brand new loop
            recOutput = true;
            //create a new instance of the recording class
            recorder = new Recording();
            recorder.StartSongTime();
            recorder.looper = this;
            StartCoroutine(recorder.StartTime());
        }
        else
        {
            recOutput = false;
            //if the recording has some midi notes recorded
            if (recorder.notes.Count > 0)
            {
                if (!recorder.loopComplete)
                {
                    recorder.loopComplete = true;
                    recorder.RecordLoop();
                    MIDIRecordings.Add(recorder);
                    recorder.helmController = helmController;
                    recorder.PlayNotes = recorder.PlayMIDINotes();
                    StartCoroutine(recorder.PlayNotes);
                }
            }
            else
                recorder = null;
        }
    }

    public void Overdub()
    {
        //if there is a current recording to overdub on (this will be the most recent loop)
        if (recorder != null)
        {
            if (!recOutput)
            {
                recOutput = true;
                recDub = true;
            }
            else
            {
                recOutput = false;
                recDub = false;
            }
        }
        else
        {
            Debug.Log("No loop to overdub on!");
        }
    }

    private void RecordMIDINote(int note, float velocity)
    {
        if (recOutput)
        {
            Notes newNote = new Notes();
            newNote.noteNumber = note;
            newNote.noteVelocity = velocity;
            newNote.timeNotePlayed = recorder.songPos;
            recorder?.notes.Add(newNote);
        }
    }

    public void StopPlaying()
    {
        foreach (Recording recording in MIDIRecordings)
        {
            StopCoroutine(recording.PlayNotes);
        }
    }

    public void StartPlaying()
    {
        foreach (Recording recording in MIDIRecordings)
        {
            StartCoroutine(recording.PlayNotes);
        }
    }

    public void RemovePriorLoop()
    {
        //if there is a loop to record
        if (MIDIRecordings.Count >= 1)
        {
            //get the previous loop (or the only one) and stop playing sounds
            Recording priorRecording = MIDIRecordings[MIDIRecordings.Count - 1];
            StopCoroutine(priorRecording.PlayNotes);
            //if the recording is the current one we are looping then ensure the current recording is now null (i.e., reset it)
            if (recorder == priorRecording)
            {
                recOutput = false;
                recDub = false;
                recorder = null;
            }
            MIDIRecordings.Remove(priorRecording);
        }
        else
            Debug.Log("No loops recorded!");
    }

    public double LongestLoop()
    {
        if (MIDIRecordings.Count > 1)
        {
            double longestLoop = MIDIRecordings.Max(t => t.loopLength);
            return longestLoop;

        }
        else
            return 0;
    }

    public bool IsRecording(Recording recording) => recOutput && recorder != recording;

    public void Exit()
    {
        Application.Quit();
    }
}
