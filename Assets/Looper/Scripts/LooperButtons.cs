using TMPro;
using UnityEngine;

//simple class that listens for the MIDI looper bools and changes text based on this
public class LooperButtons : MonoBehaviour
{
    public TextMeshProUGUI loopText;
    public TextMeshProUGUI dubText;
    public string recordingText = "rec...";

    private string defaultLoopText;
    private string defaultDubText;

    private void Start()
    {
        defaultLoopText = loopText.text;
        defaultDubText = dubText.text;
    }

    private void Update()
    {
        if (MIDILooper.recOutput)
        {
            if (!MIDILooper.recDub)
                loopText.text = recordingText;
            else
                dubText.text = recordingText;
        }
        else
        {
            loopText.text = defaultLoopText;
            dubText.text = defaultDubText;
        }
    }

}
