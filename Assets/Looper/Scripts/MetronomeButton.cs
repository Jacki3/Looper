using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetronomeButton : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        bool muted = RhythmClock.i ? RhythmClock.i.muteMetronome : false;
        var bColour = button.image.color;
        bColour.a = muted ? .3f : 1f;
        button.image.color = bColour;
    }
}
