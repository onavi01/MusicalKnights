using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneHPString : MonoBehaviour {

    [SerializeField] public int lane = 0;
    [SerializeField] public bool friendlySide = false;
    private Lane currLane;
    private Text textCmpnt;

    void Start() {
        currLane = GameObject.Find("Lanes").transform.GetChild(lane).gameObject.GetComponent<Lane>();
        textCmpnt = GetComponent<Text>();
        textCmpnt.text = "Lane HP: 0/0";
    }

    void Update() {
        if ( friendlySide ) {
            textCmpnt.text = "Lane HP: " + currLane.friendlyLaneHP + "/" + currLane.maxFriendlyLaneHP;
        } else {
            textCmpnt.text = "Lane HP: " + currLane.enemyLaneHP + "/" + currLane.maxEnemyLaneHP;
        }
    }
}
