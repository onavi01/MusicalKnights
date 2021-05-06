using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandString : MonoBehaviour {
    
    Text str;

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;
    
    public bool isFriendly = false;
    private int tick = 0;

    void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;
        str = GetComponent<Text>();

        if ( isFriendly ) {
            str.text = PlayerCommander.cmdIssued;
        } else {
            str.text = EnemyCommander.cmdIssued;
        }
    }

    void Update() {

        if ( isFriendly ) {
            str.text = PlayerCommander.cmdIssued;
        } else {
            str.text = EnemyCommander.cmdIssued;
        }

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;
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
