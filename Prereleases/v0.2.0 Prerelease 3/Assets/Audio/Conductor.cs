using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour {

    //CONDUCTOR PARAMETERS
    [SerializeField] public int countInBeats = 0;
    [SerializeField] public float songBPM = 0;
    private float secPerBeat = 0;
    public float songPosition = 0;
    public float timePosition = 0;
    public float songPositionInBeats = 0;
    public float timePositionInBeats = 0;
    private float dspSongTime = 0;

    public float beatsPerLoop = 0; //inputted manually
    private int completedLoops = 0;
    public float loopPositionInBeats = 0;

    /*Offsets*/
    public float leadOffset = 0f;
    public float playerInputOffset = 0;

    //CONDUCTOR OBJECT
    public static Conductor instance;
    public AudioSource[] musicSources;
    [SerializeField] public AudioClip playerCue;
    [SerializeField] public AudioClip attackSfx;

    //INTERNAL METRONOME
    public bool songStart = false;
    public int currBeat = -1;
    public int lastBeat = -1;
    public float measurePos = 0f;

    //ADAPTIVE PLAYER
    public bool playWarning = false;
    public int track = 0;

    //INPUT PRECISION
    public const float MIN_INPUT_WINDOW = 0.8f;
    public const float MAX_INPUT_WINDOW = 0.2f;
    public const float MIN_PERFECT_INPUT_WINDOW = 0.85f;
    public const float MAX_PERFECT_INPUT_WINDOW = 0.15f;

    private void Start() {
        musicSources = GetComponents<AudioSource>();    //load AudioSources
        secPerBeat = 60f / songBPM;                     //calculate number of seconds in each beat
        dspSongTime = (float) AudioSettings.dspTime;    //record time passed since music start
        musicSources[track].PlayScheduled( dspSongTime + leadOffset + (secPerBeat * countInBeats) );
    }

    private void Awake() {
        instance = this;
    }

    private void Update() {
        timePosition = (float) ( AudioSettings.dspTime - dspSongTime - leadOffset );
        songPosition = (float) ( AudioSettings.dspTime - dspSongTime - leadOffset - (secPerBeat * countInBeats) ); //determine seconds since song started

        timePositionInBeats = timePosition / secPerBeat;
        songPositionInBeats = songPosition / secPerBeat;
        
        /*Increment loops completed*/
        if ( songPositionInBeats >= (completedLoops + 1) * beatsPerLoop ) {
            completedLoops++;            
        }

        if ( countInBeats == 0 ) {
            songStart = true;
        }

        /*Update internal metronome*/
        if ( songStart && songPositionInBeats >= 0 ) {
            loopPositionInBeats = songPositionInBeats - (completedLoops * beatsPerLoop);
            currBeat = (int) loopPositionInBeats % 4;
            measurePos = (loopPositionInBeats - playerInputOffset) % 8; //apply input calibration
        } else if ( !songStart && timePositionInBeats >= 0 ) {
            loopPositionInBeats =  timePositionInBeats - (completedLoops * beatsPerLoop);
            currBeat = (int) loopPositionInBeats % countInBeats;
        }
        
        /*On every beat*/
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;

            if ( songStart && lastBeat == 0 && loopPositionInBeats < 1 ) {
                double nextLoopTime = AudioSettings.dspTime + (secPerBeat * beatsPerLoop);
                track = 1 - track;
                musicSources[track].PlayScheduled(nextLoopTime);
            }

            if ( !songStart ) {
                if ( lastBeat >= countInBeats - 1 ) {
                    songStart = true;
                }
                
                musicSources[0].PlayOneShot(playerCue, 0.6f);
            }
        }
    }
    
}
