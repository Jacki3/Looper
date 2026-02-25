using AudioHelm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MIDILooper : MonoBehaviour
{
    public static bool recOutput;
    public static bool recDub;
    public HelmController helmController;
    public List<Recording> MIDIRecordings = new List<Recording>();
    private Recording recorder;
    public HelmChannel[] channels = new HelmChannel[15];
    private Coroutine masterPlayback;
    private bool stopAfterCycle;
    private bool waitingToRecord;

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
        public double recordingEndTime;
        public bool loopComplete;
        //this keeps track of the audio time since the loop began and is used when a midi note is played to keep track of what time it was played during the loop
        public double songTime;
        public MIDILooper looper;
        public IEnumerator PlayNotes;
        public double playbackStartTime;
        public bool isPlayingInSequence = false;

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

                    for (int i = 0; i <= SortedList.Count; i++)
                    {
                        //wait either for the first note or if the previous note depending on where we are in the loop
                        float waitTime = 0;
                        if (i != SortedList.Count)
                        {
                            waitTime = (float)SortedList[i].timeNotePlayed;
                            if (i > 0)
                                waitTime = (float)(SortedList[i].timeNotePlayed - SortedList[i - 1].timeNotePlayed);
                        }
                        else
                        {
                            waitTime = (float)(recordingEndTime - SortedList[i - 1].timeNotePlayed);
                        }

                        yield return new WaitForSeconds(waitTime);
                        //here we have hardcoded the velocity and how long the note was played for - velocity is easy to get but note duration will require listening for note off rather than on in MIDI
                        if (i != SortedList.Count)
                            helmController.NoteOn(SortedList[i].noteNumber, 1f, 0.5f);
                    }
                    if (this.loopLength > looper.LongestLoop())
                    {
                        //no need to wait as this is the longest loop                    
                    }
                    else
                    {
                        //wait for the longest loop to finish
                        yield return new WaitForSeconds((float)looper.LongestLoop() - (float)this.loopLength);
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
            loopLength = recordingEndTime;
            if (looper.MIDIRecordings.Count <= 1)
                StartSongTime();
        }
    }

    [Serializable]
    public class HelmChannel
    {
        public HelmController controller;
        public string patchName;
        public List<Recording> recordings = new List<Recording>();
        public bool InUse => recordings.Count > 0;
    }

    private HelmChannel FindOrAllocateChannel()
    {
        string patchName = HelmPatchSelector.currentPatch.patchData.patch_name;
        // First: find an existing channel with the same patch
        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i].InUse && channels[i].patchName == patchName)
                return channels[i];
        }

        // Second: find the next free channel
        for (int i = 0; i < channels.Length; i++)
        {
            if (!channels[i].InUse)
            {
                channels[i].patchName = patchName;
                channels[i].controller.LoadPatch(HelmPatchSelector.currentPatch);
                return channels[i];
            }
        }

        // No channels available
        Debug.LogWarning("All 15 loop channels are in use!");
        return null;
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
        // how long the note was held down for
        public float length;
        public HelmController controller;
    }

    private void OnEnable()
    {
        Keyboard.MIDIPlayed += RecordMIDINote;
        Keyboard.MIDIOff += RecordMIDINoteOff;
    }

    private void OnDisable()
    {
        Keyboard.MIDIPlayed -= RecordMIDINote;
        Keyboard.MIDIOff -= RecordMIDINoteOff;
    }

    private IEnumerator PlayLoopsSequentially()
    {
        while (true)
        {
            for (int r = 0; r < MIDIRecordings.Count; r++)
            {
                Recording rec = MIDIRecordings[r];
                rec.isPlayingInSequence = true;
                rec.playbackStartTime = AudioSettings.dspTime;

                List<Notes> sorted = rec.notes.OrderBy(o => o.timeNotePlayed).ToList();

                for (int i = 0; i < sorted.Count; i++)
                {
                    float waitTime;
                    if (i == 0)
                        waitTime = (float)sorted[i].timeNotePlayed;
                    else
                        waitTime = (float)(sorted[i].timeNotePlayed - sorted[i - 1].timeNotePlayed);

                    yield return new WaitForSeconds(waitTime);
                    //rec.helmController.NoteOn(sorted[i].noteNumber, 1f, sorted[i].length);
                    sorted[i].controller.NoteOn(sorted[i].noteNumber, 1f, sorted[i].length);
                }

                if (sorted.Count > 0)
                {
                    float remaining = (float)(rec.recordingEndTime - sorted[sorted.Count - 1].timeNotePlayed);
                    yield return new WaitForSeconds(remaining);
                }

                rec.isPlayingInSequence = false;
            }

            if (stopAfterCycle)
            {
                stopAfterCycle = false;
                masterPlayback = null;
                if (waitingToRecord && recorder != null)
                {
                    waitingToRecord = false;
                    recOutput = true;
                    recorder.StartSongTime();
                    StartCoroutine(recorder.StartTime());
                }
                yield break;
            }
        }
    }

    public void RecordLoop()
    {
        isPlaying = true;

        if (!recOutput)
        {
            //recording a brand new loop
            //create a new instance of the recording class
            recorder = new Recording();
            recorder.looper = this;
            //recorder.StartSongTime();
            //recorder.looper = this;
            //StartCoroutine(recorder.StartTime());

            if (MIDIRecordings.Count > 0)
            {
                waitingToRecord = true;
                stopAfterCycle = true;

                Recording master = MIDIRecordings[0];
                double elapsed = AudioSettings.dspTime - master.songTime;
                double postInCycle = elapsed % master.loopLength;
                double timeUntilBoundary = master.loopLength - postInCycle;
                recorder.songTime = AudioSettings.dspTime + timeUntilBoundary;
            }
            else
            {
                recOutput = true;
                recorder.StartSongTime();
                StartCoroutine(recorder.StartTime());
            }
        }
        else
        {
            recOutput = false;
            //if the recording has some midi notes to be recorded
            if (recorder.notes.Count > 0)
            {
                recorder.loopComplete = true;
                recorder.RecordLoop();

                HelmChannel channel = FindOrAllocateChannel();

                if (channel != null)
                {
                    recorder.helmController = channel.controller;
                    channel.recordings.Add(recorder);
                    MIDIRecordings.Add(recorder);
                    recorder.recordingEndTime = recorder.songPos;
                    recorder.notes.RemoveAll(n => n.timeNotePlayed < 0);
                }
            }
            else
                recorder = null;

            if (masterPlayback != null)
                StopCoroutine(masterPlayback);
            if (MIDIRecordings.Count > 0)
                masterPlayback = StartCoroutine(PlayLoopsSequentially());
        }
    }

    public void Overdub()
    {
        if (recorder != null && masterPlayback != null)
        {
            if (MIDIRecordings.Count == 0 || masterPlayback == null)
            {
                Debug.Log("No loop to overdub on!");
                return;
            }

            if (!recOutput)
            {
                recOutput = true;
                recDub = true;
                HelmChannel channel = FindOrAllocateChannel();
            }
            else
            {
                recOutput = false;
                recDub = false;
            }
        }
    }

    private Dictionary<int, Notes> pendingNotes = new Dictionary<int, Notes>();
    private void RecordMIDINote(int note, float velocity)
    {
        if (recOutput)
        {
            HelmChannel channel = FindOrAllocateChannel();
            if (channel == null) return;

            // If overdubbing, add to whichever recording is currently playing
            Recording target = recDub ? GetActiveRecording() : recorder;
            if (target == null) return;

            Notes newNote = new Notes();
            newNote.noteNumber = note;
            newNote.noteVelocity = velocity;
            newNote.controller = channel.controller;

            if (recDub && target.isPlayingInSequence)
                newNote.timeNotePlayed = AudioSettings.dspTime - target.playbackStartTime;
            else
                newNote.timeNotePlayed = target.songPos;

            target.notes.Add(newNote);
            pendingNotes[note] = newNote;
        }
    }

    private void RecordMIDINoteOff(int note)
    {
        if (pendingNotes.TryGetValue(note, out Notes pending))
        {
            Recording target = recDub ? GetActiveRecording() : recorder;
            if (target == null) return;

            double currentTime = (recDub && target.isPlayingInSequence)
                ? AudioSettings.dspTime - target.playbackStartTime
                : target.songPos;

            pending.length = (float)(currentTime - pending.timeNotePlayed);
            pendingNotes.Remove(note);
        }
    }

    private Recording GetActiveRecording()
    {
        for (int i = 0; i < MIDIRecordings.Count; i++)
        {
            if (MIDIRecordings[i].isPlayingInSequence)
                return MIDIRecordings[i];
        }
        return null;
    }

    private bool isPlaying;

    public void StopPlaying()
    {
        if (masterPlayback != null)
            StopCoroutine(masterPlayback);
        masterPlayback = null;
        isPlaying = false;
    }

    public void StartPlaying()
    {
        if (isPlaying) return;
        isPlaying = true;
        masterPlayback = StartCoroutine(PlayLoopsSequentially());
    }

    public void RemovePriorLoop()
    {
        if (MIDIRecordings.Count >= 1)
        {
            if (masterPlayback != null)
                StopCoroutine(masterPlayback);


            Recording priorRecording = MIDIRecordings[MIDIRecordings.Count - 1];

            foreach (HelmChannel channel in channels)
            {
                if (channel.recordings.Remove(priorRecording))
                {
                    // If channel has no more recordings, it's free again
                    if (!channel.InUse)
                        channel.patchName = null;
                    break;
                }
            }

            if (recorder == priorRecording)
            {
                recOutput = false;
                recDub = false;
                recorder = null;
            }

            MIDIRecordings.Remove(priorRecording);

            if (MIDIRecordings.Count > 0)
                masterPlayback = StartCoroutine(PlayLoopsSequentially());
            else
                masterPlayback = null;
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
