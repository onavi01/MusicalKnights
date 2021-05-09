using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour {

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;

    //BOP (TEMP)
    private SpriteRenderer rend;
    private float originScale = 0;
    private float bopMin = 0;

    //TROOP PARAMETERS & AI
    public enum Alliance {
        FRIEND,
        FOE
    };

    [SerializeField] public int alliance = 0;
    [SerializeField] public float marchSpd = 0;
    [SerializeField] public int atkDmg = 1;
    public float marchDist = 0f;

    //LANES
    /*Parent GameObject holding the three lane objects*/
    [SerializeField] private string LANE_PARENT = "Lanes";

    private GameObject laneParent;
    public GameObject currLaneObj;
    public GameObject currMarkerObj;
    private LaneInterface laneInterface;
    public Lane currLane;
    public LaneMarker currMarker;

    public int lane = 0;
    public int currMarkerID = 0;

    void Start() {

        /*Load Conductor object*/
        conductor = Conductor.instance;

        /*Rendering*/
        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        /*Load lane-related*/
        laneParent = GameObject.Find(LANE_PARENT);
        laneInterface = laneParent.GetComponent<LaneInterface>();
        currLaneObj = laneInterface.lanes[lane];
        currLane = currLaneObj.GetComponent<Lane>();
        currMarkerObj = currLaneObj.transform.GetChild(currMarkerID).gameObject;
        currMarker = currMarkerObj.GetComponent<LaneMarker>();

    }

    void Update() {
        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;
            StartCoroutine( Bop() );
        }
    }

    /*
    * Checks if the markers leading up to and including a destination are free 
    * (i.e. checks if the troop can reach the destination marker)
    */
    public bool CanMarchToMarker( int currMarkerID, int nextMarkerID, int direction ) {

        /*Check markers to the right*/
        if ( direction == 1 ) {
            /*Avoid searching beyond the lane*/
            if ( nextMarkerID >= laneInterface.numMarkers ) {
                nextMarkerID = laneInterface.numMarkers - 1;
            }

            /*Check all markers leading up to the next position*/
            for ( int i = currMarkerID + 1; i <= nextMarkerID; i++ ) {
                GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                /*If any markers are blocked*/
                if ( marker.isOccupied ) {
                    return false;
                }
            }

        /*Check markers to the left*/
        } else if ( direction == -1 ) {
            /*Avoid searching beyond the lane*/
            if ( nextMarkerID < 0 ) {
                nextMarkerID = 0;
            }

            /*Check all markers leading up to the next position*/
            for ( int i = currMarkerID - 1; i >= nextMarkerID; i-- ) {
                GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                /*If any markers are blocked*/
                if ( marker.isOccupied ) {
                    return false;
                }
            }
        }
        
        return true;
        
    }

    /*
    * Finds the closest unoccupied marker from a destination marker
    * Returns -1 if none available
    */
    public int FindNextBestMarker( int currMarkerID, int targetMarkerID, int direction ) {

        /*Check markers to the right*/
        if ( direction == 1 ) {
            /*Avoid searching beyond the lane*/
            if ( targetMarkerID >= laneInterface.numMarkers ) {
                targetMarkerID = laneInterface.numMarkers - 1;
            }

            /*Check all markers between the troop's current position and the target destination*/
            for ( int i = targetMarkerID - 1; i > currMarkerID; i-- ) {
                GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                /*Return furthest vacant marker*/
                if ( !marker.isOccupied ) {
                    return i;
                }
            }
        
        /*Check markers to the left*/
        } else if ( direction == -1 ) {
            /*Avoid searching beyond the lane*/
            if ( targetMarkerID < 0 ) {
                targetMarkerID = 0;
            }

            /*Check all markers between the troop's current position and the target destination*/
            for ( int i = targetMarkerID + 1; i < currMarkerID; i++ ) {
                GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                /*Return furthest vacant marker*/
                if ( !marker.isOccupied ) {
                    return i;
                }
            }
        }

        return -1; //already at closest vacant marker
        
    }

    /*
    * Moves the troop the specified distance to the left or right & updates lane marker occupancy
    */
    public IEnumerator MarchTo( int nextMarkerID, float nextMarkerDist, int direction ) {

        int beatTick = -1;

        /*Vacate & update previous marker*/
        currMarker.isOccupied = false;
        currMarker.friendlyOccupied = false;
        currMarker.occupyingEntity = null;

        /*If at or beyond opposition's side, damage lane & destroy this troop*/
        if ( ( direction == 1 && nextMarkerID >= laneInterface.numMarkers ) || ( direction == -1 && nextMarkerID < 0 ) ) {
            currLane.numTroops--;

            if ( alliance == (int) Alliance.FRIEND ) {
                currLane.DamageEnemyLane(atkDmg);
                currLane.numFriendlies--;
            } else if ( alliance == (int) Alliance.FOE ) {
                currLane.DamageFriendlyLane(atkDmg);
                currLane.numEnemies--;
            }

            Destroy(gameObject);
            yield break; //exit coroutine
        }

        GameObject nextMarkerObj = currLane.transform.GetChild(nextMarkerID).gameObject;
        LaneMarker nextMarker = nextMarkerObj.GetComponent<LaneMarker>();
        
        /*Occupy destination marker*/
        currMarkerID = nextMarkerID;
        currMarkerObj = nextMarkerObj;
        currMarker = currMarkerObj.GetComponent<LaneMarker>();
        
        nextMarker.isOccupied = true;
        nextMarker.occupyingEntity = gameObject;

        if ( alliance == (int) Alliance.FRIEND ) {
            nextMarker.friendlyOccupied = true;
        } else if ( alliance == (int) Alliance.FOE ) {
            nextMarker.friendlyOccupied = false;
        }

        /*Move closer to the marker on every beat*/
        while ( true ) {
            if ( beatTick != conductor.currBeat ) {
                beatTick = conductor.currBeat;
                StartCoroutine( March( nextMarkerDist, direction ) );
            }

            /*Stop after 4 steps*/
            if ( beatTick == 3 ) {
                yield break;
            }

            yield return null;
        }
        
    }

    /*
    * Moves the troop a given distance to the left or right
    */
    public IEnumerator March( float dist, int direction ) {
        float nextPos = transform.position.x + dist;

        /*Move to the right*/
        if ( direction == 1 ) {
            while ( transform.position.x < nextPos ) {

                /*Avoid going beyond the specified distance*/
                if ( ( transform.position.x + marchSpd ) <= nextPos ) {
                    transform.position += new Vector3( marchSpd, 0f, 0f );
                } else {
                    transform.position += new Vector3( nextPos - transform.position.x, 0f, 0f ); 
                }
                
                yield return null;
            }

        /*Move to the left*/
        } else if ( direction == -1 ) {
            while ( transform.position.x > nextPos ) {

                /*Avoid going beyond the specified distance*/
                if ( ( transform.position.x + marchSpd ) >= nextPos ) {
                    transform.position += new Vector3( marchSpd, 0f, 0f );
                } else {
                    transform.position += new Vector3( nextPos - transform.position.x, 0f, 0f ); 
                }
                
                yield return null;
            }
        }
    }

    /*Temporary function*/
    public IEnumerator Bop() {
        /*Squish*/
        while ( transform.localScale.y > bopMin ) {
            transform.localScale += new Vector3( 0, -0.03f, 0);
            yield return null;
        }

        /*Stretch*/
        while ( transform.localScale.y < originScale ) {
            transform.localScale += new Vector3( 0, 0.03f, 0);
            yield return null;
        }
    }
}
