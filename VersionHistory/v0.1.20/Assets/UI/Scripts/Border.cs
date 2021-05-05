using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Border : MonoBehaviour {

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;

    //RENDERING
    private Shader shaderGUIText; //renders border sprite using GUI Text Shader to allow for easy colouration
    [SerializeField] public float minOpacity;
    [SerializeField] public float maxOpacity;

    private void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;

        /*Rendering*/
        shaderGUIText = Shader.Find("GUI/Text Shader"); //load GUI Text Shader
        GetComponent<SpriteRenderer>().material.shader = shaderGUIText;
        GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, minOpacity ); //set to black
    }

    private void Update() {
        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            bool playerTurn = true;
            lastBeat = conductor.currBeat;

            /*Detect if player's turn*/
            if ( conductor.measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || conductor.measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {
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
