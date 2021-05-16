using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class Conductor : MonoBehaviour, IInspectorEditable {

        // LOGISTICS
        //[HideInInspector] public bool hasLoaded = false;

        // SONG PROPERTIES
        [HideInInspector] public static Conductor instance;
        [HideInInspector] public AudioSource[] musicSources;
        
        [HideInInspector] private float _dspSongTime = 0;
        [HideInInspector] private float _secPerBeat = 0;
        [HideInInspector] public float songPos = 0;
        [HideInInspector] public float songPosAbs = 0;
        [HideInInspector] public float songPosInBeats = 0;
        [HideInInspector] public float songPosAbsInBeats = 0;
        
        [HideInInspector] private int _numLoopsCompleted = 0;
        [HideInInspector] public float loopPosInBeats = 0;
      
        // ADAPTIVE PLAYER
        [HideInInspector] public int trackID = 0;

        // INTERNAL METRONOME
        [HideInInspector] public bool songStarted = false;
        [HideInInspector] public float turnPos = 0f;
        [HideInInspector] public int currentBeat = -1;
        [HideInInspector] public int measurePosInBeats = -1;
        [HideInInspector] public bool onBeat = false;

        // SONG SETTINGS
        [Header("Audio Clips")]
        public AudioClip playerCue;
        public AudioClip troopAtkSfx;

        [Header("Soundtrack")]
        public float songBPM = 120;
        public int beatsPerLoop = 0;
        public int beatsPerMeasure = 4;
        public int numCountInBeats = 0;
        public float leadOffset = 0f;
  
        [Header("Input Calibration")]
        [Range(-0.25f, 0.25f)] public float playerInputOffset = 0;
        public float minInputWindow = 0.8f;
        public float maxInputWindow = 0.2f;
        public float minPerfectInputWindow = 0.85f;
        public float maxPerfectInputWindow = 0.15f;

        private void Start() {
            _secPerBeat = 60f / songBPM;
            _dspSongTime = (float) AudioSettings.dspTime;
            musicSources = GetComponents<AudioSource>();
            musicSources[trackID].PlayScheduled( _dspSongTime + leadOffset + (_secPerBeat * numCountInBeats) );
        }

        private void Awake() {
            instance = this;
        }

        private void Update() {

            ValidateProperties(); //temp - move out of Update()
        
            //Initialize song-positional variables
            songPos = (float) ( AudioSettings.dspTime - _dspSongTime - leadOffset - (_secPerBeat * numCountInBeats) );
            songPosAbs = (float) ( AudioSettings.dspTime - _dspSongTime - leadOffset );

            songPosInBeats = songPos / _secPerBeat;
            songPosAbsInBeats = songPosAbs / _secPerBeat;

            if ( songPosInBeats >= (_numLoopsCompleted + 1) * beatsPerLoop ) {
                _numLoopsCompleted++;            
            }

            //Acknowledge count-in, if given
            if ( numCountInBeats == 0 ) {
                songStarted = true;
            }
            
            if ( songStarted && songPosInBeats >= 0 ) {
                loopPosInBeats = songPosInBeats - (_numLoopsCompleted * beatsPerLoop);
                currentBeat = (int) loopPosInBeats % beatsPerMeasure;
                turnPos = (loopPosInBeats - playerInputOffset) % (beatsPerMeasure * 2);

            } else if ( !songStarted && songPosAbsInBeats >= 0 ) {
                loopPosInBeats =  songPosAbsInBeats - (_numLoopsCompleted * beatsPerLoop);
                currentBeat = (int) loopPosInBeats % numCountInBeats;
            }

            //To do on every beat
            onBeat = BeatCheck();

            if ( onBeat ) {

                if ( songStarted ) {
                    if ( measurePosInBeats == 0 && loopPosInBeats < beatsPerMeasure ) {
                        double nextLoopTime = AudioSettings.dspTime + (_secPerBeat * beatsPerLoop);
                        trackID = 1 - trackID;
                        musicSources[trackID].PlayScheduled(nextLoopTime);
                    }

                } else {
                    if ( measurePosInBeats >= numCountInBeats - 1 ) {
                        songStarted = true;
                    }
                    
                    musicSources[0].PlayOneShot(playerCue, 0.6f);
                }
                
            }
            
        }

        /*
        * Updates a public flag on each beat, allowing classes to do stuff in time with the song
        */
        public bool BeatCheck() {
            if ( measurePosInBeats != currentBeat ) {
                measurePosInBeats = currentBeat;
                return true;
            } else {
                return false;
            }
        }

        public void ValidateProperties() {
            if ( numCountInBeats < 0 ) {
                numCountInBeats = 0;
            } else if ( beatsPerMeasure < 2 ) {
                beatsPerMeasure = 2;
            } else if ( songBPM <= 0 ) {
                songBPM = 120;
            } else if ( playerInputOffset <= -0.25f || playerInputOffset >= 0.25f ) {
                playerInputOffset = 0;
            }
        }
        
    }

}