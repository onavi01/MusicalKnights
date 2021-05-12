using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.Entity.Troop;

namespace MusicalKnights.World {

    public class Lane : MonoBehaviour {

        //LANE STATS & PARAMETERS
        public int friendlyLaneHP = 0, maxFriendlyLaneHP = 0;
        public int enemyLaneHP = 0, maxEnemyLaneHP = 0;
        public int numTroops = 0, numFriendlies = 0, numEnemies = 0;

        /*
        * Damages the lane on the specified side
        */
        public void DamageLane( int alliance, int dmg ) {

            if ( alliance == (int) Troop.Alliance.FOE ) {
                if ( friendlyLaneHP - dmg >= 0 ) {
                    friendlyLaneHP -= dmg;
                } else {
                    friendlyLaneHP = 0;
                }

            } else if ( alliance == (int) Troop.Alliance.FRIEND ) {
                if ( enemyLaneHP - dmg >= 0 ) {
                    enemyLaneHP -= dmg;
                } else {
                    enemyLaneHP = 0;
                }
            }

        }

    }

}

