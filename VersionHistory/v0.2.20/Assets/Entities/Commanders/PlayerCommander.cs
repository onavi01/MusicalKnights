using System.Linq; //included for Enumerable.SequenceEqual() function
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.UI;
using MusicalKnights.World;

namespace MusicalKnights.Entity.Commander {
    public class PlayerCommander : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;
        private double _inputAcc = 0f;

        // BOP (TEMP)
        private float _originScale = 0;
        private float _bopMin = 0;

        // COMMANDER AI
        private char[] _cmdChain = new char[4]; 
        private int _cmdCount = 0;
        [HideInInspector] public static string cmdIssued = "...";
        [HideInInspector] public int numMistakesThisTurn = 0;

        private float _timeOfLastInput = 0f;
        private float _timeSinceLastInput = 0f;
        private bool _isTurnValidated = true;
        [HideInInspector] public static bool playerTurn = false;
        public const float playerTurnStartThreshold = 3.7f;
        public const float playerTurnEndThreshold = 7.7f;
        public const int mistakeLimit = 2;
        
        // RENDERING
        private SpriteRenderer _rend;

        // UI
        private Text _inputAccText;

        private void Start() {
            _conductor = Conductor.instance;

            _originScale = transform.localScale.y;
            _bopMin = transform.localScale.y - 0.3f;
            _rend = GetComponent<SpriteRenderer>();

            //Initialize cmdChain with dummy inputs
            for ( int i = 0; i < 4; i++ ) {
                _cmdChain[i] = 'x';
            }

            //Load UI-related
            _inputAccText = GameObject.Find("Input Accuracy").GetComponent<Text>();
        }

        private void Update() {

            _inputAcc = _conductor.turnPos % 1;

            if ( _conductor.songStarted ) {

                //Assess player performance just before the start of the adversary's turn to give troops time to react
                if ( _conductor.turnPos > playerTurnEndThreshold ) {
                    
                    if ( !_isTurnValidated ) {
                        IssueCmd( _cmdChain );

                        _isTurnValidated = true;
                        _cmdCount = 0;

                        playerTurn = false;
                        numMistakesThisTurn = 0;   
                    }

                //Reset validation at the start of the player's turn
                } else if ( _conductor.turnPos >= playerTurnStartThreshold && _conductor.turnPos <= playerTurnEndThreshold ) {
                    if ( _isTurnValidated ) {
                        _isTurnValidated = false;
                        playerTurn = true;
                        cmdIssued = "...";
                    }
                }

            }

            //Actively listen for commands
            if ( !InputManager.gamePaused ) {

                bool tooManyInputs;
                char cmd = 'x';

                if ( Input.GetButtonDown("UCmd") || Input.GetButtonDown("LCmd") || Input.GetButtonDown("DCmd") || Input.GetButtonDown("RCmd") ) {
                    _timeSinceLastInput = Time.time - _timeOfLastInput;

                    if ( _timeSinceLastInput > _conductor.maxInputWindow * 2 ) {
                        _timeOfLastInput = Time.time;
                        tooManyInputs = false;
                    } else {
                        tooManyInputs = true;
                    }

                    if ( !SimultaneousInputs() ) {
                        if ( Input.GetButtonDown("UCmd") ) {
                            cmd = 'w';
                        } else if ( Input.GetButtonDown("LCmd") ) {
                            cmd = 'a';
                        } else if ( Input.GetButtonDown("DCmd") ) {
                            cmd = 's';
                        } else if ( Input.GetButtonDown("RCmd") ) {
                            cmd = 'd';
                        }
                    } else {
                        cmd = 'x';
                        tooManyInputs = true;
                    }

                    StartCoroutine( ValidateCmd( cmd, _inputAcc, playerTurn, tooManyInputs ) );
                }

            }

            //Do on every beat
            if ( _conductor.onBeat ) {
                StartCoroutine( Bop() );

                if ( playerTurn ) {
                    _conductor.musicSources[1 - _conductor.trackID].PlayOneShot(_conductor.playerCue, 0.45f);
                }
            }

        }

