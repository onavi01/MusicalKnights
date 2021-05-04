using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCommandStrings : MonoBehaviour {

    //UI
    [SerializeField] public GameObject commandStr;

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;

    void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;
    }

    void Update() {
        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;

            /*Spawn command strings above respective commander's head*/
            if ( lastBeat == 0 && ( conductor.measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || conductor.measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                GameObject player = GameObject.Find("Player");
                GameObject str = Instantiate( commandStr, new Vector3( player.transform.position.x, player.transform.position.y + 1.2f, 0f ), player.transform.rotation );
                str.transform.SetParent(gameObject.transform);
                str.GetComponent<Text>().text = PlayerCommander.cmdIssued;
            } else if ( lastBeat == 0 && ( conductor.measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                GameObject enemy = GameObject.Find("Adversary");
                GameObject str = Instantiate( commandStr, new Vector3( enemy.transform.position.x, enemy.transform.position.y + 1.2f, 0f ), enemy.transform.rotation );
                str.transform.SetParent(gameObject.transform);
                str.GetComponent<Text>().text = EnemyCommander.cmdIssued; 
            }
        }
    }
}
