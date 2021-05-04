using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneInterface : MonoBehaviour {

    //MARKERS
    [SerializeField] private GameObject laneMarker;

    /*
    * The number of locations troops can stop at while marching across each lane;
    * MUST be at least 2 to account for the markers at the edges of the screen
    */
    [SerializeField] public int numMarkers = 2; 
    
    //LANES
    private GameObject[] lanes = new GameObject[3];
    private BoxCollider2D[] laneCols = new BoxCollider2D[3];

    void Start() {
        /*Error checking*/
        if ( numMarkers < 2 ) {
            numMarkers = 2;
        }

        /*Load lane objects*/
        for ( int i = 0; i < 3; i++ ) {
            lanes[i] = transform.GetChild(i).gameObject;
            laneCols[i] = lanes[i].GetComponent<BoxCollider2D>();
            PlaceMarkers(lanes[i], laneCols[i]); //spawn markers
        }
    }

    /*Places a variable amount of lane markers evenly across a given lane*/
    private void PlaceMarkers( GameObject lane, BoxCollider2D laneCol ) {
        float leftLaneEdge = laneCol.bounds.center.x - laneCol.bounds.extents.x;
        float rightLaneEdge = laneCol.bounds.center.x + laneCol.bounds.extents.x;
        float distBetweenMarkers = (rightLaneEdge - leftLaneEdge) / (numMarkers - 1); //if we are placing x markers, then there are x-1 total gaps across the lane
        float markerX = 0f;
        
        int markersPlaced = 0;

        while ( markersPlaced < numMarkers ) {
            GameObject marker = Instantiate( laneMarker, new Vector3(leftLaneEdge + markerX, laneCol.bounds.center.y, 0f), lane.transform.rotation );
            marker.transform.parent = lane.transform;                   //create marker object as a child of its parent lane
            marker.GetComponent<LaneMarker>().markerID = markersPlaced; //designate marker ID relative to its lane
            
            markerX += distBetweenMarkers;
            markersPlaced++;
        }
    }

}