        /*
        * Checks if two or more buttons are pressed at once
        */
        private bool SimultaneousInputs() {
            foreach ( KeyCode kCode1 in System.Enum.GetValues( typeof(KeyCode) ) ) {
                if ( Input.GetKey(kCode1) ) {

                    foreach ( KeyCode kCode2 in System.Enum.GetValues( typeof(KeyCode) ) ) {
                        if ( Input.GetKeyDown(kCode2) && kCode2 != kCode1 ) {
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        /*
        * Attempts to parse the player's command inputs into one of the 6 valid
        * game commands
        */
        private void IssueCmd( char[] cmdChain ) {

            if ( _cmdCount != 4 ) {
                return;
            }

            if ( numMistakesThisTurn > mistakeLimit ) {
                _inputAccText.text = "Too many mistakes\nthis turn!";
                return;
            }

            if ( CharArrsEqual( _cmdChain, Commands.MARCH ) ) {
                cmdIssued = Commands.MARCH_ID;
            } else if ( CharArrsEqual( _cmdChain, Commands.STEADY ) ) {
                cmdIssued = Commands.STEADY_ID;
            } else if ( CharArrsEqual( _cmdChain, Commands.CHARGE ) ) {
                cmdIssued = Commands.CHARGE_ID;
            } else if ( CharArrsEqual( _cmdChain, Commands.SPAWN_TOP ) ) {
                cmdIssued = Commands.SPAWN_TOP_ID;
            } else if ( CharArrsEqual( _cmdChain, Commands.SPAWN_MID ) ) {
                cmdIssued = Commands.SPAWN_MID_ID;
            } else if ( CharArrsEqual( _cmdChain, Commands.SPAWN_BTM ) ) {
                cmdIssued = Commands.SPAWN_BTM_ID;
            } else {
                cmdIssued = "do what now?";
            }

        }

        /*
        * Used to compare two arrays of characters for element equality
        */
        private bool CharArrsEqual( char[] arr1, char[] arr2 ) {
            return Enumerable.SequenceEqual( arr1, arr2 );
        }

        /*
        * Takes a command entered by the user and determines whether or not to add it to the final
        * command chain based on several key factors:
        *   - if it's currently the player's turn
        *   - input accuracy
        *   - the amount of inputs being entered at once
        */
        private IEnumerator ValidateCmd( char cmd, double inputAcc, bool playerTurn, bool tooManyInputs ) {

            bool inputSuccess = false;
            
            //Load input windows
            float perfectMin = _conductor.minPerfectInputWindow;
            float perfectMax = _conductor.maxPerfectInputWindow;
            float goodMin = _conductor.minInputWindow;
            float goodMax = _conductor.maxInputWindow;

            if ( playerTurn && numMistakesThisTurn <= mistakeLimit && !tooManyInputs ) {
                if ( inputAcc >= perfectMin || inputAcc <= perfectMax ) {
                    inputSuccess = true;
                    _rend.color = new Color(0.5f, 1f, 0.5f, 1f);
                    
                } else if ( ( inputAcc < perfectMin && inputAcc >= goodMin ) || ( inputAcc > perfectMax && inputAcc <= goodMax ) ) {
                    inputSuccess = true;
                    _rend.color = new Color(1f, 0.92f, 0.016f, 1f);
                }
            }

            if ( inputSuccess ) {
                _cmdChain[_cmdCount] = cmd;
                _cmdCount++;

                if ( inputAcc >= 0.5 ) {
                    double accuracy = (1f - inputAcc) * -1;
                    _inputAccText.text = "Accuracy: " + accuracy.ToString("N5");
                } else {
                    _inputAccText.text = "Accuracy: " + inputAcc.ToString("N5");
                }

            } else {
                _rend.color = new Color(1f, 0.5f, 0.5f, 1f);

                //Update error messages accordingly
                if ( !playerTurn ) {
                    _inputAccText.text = "Not your turn!";

                } else if ( numMistakesThisTurn > mistakeLimit ) {
                    _inputAccText.text = "Too many mistakes\nthis turn!";

                } else if ( tooManyInputs ) {
                    _inputAccText.text = "Slow down!";
                    
                } else {
                    if ( inputAcc >= 0.5 ) {
                        double accuracy = (1f - inputAcc) * -1;
                        _inputAccText.text = "Accuracy: " + accuracy.ToString("N5") + "\nToo early!";
                    } else {
                        _inputAccText.text = "Accuracy: " + inputAcc.ToString("N5") + "\nToo late!";
                    }
                }

                if ( playerTurn ) {
                    numMistakesThisTurn++;
                }
            }

            yield return new WaitForSeconds(0.1f);

            _rend.color = new Color(1f,1f,1f,1f); //reset color

        }

        private IEnumerator Bop() {
            /*Squish*/
            while ( transform.localScale.y > _bopMin ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                transform.localScale += new Vector3( 0, -0.03f, 0);
                yield return null;
            }

            /*Stretch*/
            while ( transform.localScale.y < _originScale ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                transform.localScale += new Vector3( 0, 0.03f, 0);
                yield return null;
            }
        }

    }

}