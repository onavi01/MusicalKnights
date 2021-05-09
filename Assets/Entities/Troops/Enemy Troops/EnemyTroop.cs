using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ RequireComponent(typeof(Troop)) ]

public class EnemyTroop : MonoBehaviour {

    //INHERITED CLASSES
    Troop troop;
    
    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = 0;

    //AI STATS & PARAMETERS
    [SerializeField] public int marchDistInMarkers = 0;
    [SerializeField] public int steadyDistInMarkers = 0;
    [SerializeField] public float marchSpd = 0f;
    [SerializeField] public int atkDmg = 1;
    public float marchDist = 0f;
    public int currMarkerID = 0;
    public int lane = 0;

    [SerializeField] public int health = 0;
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

        /*Load lanes*/
        laneParent = GameObject.Find(LANE_PARENT);
        laneInterface = laneParent.GetComponent<LaneInterface>();
        currLane = laneParent.transform.GetChild(lane).gameObject;
        currLaneScript = currLane.GetComponent<Lane>();

        /*Load other components*/
        rend = GetComponent<SpriteRenderer>();

        /*Inherit superclasses*/
        troop = GetComponent<Troop>();
    }

    void Update() {

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;

            /*Follow enemy commands directly after enemy's turn*/
            if ( lastBeat == 0 && PlayerCommander.playerTurn ) {

                /*MARCH/CHARGE*/
                if ( EnemyCommander.cmdIssued == Commands.MARCH_STR || EnemyCommander.cmdIssued == Commands.CHARGE_STR ) {

                    /*Try marching to the destination marker directly*/
                    if ( troop.CanMarchToMarker( troop.currMarkerID, troop.currMarkerID + marchDistInMarkers, -1 ) ) {
                        troop.marchDist = (laneInterface.distBtwMarkers * marchDistInMarkers) / 4f;               //divide march distance into 4 steps
                        StartCoroutine( troop.MarchTo( troop.currMarkerID + marchDistInMarkers, troop.marchDist, -1 ) );

                    /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
                    } else {
                        int nextClosestMarkerID = troop.FindNextBestMarker( troop.currMarkerID, troop.currMarkerID + marchDistInMarkers, -1 );
                        int nextClosestDistInMarkers = 0;

                        if ( nextClosestMarkerID != -1 ) {
                            nextClosestDistInMarkers = nextClosestMarkerID - troop.currMarkerID;
                            troop.marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                            StartCoroutine( troop.MarchTo( troop.currMarkerID + nextClosestDistInMarkers, troop.marchDist, -1 ) );
                        }

                        /*
                        if ( !inCombat && EngagingEnemy( currMarkerID - 1 ) ) {
                            StartCoroutine( AttackMarker( currMarkerID - 1 ) );
                        }
                        */
                    }

                /*STEADY*/
                } else if ( EnemyCommander.cmdIssued == Commands.STEADY_STR ) {

                    /*Try marching to the destination marker directly*/
                    if ( troop.CanMarchToMarker( troop.currMarkerID, troop.currMarkerID + steadyDistInMarkers, -1 ) ) {
                        troop.marchDist = (laneInterface.distBtwMarkers * steadyDistInMarkers) / 4f;              //divide march distance into 4 steps
                        StartCoroutine( troop.MarchTo( troop.currMarkerID + steadyDistInMarkers, troop.marchDist, -1 ) );

                    /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
                    } else {
                        int nextClosestMarkerID = troop.FindNextBestMarker( troop.currMarkerID, troop.currMarkerID + steadyDistInMarkers, -1 );
                        int nextClosestDistInMarkers = 0;

                        if ( nextClosestMarkerID != -1 ) {
                            nextClosestDistInMarkers = nextClosestMarkerID - troop.currMarkerID;
                            troop.marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                            StartCoroutine( troop.MarchTo( troop.currMarkerID + nextClosestDistInMarkers, troop.marchDist, -1 ) );
                        }
                    }

                }
            }

        }

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
        
        /*If at opposing side, damage lane & destroy this troop*/
        } else {
            currLaneScript.DamageFriendlyLane(atkDmg);
            currLaneScript.numTroops--;
            currLaneScript.numEnemies--;
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
        GameObject adjacentMarker = currLane.transform.GetChild(adjacentMarkerID).gameObject;
        LaneMarker adjacentMarkerScript = adjacentMarker.GetComponent<LaneMarker>();
        return adjacentMarkerScript.friendlyOccupied;
    }

    private IEnumerator AttackMarker( int targetMarkerID ) {
        GameObject targetMarker = currLane.transform.GetChild(targetMarkerID).gameObject;
        LaneMarker targetMarkerScript = targetMarker.GetComponent<LaneMarker>();
        bool cueAttack = false;
        int beatTick = -1;

        inCombat = true;

        while ( true ) {
            if ( beatTick != conductor.currBeat ) {
                beatTick = conductor.currBeat;

                if ( cueAttack ) {
                    GameObject target = targetMarkerScript.occupyingEntity;
                    FriendlyTroop friendly = target.GetComponent<FriendlyTroop>();
                    StartCoroutine( friendly.TakeDamage(atkDmg) );
                    conductor.musicSources[0].PlayOneShot(conductor.metronome, 0.4f);

                    if ( friendly.health <= 0 ) {
                        targetMarkerScript.isOccupied = false;
                        targetMarkerScript.friendlyOccupied = false;
                        targetMarkerScript.occupyingEntity = null;

                        currLaneScript.numTroops--;
                        currLaneScript.numFriendlies--;

                        inCombat = false;

                        Destroy( target );
                        yield break;
                    }
                }

                if ( !cueAttack && beatTick == 3 ) {
                    conductor.musicSources[0].PlayOneShot(conductor.metronome, 0.65f);
                    cueAttack = true;
                }
            }

            yield return null;
        }
    }

    public IEnumerator TakeDamage( int dmg ) {
        health -= dmg;

        Color originalColor = rend.color;
        rend.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        rend.color = originalColor;

        yield return null;
    }

}
