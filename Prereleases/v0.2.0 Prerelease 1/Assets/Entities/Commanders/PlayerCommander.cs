using System.Linq; //included for Enumerable.SequenceEqual() function
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCommander : MonoBehaviour {

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;
    private double inputAcc = 0f;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //COMMANDER ORDERS
    private char[] cmdChain = new char[4];
    public static string cmdIssued = "...";
    private int cmdCount = 0;

    private float timeOfLastInput = 0f, timeSinceLastInput = 0f;
    private bool doValidateTurn = true;
    public bool playerTurn = true;

    public const float PLAYER_TURN_START_THRESHOLD = 3.7f;
    public const float PLAYER_TURN_END_THRESHOLD = 7.7f;
    
    //RENDERING
    private SpriteRenderer rend;

    //UI
    private Text inputAccUI;

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

        inputAccUI = GameObject.Find("Input Accuracy").GetComponent<Text>(); //load input accuracy UI element
    }

    void Update() {

        inputAcc = conductor.measurePos % 1;

        /*Try to assess player performance just before the start of the adversary's turn to give troops time to react*/
        if ( conductor.measurePos > PLAYER_TURN_END_THRESHOLD ) {
            
            /*Only asses player's turn once per turn sequence*/
            if ( doValidateTurn ) {

                if ( cmdCount == 4 ) {

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

                } else {
                    cmdIssued = "...";
                }

                playerTurn = false;

                /*Reset flags & counters*/
                doValidateTurn = false;
                cmdCount = 0;

            }

        /*Reset at the start of the player's turn*/
        } else if ( conductor.measurePos >= PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PLAYER_TURN_END_THRESHOLD ) {
            if ( !doValidateTurn ) {
                playerTurn = true;
                doValidateTurn = true;
            }
        }

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;
            StartCoroutine( Bop() );
        }

        /*Commands*/
        bool tooManyInputs;

        /*Check if any of the 4 designated command buttons are pressed*/
        if ( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ) {
            timeSinceLastInput = Time.time - timeOfLastInput;

            /*Determine if commands are pressed too quickly*/
            if ( timeSinceLastInput > Conductor.MAX_INPUT_WINDOW * 2 ) {
                timeOfLastInput = Time.time;
                tooManyInputs = false;
            } else {
                tooManyInputs = true;
            }

            /*Or if 2 buttons are pressed at (approximately) the same time*/
            if ( Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.D) ) {
                StartCoroutine( ValidateCmd( 'w', inputAcc, playerTurn, tooManyInputs ) );
            } else if ( Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.D) ) {
                StartCoroutine( ValidateCmd( 'a', inputAcc, playerTurn, tooManyInputs ) );
            } else if ( Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D) ) {
                StartCoroutine( ValidateCmd( 's', inputAcc, playerTurn, tooManyInputs ) );
            } else if ( Input.GetKeyDown(KeyCode.D) && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.S) ) {
                StartCoroutine( ValidateCmd( 'd', inputAcc, playerTurn, tooManyInputs ) );
            } else {
                StartCoroutine( ValidateCmd( 'x', inputAcc, playerTurn, true ) );
            }
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

    /*
     * Takes a command entered by the user and determines whether or not to add it to the final
     * command sequence based on several factors including input accuracy, whose turn it is currently,
     * and the speed at which inputs are entered
     */
    private IEnumerator ValidateCmd( char cmd, double inputAcc, bool playerTurn, bool tooManyInputs ) {
        bool inputSuccess = false;
        
        /*Input windows*/
        float perfectMin = Conductor.MIN_PERFECT_INPUT_WINDOW;
        float perfectMax = Conductor.MAX_PERFECT_INPUT_WINDOW;
        float goodMin = Conductor.MIN_INPUT_WINDOW;
        float goodMax = Conductor.MAX_INPUT_WINDOW;

        /*Determine accuracy & validity of player input*/
        /*Perfect input*/
        if ( !tooManyInputs && playerTurn && ( inputAcc >= perfectMin || inputAcc <= perfectMax ) ) {
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
        } if ( !tooManyInputs && playerTurn && ( ( inputAcc < perfectMin && inputAcc >= goodMin ) || ( inputAcc > perfectMax && inputAcc <= goodMax ) ) ) {
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
        if ( !inputSuccess || tooManyInputs ) {
            rend.color = new Color(1f, 0.5f, 0.5f, 1f);

            /*Temp, update UI*/
            if ( !playerTurn ) {
                inputAccUI.text = "Not your turn!";
            } else if ( tooManyInputs ) {
                inputAccUI.text = "Too many inputs!";
            } else {
                if ( inputAcc >= 0.5 ) {
                    double accuracy = (1f - inputAcc) * -1;
                    inputAccUI.text = "Accuracy: " + accuracy.ToString("N5") + "\nToo early!";
                } else {
                    inputAccUI.text = "Accuracy: " + inputAcc.ToString("N5") + "\nToo late!";
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);

        rend.color = new Color(1f,1f,1f,1f); //reset color

    }
    
}
