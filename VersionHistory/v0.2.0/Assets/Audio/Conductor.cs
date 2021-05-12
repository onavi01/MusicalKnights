using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class Conductor : MonoBehaviour, IInspectorEditable {

        //SONG PARAMETERS
        [HideInInspector] private float dspSongTime = 0, secPerBeat = 0;
        [HideInInspector] public float songPos = 0, songPosAbs = 0, songPosInBeats = 0, songPosAbsInBeats = 0;
        [HideInInspector] public float beatsPerLoop = 0, loopPosInBeats = 0;
        [HideInInspector] private int numLoopsCompleted = 0;

        [HideInInspector] public static Conductor instance;
        [HideInInspector] public AudioSource[] musicSources;
        
        //ADAPTIVE PLAYER
        [HideInInspector] public int trackID = 0;

        //INTERNAL METRONOME
        [HideInInspector] public bool songStarted = false;
        [HideInInspector] public float turnPos = 0f;
        [HideInInspector] public int currBeat = -1;
        private int lastBeat = -1;

        [Header("Audio Clips")]
        public AudioClip playerCue;
        public AudioClip troopAtkSfx;

        [Header("Song Settings")]
        public float songBPM = 120;
        public int numCountInBeats = 0;
        public float leadOffset = 0f;
  
        [Header("Input Calibration")]
        [Range(-0.25f, 0.25f)] public float playerInputOffset = 0;
        public float MIN_INPUT_WINDOW = 0.8f;
        public float MAX_INPUT_WINDOW = 0.2f;
        public float MIN_PERFECT_INPUT_WINDOW = 0.85f;
        public float MAX_PERFECT_INPUT_WINDOW = 0.15f;

        private void Start() {
            musicSources = GetComponents<AudioSource>();
            secPerBeat = 60f / songBPM;
            dspSongTime = (float) AudioSettings.dspTime;
            musicSources[trackID].PlayScheduled( dspSongTime + leadOffset + (secPerBeat * numCountInBeats) );
        }

        private void Awake() {
            instance = this;
        }

        private void Update() {

            ValidateProperties();
        
            songPos = (float) ( AudioSettings.dspTime - dspSongTime - leadOffset - (secPerBeat * numCountInBeats) );
            songPosAbs = (float) ( AudioSettings.dspTime - dspSongTime - leadOffset );

            songPosInBeats = songPos / secPerBeat;
            songPosAbsInBeats = songPosAbs / secPerBeat;

            if ( numCountInBeats == 0 ) {
                songStarted = true;
            }

            if ( songPosInBeats >= (numLoopsCompleted + 1) * beatsPerLoop ) {
                numLoopsCompleted++;            
            }

            if ( songStarted && songPosInBeats >= 0 ) {
                loopPosInBeats = songPosInBeats - (numLoopsCompleted * beatsPerLoop);
                currBeat = (int) loopPosInBeats % 4;
                turnPos = (loopPosInBeats - playerInputOffset) % 8;
            } else if ( !songStarted && songPosAbsInBeats >= 0 ) {
                loopPosInBeats =  songPosAbsInBeats - (numLoopsCompleted * beatsPerLoop);
                currBeat = (int) loopPosInBeats % numCountInBeats;
            }
            
            /*On every beat*/
            if ( lastBeat != currBeat ) {
                lastBeat = currBeat;

                if ( songStarted && lastBeat == 0 && loopPosInBeats < 4 ) {
                    double nextLoopTime = AudioSettings.dspTime + (secPerBeat * beatsPerLoop);
                    trackID = 1 - trackID;
                    musicSources[trackID].PlayScheduled(nextLoopTime);
                }

                if ( !songStarted ) {
                    if ( lastBeat >= numCountInBeats - 1 ) {
                        songStarted = true;
                    }
                    
                    musicSources[0].PlayOneShot(playerCue, 0.6f);
                }
            }

        }

        public void ValidateProperties() {
            if ( numCountInBeats < 0 ) {
                numCountInBeats = 0;
            } else if ( songBPM <= 0 ) {
                songBPM = 120;
            } else if ( playerInputOffset <= -0.25f || playerInputOffset >= 0.25f ) {
                playerInputOffset = 0;
            }
        }
        
    }

}