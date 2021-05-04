using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Border : MonoBehaviour {

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;

    //RENDERING
    private Shader shaderGUIText; //renders border sprite using GUI Text Shader to allow for easy colouration
    [SerializeField] public float minOpacity;
    [SerializeField] public float maxOpacity;

    private void Start() {
        shaderGUIText = Shader.Find("GUI/Text Shader"); //load GUI Text Shader
        GetComponent<SpriteRenderer>().material.shader = shaderGUIText;
        GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, minOpacity ); //set to black
    }

    private void Update() {
        float loopPos = Conductor.instance.loopPositionInBeats;
        currBeat = (int) ( loopPos - Conductor.instance.playerInputOffset ) % 4; //retrieve current beat in song & apply player input offset
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;
        
        /*On every beat*/
        if ( lastBeat != currBeat ) {
            bool playerTurn = true;
            lastBeat = currBeat;

            /*Detect if player's turn*/
            if ( measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {
                playerTurn = false;
            }

            StartCoroutine( FlashBorder(playerTurn) );
        }
    }

    private IEnumerator FlashBorder( bool playerTurn ) {
        float opacity = maxOpacity;

        /*Set border to max opacity & set color to gray if not player's turn*/
        if ( playerTurn ) {
            GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, opacity );
        } else {
            GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 0.5f, opacity );
        }

        yield return new WaitForSeconds(0.05f);

        /*Fade border back to min opacity*/
        while ( opacity > minOpacity ) {
            opacity -= 0.02f;
            
            if ( playerTurn ) {
                GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, opacity );
            } else {
                GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 0.5f, opacity );
            }

            yield return null;
        }
    }
}
