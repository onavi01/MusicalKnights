using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class Conductor : MonoBehaviour, IInspectorEditable {

        // SONG PROPERTIES
        [HideInInspector] public static Conductor instance;
        
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
        [Header("Soundtracks")]
        public AudioSource mainTrack;
        public AudioSource[] loopingTracks;
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
        }

        private void Awake() {
            instance = this;
        }

        private void Update() {

            ValidateProperties();
        
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

            //Do on every beat
            onBeat = BeatCheck();

            if ( onBeat ) {

                if ( songStarted ) {
                    PlayMainTrackLoop();
                    
                } else {
                    mainTrack.PlayOneShot(playerCue, 0.6f);

                    if ( measurePosInBeats >= numCountInBeats - 1 ) {
                        songStarted = true;
                    }
                }
                
            }
            
        }

        /*
        * Controls the looping mechanism for the main soundtrack
        */
        private void PlayMainTrackLoop() {
            if ( measurePosInBeats != 0 ) {
                return;
            }

            double nextLoopTime = AudioSettings.dspTime + (_secPerBeat * beatsPerLoop);

            if ( songPosInBeats < beatsPerMeasure ) {
                mainTrack.Play();
                mainTrack.SetScheduledEndTime(nextLoopTime);
                loopingTracks[trackID].PlayScheduled(nextLoopTime);
                trackID = 1 - trackID;

            } else {
                if ( loopPosInBeats < beatsPerMeasure ) {
                    loopingTracks[trackID].PlayScheduled(nextLoopTime);
                    trackID = 1 - trackID;
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