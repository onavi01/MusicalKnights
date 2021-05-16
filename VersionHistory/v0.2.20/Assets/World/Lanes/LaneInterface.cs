using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class LaneInterface : MonoBehaviour, IInspectorEditable {

        // LANES
        [HideInInspector] public GameObject[] laneObjs = new GameObject[3];
        [HideInInspector] public Lane[] lanes = new Lane[3];
        [HideInInspector] public BoxCollider2D[] laneColliders = new BoxCollider2D[3];
        [HideInInspector] public float distBtwMarkers = 0f;

        [Header("Lane Settings")]
        public int STARTING_FRIENDLY_LANE_HP = 0;
        public int STARTING_ENEMY_LANE_HP = 0;

        [Header("Marker Settings")]
        public GameObject markerFab;

        //MUST be at least 2 to account for the markers at the edges of the screen
        public int numMarkers = 2;

        private void Awake() {
            ValidateProperties();

            //Initialize lanes
            for ( int i = 0; i < 3; i++ ) {
                laneObjs[i] = transform.GetChild(i).gameObject;
                laneColliders[i] = laneObjs[i].GetComponent<BoxCollider2D>();

                //Initialize lane stats & parameters
                lanes[i] = laneObjs[i].GetComponent<Lane>();
                
                lanes[i].friendlyLaneHP = STARTING_FRIENDLY_LANE_HP;
                lanes[i].maxFriendlyLaneHP = STARTING_FRIENDLY_LANE_HP;
                lanes[i].enemyLaneHP = STARTING_ENEMY_LANE_HP;
                lanes[i].maxEnemyLaneHP = STARTING_ENEMY_LANE_HP;

                SpawnMarkers(i);
            }
        }

        public void ValidateProperties() {
            if ( numMarkers < 2 ) {
                numMarkers = 2;
            }
        }

        /*
        * Places a variable amount of lane markers evenly across a given lane
        */
        private void SpawnMarkers( int laneHeight ) {
            float leftLaneEdge = laneColliders[laneHeight].bounds.center.x - laneColliders[laneHeight].bounds.extents.x;
            float rightLaneEdge = laneColliders[laneHeight].bounds.center.x + laneColliders[laneHeight].bounds.extents.x;

            //Note that if x markers need to be placed, then there are x-1 total gaps across the lane
            distBtwMarkers = (rightLaneEdge - leftLaneEdge) / (numMarkers - 1);

            int markersPlaced = 0;
            float nextMarkerXPos = 0f;

            while ( markersPlaced < numMarkers ) {
                GameObject markerObj = Instantiate( markerFab, new Vector3(leftLaneEdge + nextMarkerXPos, laneColliders[laneHeight].bounds.center.y, 0f), laneObjs[laneHeight].transform.rotation );
                LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                markerObj.transform.SetParent(laneObjs[laneHeight].transform);
                marker.markerID = markersPlaced;
                nextMarkerXPos += distBtwMarkers;

                lanes[laneHeight].markerObjList.Add(markerObj);

                markersPlaced++;
            }
        }

    }
    
}

