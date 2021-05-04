using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCommandStrings : MonoBehaviour {

    //UI
    [SerializeField] public GameObject commandStr;

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;


    void Update() {
        float loopPos = Conductor.instance.loopPositionInBeats;

        /*Apply input calibration is applied to measurePos and input accuracy*/
        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;

        /*On every beat*/
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;

            if ( lastBeat == 0 && ( measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                GameObject player = GameObject.Find("Player");
                GameObject str = Instantiate( commandStr, new Vector3( player.transform.position.x, player.transform.position.y + 1.2f, 0f ), player.transform.rotation );
                str.transform.SetParent(gameObject.transform);
                str.GetComponent<Text>().text = PlayerCommander.cmdIssued;
            } else if ( lastBeat == 0 && ( measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                GameObject enemy = GameObject.Find("Adversary");
                GameObject str = Instantiate( commandStr, new Vector3( enemy.transform.position.x, enemy.transform.position.y + 1.2f, 0f ), enemy.transform.rotation );
                str.transform.SetParent(gameObject.transform);
                str.GetComponent<Text>().text = EnemyCommander.cmdIssued; 
            }
        }
    }
}
