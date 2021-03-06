using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCommander : MonoBehaviour {

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //COMMANDER ORDERS
    public static string cmdIssued = "...";
    private char[] cmdChain = new char[4];
    private int cmdIndex = 0; 

    //MISC
    private SpriteRenderer rend;

    void Start() {
        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        /*Initialize cmdChain with dummy inputs*/
        for ( int i = 0; i < 4; i++ ) {
            cmdChain[i] = 'x';
        }
    }

    void Update() {

        float loopPos = Conductor.instance.loopPositionInBeats;

        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8; //apply input calibration to measurePos

        /*On every beat*/
        if ( lastBeat != currBeat ) {

            lastBeat = currBeat;
            StartCoroutine( Bop() );

            /*Check if adversary's turn*/
            if ( measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {
                /*Choose a command (temp, currently random)*/
                if ( lastBeat == 0 ) {
                    cmdChain = ChooseCmd(Random.Range(1, 7));
                }

                cmdIndex++;

            } else {
                cmdIndex = 0;
            }

        }

    }

    /*
    * Takes a random integer (aka "choice factor") generated by the adversary and selects a command from the Commands
    * class to perform
    */
    private char[] ChooseCmd( int choiceFactor ) {
        if ( choiceFactor == 1 ) {
            cmdIssued = Commands.MARCH_STR;
            return Commands.MARCH;
        } else if ( choiceFactor == 2 ) {
            cmdIssued = Commands.STEADY_STR;
            return Commands.STEADY;
        } else if ( choiceFactor == 3 ) {
            cmdIssued = Commands.CHARGE_STR;
            return Commands.CHARGE;
        } else if ( choiceFactor == 4 ) {
            cmdIssued = Commands.SPAWN_TOP_STR;
            return Commands.SPAWN_TOP;
        } else if ( choiceFactor == 5 ) {
            cmdIssued = Commands.SPAWN_MID_STR;
            return Commands.SPAWN_MID;
        } else if ( choiceFactor == 6 ) {
            cmdIssued = Commands.SPAWN_BOTTOM_STR;
            return Commands.SPAWN_BOTTOM;
        }

        return null;
    }

    private IEnumerator Bop() {
        /*Squish*/
        while ( transform.localScale.y > bopMin ) {
            transform.localScale += new Vector3( 0, -0.03f, 0);
            yield return null;
        }

        /*Stretch*/
        while ( transform.localScale.y < originScale ) {
            transform.localScale += new Vector3( 0, 0.03f, 0);
            yield return null;
        }
    }

}
