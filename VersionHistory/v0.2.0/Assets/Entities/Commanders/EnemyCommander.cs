using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.World;

public class EnemyCommander : MonoBehaviour {

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //COMMANDER AI
    public static string cmdIssued = "...";
    private char[] cmdChain = new char[4];
    private int cmdIndex = 0;

    public static bool enemyTurn = false;

    //LANES

    /*Parent GameObject holding the three lane objects*/
    [SerializeField] private string LANE_PARENT = "Lanes";

    private GameObject laneParent;
    private LaneInterface laneInterface;

    //RENDERING
    private SpriteRenderer rend;

    void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;

        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        /*Initialize cmdChain with dummy inputs*/
        for ( int i = 0; i < 4; i++ ) {
            cmdChain[i] = 'x';
        }

        /*Load lanes*/
        laneParent = GameObject.Find(LANE_PARENT);
        laneInterface = laneParent.GetComponent<LaneInterface>();
    }

    void Update() {

        /*Determine current turn*/
        if ( conductor.turnPos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || conductor.turnPos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {
            enemyTurn = true;
        } else {
            enemyTurn = false;
        }

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;
            StartCoroutine( Bop() );

            /*Check if adversary's turn*/
            if ( enemyTurn ) {
                
                /*Choose a command*/
                if ( lastBeat == 0 ) {
                    int choiceFactor = DecideNextCmd();
                    cmdChain = ChooseCmd(choiceFactor);
                }

                cmdIndex++;
            } else {
                cmdIndex = 0;
            }
        }

    }

    /*
    * Simulates the adversary commander's thought process in deciding the next command to issue to their troops
    * Generates an integer ("choice factor") to represent the command eventually decided upon
    */
    private int DecideNextCmd() {
        int finalChoice = -1;
        bool[] availableSpawns = { false, false, false };
        bool allSpawnsAvailable = true, noEnemiesInLanes = true;

        Lane lane;
        LaneMarker spawnMarker;

        /*Assess the states of all three lanes & the number of enemy troops currently deployed*/
        for ( int i = 0; i < 3; i++ ) {
            lane = laneInterface.lanes[i].GetComponent<Lane>();
            spawnMarker = lane.transform.GetChild(laneInterface.numMarkers - 1).gameObject.GetComponent<LaneMarker>();

            if ( lane.numEnemies > 0 ) {
                noEnemiesInLanes = false;
            }

            if ( !spawnMarker.isOccupied ) {
                availableSpawns[i] = true;
            } else {
                allSpawnsAvailable = false;
            }
        }

        /*If there are no currently deployed enemies, don't issue advancement commands*/
        if ( noEnemiesInLanes ) {
            finalChoice = Random.Range(4, 7);
        } else {
            finalChoice = Random.Range(1, 7); 
        }

        /*If any enemy spawn markers are occupied*/
        if ( !allSpawnsAvailable && ( finalChoice >= 4 && finalChoice <= 6 ) ) {
            finalChoice = 4; //start from the top lane and move downwards

            /*Choose the next unoccupied spawn marker*/
            while ( finalChoice < 7 ) {
                if ( availableSpawns[finalChoice - 4] ) {
                    break;
                }

                finalChoice++;
            }
        }

        /*If troop deployment is impossible, issue a random advancement command*/
        if ( finalChoice >= 7 ) {
            finalChoice = Random.Range(1, 3);
        }

        return finalChoice;
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
        while ( transform.localScale.y > bopMin ) {
            /*Halt on game pause*/
            while ( InputManager.gamePaused ) {
                yield return null;
            }
            
            transform.localScale += new Vector3( 0, -0.03f, 0);
            yield return null;
        }

        /*Stretch*/
        while ( transform.localScale.y < originScale ) {
            /*Halt on game pause*/
            while ( InputManager.gamePaused ) {
                yield return null;
            }
            
            transform.localScale += new Vector3( 0, 0.03f, 0);
            yield return null;
        }
    }

}
