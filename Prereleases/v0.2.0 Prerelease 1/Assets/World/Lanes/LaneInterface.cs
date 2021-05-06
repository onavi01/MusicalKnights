using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneInterface : MonoBehaviour {

    //LANES
    private GameObject[] lanes = new GameObject[3];             //lane game objects
    private BoxCollider2D[] laneCols = new BoxCollider2D[3];    //lane box colliders
    public float distBtwMarkers = 0f;

    [SerializeField] public int friendlyLaneHP = 0;
    [SerializeField] public int enemyLaneHP = 0;

    //MARKERS
    [SerializeField] private GameObject laneMarker;

    /*
    * The number of locations troops can stop at while marching across each lane
    * MUST be at least 2 to account for the markers at the edges of the screen
    */
    [SerializeField] public int numMarkers = 2;

    void Start() {
        /*Error checking*/
        if ( numMarkers < 2 ) {
            numMarkers = 2;
        }

        /*Load lane objects*/
        for ( int i = 0; i < 3; i++ ) {
            lanes[i] = transform.GetChild(i).gameObject;
            laneCols[i] = lanes[i].GetComponent<BoxCollider2D>();

            /*Initialize lane stats & parameters*/
            Lane lane = lanes[i].GetComponent<Lane>();
            
            lane.friendlyLaneHP = friendlyLaneHP;
            lane.maxFriendlyLaneHP = friendlyLaneHP;
            lane.enemyLaneHP = enemyLaneHP;
            lane.maxEnemyLaneHP = enemyLaneHP;

            PlaceMarkers(lanes[i], laneCols[i]); //spawn markers
        }
    }

    /*Places a variable amount of lane markers evenly across a given lane*/
    private void PlaceMarkers( GameObject lane, BoxCollider2D laneCol ) {
        float leftLaneEdge = laneCol.bounds.center.x - laneCol.bounds.extents.x;
        float rightLaneEdge = laneCol.bounds.center.x + laneCol.bounds.extents.x;
        distBtwMarkers = (rightLaneEdge - leftLaneEdge) / (numMarkers - 1); //if we are placing x markers, then there are x-1 total gaps across the lane

        float markerX = 0f;
        
        int markersPlaced = 0;

        while ( markersPlaced < numMarkers ) {
            GameObject marker = Instantiate( laneMarker, new Vector3(leftLaneEdge + markerX, laneCol.bounds.center.y, 0f), lane.transform.rotation );
            LaneMarker markerScript = marker.GetComponent<LaneMarker>();
            marker.transform.SetParent(lane.transform);     //create marker object as a child of its parent lane
            markerScript.markerID = markersPlaced;          //designate marker ID relative to its lane
            markerScript.friendlyOccupied = false;
            
            markerX += distBtwMarkers;
            markersPlaced++;
        }
    }

}