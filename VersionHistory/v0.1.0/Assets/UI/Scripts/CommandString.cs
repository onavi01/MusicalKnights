using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandString : MonoBehaviour {
    Text str;

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;
    private int tick = 0;

    void Start() {
        str = GetComponent<Text>();
    }

    void Update() {
        float loopPos = Conductor.instance.loopPositionInBeats;

        /*Apply input calibration is applied to measurePos and input accuracy*/
        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;

        /*On every beat*/
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;
            tick++;

            StartCoroutine( Fade() );

            /*Destroy after fading out completely*/
            if ( tick == 5 ) {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator Fade() {
        float opacity = str.color.a - 0.25f;

        while ( str.color.a > opacity ) {
            str.color += new Color(1f, 1f, 1f, -0.04f);
            yield return null;
        }
    }
}
