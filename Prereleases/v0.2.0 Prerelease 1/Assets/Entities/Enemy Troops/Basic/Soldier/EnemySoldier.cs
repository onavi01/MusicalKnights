using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoldier : MonoBehaviour {
    
    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = 0;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //AI STATS & PARAMETERS
    [SerializeField] public int marchDistInMarkers = 0;
    [SerializeField] public int steadyDistInMarkers = 0;
    [SerializeField] public float marchSpd = 0f;
    [SerializeField] public int atkDmg = 1;
    public float marchDist = 0f;
    public int currMarkerID = 0;
    public int lane = 0;
    public bool inCombat = false;

    //LANES

    /*Parent GameObject holding the three lane objects*/
    [SerializeField] private string LANE_PARENT = "Lanes";

    private GameObject laneParent;
    private LaneInterface laneInterface;

    public GameObject currLane;
    public Lane currLaneScript;
    
    public GameObject currMarker;

    //RENDERING
    private SpriteRenderer rend;
    
    void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;

        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        /*Load lanes*/
        laneParent = GameObject.Find(LANE_PARENT);
        currLane = laneParent.transform.GetChild(lane).gameObject;
        currLaneScript = currLane.GetComponent<Lane>();
        laneInterface = laneParent.GetComponent<LaneInterface>();
    }

    void Update() {

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;

            /*Follow enemy commands directly after enemy's turn*/
            if ( lastBeat == 0 && ( conductor.measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {

                /*MARCH/CHARGE*/
                if ( EnemyCommander.cmdIssued == Commands.MARCH_STR || EnemyCommander.cmdIssued == Commands.CHARGE_STR ) {

                    /*Try marching to the destination marker directly*/
                    if ( CanMarchToMarker( currMarkerID, currMarkerID + marchDistInMarkers ) ) {
                        marchDist = (laneInterface.distBtwMarkers * marchDistInMarkers) / 4f;               //divide march distance into 4 steps
                        StartCoroutine( MarchTo( currMarkerID + marchDistInMarkers, marchDist ) );

                    /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
                    } else {
                        int nextClosestMarkerID = FindNextBestMarker( currMarkerID, currMarkerID + marchDistInMarkers );
                        int nextClosestDistInMarkers = 0;

                        if ( nextClosestMarkerID != -1 ) {
                            nextClosestDistInMarkers = nextClosestMarkerID - currMarkerID;
                            marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                            StartCoroutine( MarchTo( currMarkerID + nextClosestDistInMarkers, marchDist ) );
                        }

                        if ( !inCombat && EngagingEnemy( currMarkerID - 1 ) ) {
                            StartCoroutine( AttackMarker( currMarkerID - 1 ) );
                        }
                    }

                /*STEADY*/
                } else if ( EnemyCommander.cmdIssued == Commands.STEADY_STR ) {

                    /*Try marching to the destination marker directly*/
                    if ( CanMarchToMarker( currMarkerID, currMarkerID + steadyDistInMarkers ) ) {
                        marchDist = (laneInterface.distBtwMarkers * steadyDistInMarkers) / 4f;              //divide march distance into 4 steps
                        StartCoroutine( MarchTo( currMarkerID + steadyDistInMarkers, marchDist ) );

                    /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
                    } else {
                        int nextClosestMarkerID = FindNextBestMarker( currMarkerID, currMarkerID + marchDistInMarkers );
                        int nextClosestDistInMarkers = 0;

                        if ( nextClosestMarkerID != -1 ) {
                            nextClosestDistInMarkers = nextClosestMarkerID - currMarkerID;
                            marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                            StartCoroutine( MarchTo( currMarkerID + nextClosestDistInMarkers, marchDist ) );
                        }
                    }

                }
            }
            
            StartCoroutine( Bop() );

        }

    }

    private IEnumerator Bop() {
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

    /*
    * Checks if the markers leading up to and including a destination marker to the left are free 
    * (i.e. the troop can reach the destination marker)
    */
    private bool CanMarchToMarker( int currMarkerID, int nextMarkerID ) {

        /*Avoid searching beyond the lane*/
        if ( nextMarkerID < 0 ) {
            nextMarkerID = 0;
        }

        /*Check all markers leading up to the next position*/
        for ( int i = currMarkerID - 1; i >= nextMarkerID; i-- ) {
            GameObject marker = laneParent.transform.GetChild(lane).gameObject.transform.GetChild(i).gameObject;
            LaneMarker markerScript = marker.GetComponent<LaneMarker>();

            /*If any markers are blocked*/
            if ( markerScript.isOccupied ) {
                return false;
            }
        }

        return true;
    }

    /*
    * Finds the next unoccupied marker from a destination marker to the left
    * Returns -1 if none available
    */
    private int FindNextBestMarker( int currMarkerID, int targetMarkerID ) { 

        /*Avoid going beyond the lane*/
        if ( targetMarkerID < 0 ) {
            targetMarkerID = 0;
        }

        /*Check all markers between the troop's current position and the target destination*/
        for ( int i = targetMarkerID + 1; i < currMarkerID; i++ ) {
            GameObject marker = laneParent.transform.GetChild(lane).gameObject.transform.GetChild(i).gameObject;
            LaneMarker markerScript = marker.GetComponent<LaneMarker>();

            /*Return furthest vacant marker*/
            if ( !markerScript.isOccupied ) {
                return i;
            }
        }

        return -1; //already at closest vacant marker
        
    }

    /*
    * Moves the troop the specified distance to the left & updates lane marker occupancy
    */
    private IEnumerator MarchTo( int nextMarkerID, float nextMarkerDist ) {
        LaneMarker currMarkerScript = currMarker.GetComponent<LaneMarker>();
        int beatTick = -1;

        /*Vacate & update previous marker*/
        currMarkerScript.isOccupied = false;
        currMarkerScript.friendlyOccupied = false;
        currMarkerScript.occupyingEntity = null;

        /*Occupy & update next marker if not already at opposing edge*/
        if ( nextMarkerID >= 0 ) {
            GameObject nextMarker = laneParent.transform.GetChild(lane).gameObject.transform.GetChild(nextMarkerID).gameObject;
            LaneMarker nextMarkerScript = nextMarker.GetComponent<LaneMarker>();
            
            currMarker = nextMarker;
            currMarkerID = nextMarkerID;
            
            nextMarkerScript.isOccupied = true;
            nextMarkerScript.friendlyOccupied = false;
            nextMarkerScript.occupyingEntity = gameObject;

            /*March to the right on every beat*/
            while ( true ) {
                if ( beatTick != conductor.currBeat ) {
                    beatTick = conductor.currBeat;
                    StartCoroutine( March(nextMarkerDist) );
                }

                /*Stop after 4 steps*/
                if ( beatTick == 3 ) {
                    break;
                }

                yield return null;
            }
        
        /*If at opposing edge, damage lane & destroy this troop*/
        } else {
            currLaneScript.DamageFriendlyLane(atkDmg);
            Destroy(gameObject);
            yield break; //exit coroutine
        }
    }

    /*
    * Moves the entity to a given point to the left
    */
    private IEnumerator March( float dist ) {
        float nextPos = transform.position.x + dist;

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

    private bool EngagingEnemy( int adjacentMarkerID ) {
        GameObject adjacentMarker = laneParent.transform.GetChild(lane).gameObject.transform.GetChild(adjacentMarkerID).gameObject;
        LaneMarker adjacentMarkerScript = adjacentMarker.GetComponent<LaneMarker>();
        return adjacentMarkerScript.friendlyOccupied;
    }

    private IEnumerator AttackMarker( int targetMarkerID ) {
        GameObject targetMarker = laneParent.transform.GetChild(lane).gameObject.transform.GetChild(targetMarkerID).gameObject;
        LaneMarker targetMarkerScript = targetMarker.GetComponent<LaneMarker>();
        bool cueAttack = false;
        int beatTick = -1;

        inCombat = true;

        while ( true ) {
            if ( beatTick != conductor.currBeat ) {
                beatTick = conductor.currBeat;

                if ( cueAttack ) {
                    Destroy( targetMarkerScript.occupyingEntity );
                    targetMarkerScript.isOccupied = false;
                    targetMarkerScript.friendlyOccupied = false;
                    targetMarkerScript.occupyingEntity = null;
                    inCombat = false;
                    yield break;
                }

                if ( !cueAttack && beatTick == 3 ) {
                    conductor.musicSources[0].PlayOneShot(conductor.metronome, 0.65f);
                    cueAttack = true;
                }
            }

            yield return null;
        }
    }

}
