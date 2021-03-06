using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour {

    //CONDUCTOR PARAMETERS
    public float songBPM; //inputted manually
    private float secPerBeat;
    private float songPosition;
    private float songPositionInBeats;
    private float dspSongTime;

    public float beatsPerLoop; //inputted manually
    private int completedLoops = 0;
    public float loopPositionInBeats;

    /*Offsets*/
    public float leadOffset = 0f;
    public float playerInputOffset = 0;

    //CONDUCTOR OBJECT
    public static Conductor instance;
    public AudioSource[] musicSources;
    [SerializeField] public AudioClip metronome;

    //ADAPTIVE PLAYER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;
    public bool playWarning = false;
    
    //CHORUS TIMESTAMPS
    public const int CHORUS_START = 128;
    public const int CHORUS_END = 192;

    //INPUT PRECISION
    public const float MIN_INPUT_WINDOW = 0.8f;
    public const float MAX_INPUT_WINDOW = 0.2f;
    public const float MIN_PERFECT_INPUT_WINDOW = 0.85f;
    public const float MAX_PERFECT_INPUT_WINDOW = 0.15f;

    private void Start() {
        Application.targetFrameRate = 60; //set target frame rate for the game

        musicSources = GetComponents<AudioSource>();    //load AudioSources
        secPerBeat = 60f / songBPM;                     //calculate number of seconds in each beat
        dspSongTime = (float) AudioSettings.dspTime;    //record time passed since music start

        /*Start all tracks at once & unmute only the main track*/
        for ( int i = 0; i < musicSources.Length; i++ ) {
            musicSources[i].PlayScheduled(dspSongTime + leadOffset);
        }

        musicSources[1].mute = true;
    }

    private void Awake() {
        instance = this;
    }

    private void Update() {
        songPosition = (float) ( AudioSettings.dspTime - dspSongTime - leadOffset); //determine seconds since song started
        songPositionInBeats = songPosition / secPerBeat;

        /*Increment loops completed and replay song*/
        if ( songPositionInBeats >= (completedLoops + 1) * beatsPerLoop ) {
            completedLoops++;            
        }

        loopPositionInBeats = songPositionInBeats - completedLoops * beatsPerLoop;

        currBeat = (int) loopPositionInBeats % 4;
        measurePos = loopPositionInBeats % 8;
        
        /*On every beat*/
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;
        }

    }
}
