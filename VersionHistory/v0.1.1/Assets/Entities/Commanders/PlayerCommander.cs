using System.Linq; //included for Enumerable.SequenceEqual() function
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCommander : MonoBehaviour {

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;
    private double inputAcc = 0f;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //COMMANDER ORDERS
    public static string cmdIssued = "...";
    private int cmdCount = 0;
    private char[] cmdChain = new char[4];
    private bool playerTurn = true, doValidateTurn = true;

    public const float PLAYER_TURN_START_THRESHOLD = 3.7f;
    public const float PLAYER_TURN_END_THRESHOLD = 7.7f;
    
    //RENDERING
    private SpriteRenderer rend;

    //UI
    private Text inputAccUI;

    void Start() {
        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        /*Initialize cmdChain with dummy inputs*/
        for ( int i = 0; i < 4; i++ ) {
            cmdChain[i] = 'x';
        }

        inputAccUI = GameObject.Find("Input Accuracy").GetComponent<Text>(); //load input accuracy UI element
    }

    void Update() {

        float loopPos = Conductor.instance.loopPositionInBeats;

        /*Apply input calibration is applied to measurePos and input accuracy*/
        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;
        inputAcc = (loopPos - Conductor.instance.playerInputOffset) % 1;

        /*Check if adversary's turn*/
        if ( measurePos < PLAYER_TURN_START_THRESHOLD || measurePos > PLAYER_TURN_END_THRESHOLD ) {
            
            /*Incorrect # of inputs*/
            if ( cmdCount != 4 && doValidateTurn ) {
                cmdIssued = "...";
            }

            /*Check if command sequence is valid*/
            if ( cmdCount == 4 && doValidateTurn ) {

                /*Compare player-inputted command chain with list of valid commands within class Commands*/
                if ( CmdChainEquals( cmdChain, Commands.MARCH ) ) {
                    cmdIssued = Commands.MARCH_STR;
                } else if ( CmdChainEquals( cmdChain, Commands.STEADY ) ) {
                    cmdIssued = Commands.STEADY_STR;
                } else if ( CmdChainEquals( cmdChain, Commands.CHARGE ) ) {
                    cmdIssued = Commands.CHARGE_STR;
                } else if ( CmdChainEquals( cmdChain, Commands.SPAWN_TOP ) ) {
                    cmdIssued = Commands.SPAWN_TOP_STR;
                } else if ( CmdChainEquals( cmdChain, Commands.SPAWN_MID ) ) {
                    cmdIssued = Commands.SPAWN_MID_STR;
                } else if ( CmdChainEquals( cmdChain, Commands.SPAWN_BOTTOM ) ) {
                    cmdIssued = Commands.SPAWN_BOTTOM_STR;
                } else {
                    cmdIssued = "do what now?";
                }

            }

            /*Reset flags & counters*/
            playerTurn = false;
            doValidateTurn = false;
            cmdCount = 0;
            
        /*If player's turn*/
        } else {
            playerTurn = true;
            doValidateTurn = true;
            cmdIssued = "...";
        }

        /*Bop (temp)*/
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;
            StartCoroutine( Bop() );
        }

        /*Commands*/
        if ( Input.GetKeyDown(KeyCode.W) ) {
            StartCoroutine( ValidateCmd( 'w', inputAcc, playerTurn ) );
        } else if ( Input.GetKeyDown(KeyCode.A) ) {
            StartCoroutine( ValidateCmd( 'a', inputAcc, playerTurn ) );
        } else if ( Input.GetKeyDown(KeyCode.S) ) {
            StartCoroutine( ValidateCmd( 's', inputAcc, playerTurn ) );
        } else if ( Input.GetKeyDown(KeyCode.D) ) {
            StartCoroutine( ValidateCmd( 'd', inputAcc, playerTurn ) );
        }

    }

    /*
     * Used to compare two "command sequences" for element equality
     * Allows us to check if a player command matches any of the available commands defined in the Commands class
     */
    private bool CmdChainEquals( char[] chain1, char[] chain2 ) {
        return Enumerable.SequenceEqual( chain1, chain2 );
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

    private IEnumerator ValidateCmd( char cmd, double inputAcc, bool playerTurn ) {
        bool inputSuccess = false;
        
        /*Input windows*/
        float perfectMin = Conductor.MIN_PERFECT_INPUT_WINDOW;
        float perfectMax = Conductor.MAX_PERFECT_INPUT_WINDOW;
        float goodMin = Conductor.MIN_INPUT_WINDOW;
        float goodMax = Conductor.MAX_INPUT_WINDOW;

        /*Determine accuracy & validity of player input*/
        /*Perfect input*/
        if ( playerTurn && ( inputAcc >= perfectMin || inputAcc <= perfectMax ) ) {
            inputSuccess = true;
            rend.color = new Color(0.5f, 1f, 0.5f, 1f);

            /*Only accept up to 4 commands*/
            if ( cmdCount < 4 ) {
                cmdChain[cmdCount] = cmd;
            }
            
            cmdCount++;

            /*Temp, update UI*/
            if ( inputAcc >= 0.5 ) {
                double accuracy = (1f - inputAcc) * -1;
                inputAccUI.text = "Accuracy: " + accuracy.ToString("N5");
            } else {
                inputAccUI.text = "Accuracy: " + inputAcc.ToString("N5");
            }
            
        /*Good input*/
        } if ( playerTurn && ( ( inputAcc < perfectMin && inputAcc >= goodMin ) || ( inputAcc > perfectMax && inputAcc <= goodMax ) ) ) {
            inputSuccess = true;
            rend.color = new Color(1f, 0.92f, 0.016f, 1f);

            /*Only accept up to 4 commands*/
            if ( cmdCount < 4 ) {
                cmdChain[cmdCount] = cmd;
            }
            
            cmdCount++;

            /*Temp, update UI*/
            if ( inputAcc >= 0.5 ) {
                double accuracy = (1f - inputAcc) * -1;
                inputAccUI.text = "Accuracy: " + accuracy.ToString("N5");
            } else {
                inputAccUI.text = "Accuracy: " + inputAcc.ToString("N5");
            }

        }

        /*Poor input*/
        if ( !inputSuccess ) {
            rend.color = new Color(1f, 0.5f, 0.5f, 1f);

            /*Debug - Error messages*/
            if (!playerTurn) {
                inputAccUI.text = "Not your turn!";
            } else {
                /*Temp, update UI*/
                if ( inputAcc >= 0.5 ) {
                    double accuracy = (1f - inputAcc) * -1;
                    inputAccUI.text = "Accuracy: " + accuracy.ToString("N5") + "\nToo early!";
                } else {
                    inputAccUI.text = "Accuracy: " + inputAcc.ToString("N5") + "\nToo late!";
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);

        rend.color = new Color(1f,1f,1f,1f);

    }
}
