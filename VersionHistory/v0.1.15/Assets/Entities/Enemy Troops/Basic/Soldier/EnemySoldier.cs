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
    public float marchDist = 0f;
    public int currMarkerPos = 0;
    public int lane = 0;

    //LANES

    /*Parent GameObject holding the three lane objects*/
    [SerializeField] private string LANE_PARENT = "Lanes";

    GameObject laneParent;
    GameObject currLane;
    LaneInterface laneInterface;

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
        laneInterface = laneParent.GetComponent<LaneInterface>();
    }

    void Update() {

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;

            /*Follow enemy commands directly after enemy's turn*/
            if ( conductor.measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {

                /*MARCH/CHARGE*/
                if ( EnemyCommander.cmdIssued == Commands.MARCH_STR || EnemyCommander.cmdIssued == Commands.CHARGE_STR ) {
                    if ( lastBeat == 0 ) {
                        currMarkerPos += marchDistInMarkers;

                        if ( currMarkerPos >= 0 ) {
                            GameObject nextMarker = currLane.transform.GetChild(currMarkerPos).gameObject;
                            GameObject currMarker = currLane.transform.GetChild(currMarkerPos - marchDistInMarkers).gameObject;
                            marchDist = nextMarker.transform.position.x - currMarker.transform.position.x;
                            marchDist = marchDist / 4f; //divide march into x amount of steps
                        } else {
                            Destroy(gameObject); //destroy troop if at edge of the lane
                        }
                    }
                    
                    StartCoroutine( March( marchDist ) );

                /*STEADY*/
                } else if ( EnemyCommander.cmdIssued == Commands.STEADY_STR ) {
                    if ( lastBeat == 0 ) {
                        currMarkerPos += steadyDistInMarkers;

                        if ( currMarkerPos >= 0 ) {
                            GameObject nextMarker = currLane.transform.GetChild(currMarkerPos).gameObject;
                            GameObject currMarker = currLane.transform.GetChild(currMarkerPos - steadyDistInMarkers).gameObject;
                            marchDist = nextMarker.transform.position.x - currMarker.transform.position.x;
                            marchDist = marchDist / 4f; //divide march into x amount of steps
                        } else {
                            Destroy(gameObject); //destroy troop if at edge of the lane
                        }
                    }

                    StartCoroutine( March( marchDist ) );
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
    * Moves the entity to a given point to the left
    */
    private IEnumerator March( float marchTo ) {
        float nextPos = transform.position.x + marchTo;

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
