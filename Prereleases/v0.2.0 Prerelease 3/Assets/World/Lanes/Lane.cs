using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour {
    //LANE STATS & PARAMETERS
    public int friendlyLaneHP = 0, maxFriendlyLaneHP = 0;
    public int enemyLaneHP = 0, maxEnemyLaneHP = 0;
    public int numTroops = 0, numFriendlies = 0, numEnemies = 0;

    public void DamageFriendlyLane( int dmg ) {
        if ( friendlyLaneHP - dmg >= 0 ) {
            friendlyLaneHP -= dmg;
        } else {
            friendlyLaneHP = 0;
        }
    }

    public void DamageEnemyLane( int dmg ) {
        if ( enemyLaneHP - dmg >= 0 ) {
            enemyLaneHP -= dmg;
        } else {
            enemyLaneHP = 0;
        }
    }
}