using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.World;

namespace MusicalKnights.Entity.Commander {
    public class EnemyCommander : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // BOP (TEMP)
        private float _originScale = 0;
        private float _bopMin = 0;

        // COMMANDER AI
        public static string cmdIssued = "...";
        private char[] _cmdChain = new char[4];
        private int _cmdIndex = 0;

        public static bool enemyTurn = false;

        // LANES
        [Header("Lane Settings")]
        [SerializeField] private string _laneParent = "Lanes";
        [HideInInspector] public LaneInterface laneInterface;
        
        private GameObject _laneParentObj;
    
        // RENDERING
        private SpriteRenderer _rend;

        void Start() {
            _conductor = Conductor.instance;

            _originScale = transform.localScale.y;
            _bopMin = transform.localScale.y - 0.3f;
            _rend = GetComponent<SpriteRenderer>();

            //Initialize cmdChain with dummy inputs
            for ( int i = 0; i < 4; i++ ) {
                _cmdChain[i] = 'x';
            }

            //Load lane-related
            _laneParentObj = GameObject.Find(_laneParent);
            laneInterface = _laneParentObj.GetComponent<LaneInterface>();
        }

        void Update() {

            //Check if adversary turn
            if ( _conductor.turnPos < PlayerCommander.playerTurnStartThreshold || _conductor.turnPos > PlayerCommander.playerTurnEndThreshold ) {
                enemyTurn = true;
            } else {
                enemyTurn = false;
            }

            //Do on every beat
            if ( _conductor.onBeat ) {
                StartCoroutine( Bop() );

                if ( enemyTurn ) {
                    
                    //Choose a command
                    if ( _conductor.measurePosInBeats == 0 ) {
                        int choiceFactor = DecideNextCmd();
                        _cmdChain = ChooseCmd(choiceFactor);
                    }

                    _cmdIndex++;
                } else {
                    _cmdIndex = 0;
                } 
            }

        }

        /*
        * Simulates the adversary commander's thought process in deciding the next command to issue to their troops
        * Generates an integer ("choice factor") to represent the command eventually decided upon
        */
        private int DecideNextCmd() {

            bool[] availableSpawns = { false, false, false };
            bool allSpawnsAvailable = true;
            bool noEnemiesInLanes = true;
            int totalDeployedEnemies = 0;

            int chosenCmd = -1;

            LaneMarker spawnMarker;

            //Assess the states of all three lanes & the number of enemy troops currently deployed
            for ( int i = 0; i < 3; i++ ) {
                laneInterface.lanes[i] = laneInterface.laneObjs[i].GetComponent<Lane>();
                spawnMarker = laneInterface.lanes[i].transform.GetChild(laneInterface.numMarkers - 1).gameObject.GetComponent<LaneMarker>();

                if ( laneInterface.lanes[i].numEnemies > 0 ) {
                    noEnemiesInLanes = false;
                } else {
                    totalDeployedEnemies += laneInterface.lanes[i].numEnemies;
                }

                if ( !spawnMarker.isOccupied ) {
                    availableSpawns[i] = true;
                } else {
                    allSpawnsAvailable = false;
                }
            }

            //If there are no currently deployed enemies, don't issue advancement commands
            if ( noEnemiesInLanes ) {
                chosenCmd = Random.Range(4, 7);

            } else {
                chosenCmd = Random.Range(1, 7);

                //Prefer not to spawn more troops once a certain active troop limit is reached
                if ( totalDeployedEnemies >= 4 && chosenCmd >= 4 ) {
                    chosenCmd = Random.Range(1, 7);
                }

                //If any enemy spawn markers are occupied, choose the next unoccupied spawn marker
                if ( !allSpawnsAvailable && ( chosenCmd >= 4 && chosenCmd <= 6 ) ) {
                    chosenCmd = 4;

                    while ( chosenCmd < 7 ) {
                        if ( availableSpawns[chosenCmd - 4] ) {
                            break;
                        }

                        chosenCmd++;
                    }
                }

                //If troop deployment is impossible, issue a random advancement command
                if ( chosenCmd >= 7 ) {
                    chosenCmd = Random.Range(1, 3);
                }
            }

            return chosenCmd;

        }

        /*
        * Selects a command to issue based on the adversary commander's "choice factor"
        */
        private char[] ChooseCmd( int choiceFactor ) {
            if ( choiceFactor == 1 ) {
                cmdIssued = Commands.MARCH_ID;
                return Commands.MARCH;
            } else if ( choiceFactor == 2 ) {
                cmdIssued = Commands.STEADY_ID;
                return Commands.STEADY;
            } else if ( choiceFactor == 3 ) {
                cmdIssued = Commands.CHARGE_ID;
                return Commands.CHARGE;
            } else if ( choiceFactor == 4 ) {
                cmdIssued = Commands.SPAWN_TOP_ID;
                return Commands.SPAWN_TOP;
            } else if ( choiceFactor == 5 ) {
                cmdIssued = Commands.SPAWN_MID_ID;
                return Commands.SPAWN_MID;
            } else if ( choiceFactor == 6 ) {
                cmdIssued = Commands.SPAWN_BTM_ID;
                return Commands.SPAWN_BTM;
            }

            return null;
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


