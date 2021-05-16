using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.Entity.Troop;

namespace MusicalKnights.World {

    public class Lane : MonoBehaviour {

        // LANE STATS & PARAMETERS
        [HideInInspector] public int friendlyLaneHP = 0;
        [HideInInspector] public int maxFriendlyLaneHP = 0;
        [HideInInspector] public int enemyLaneHP = 0;
        [HideInInspector] public int maxEnemyLaneHP = 0;

        [Header("Markers")]
        public List<GameObject> markerObjList = new List<GameObject>();

        // TROOPS
        [Header("Troops")]
        public List<GameObject> activeTroopObjList = new List<GameObject>();
        [HideInInspector] public int numTroops = 0;
        [HideInInspector] public int numFriendlies = 0;
        [HideInInspector] public int numEnemies = 0;

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

