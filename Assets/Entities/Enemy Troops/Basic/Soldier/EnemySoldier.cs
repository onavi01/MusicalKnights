using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoldier : MonoBehaviour {
    
    //BEAT TIMER
    private int currBeat = 0, lastBeat = 0;
    private float beatPos = 0f, measurePos = 0f;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //AI STATS & PARAMETERS
    [SerializeField] public float marchSpd = 0f;
    [SerializeField] public int marchDistInMarkers = 0;
    public float marchDist = 0f;
    public int currMarkerPos = 0;

    //LANES
    LaneInterface laneInterface;

    //RENDERING
    private SpriteRenderer rend;
    
    void Start() {
        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;
        rend = GetComponent<SpriteRenderer>();

        laneInterface = GameObject.Find("Lanes").GetComponent<LaneInterface>();
        currMarkerPos = laneInterface.numMarkers - 1;
    }

    void Update() {
        
        float loopPos = Conductor.instance.loopPositionInBeats;
        
        /*Apply input calibration is applied to measurePos and input accuracy*/
        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;
        beatPos = (loopPos - Conductor.instance.playerInputOffset) % 1;

        //On every beat
        if ( lastBeat != currBeat ) {
            lastBeat = currBeat;

            /*Follow enemy commands directly after enemy's turn*/
            if ( measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) {

                /*MARCH/CHARGE*/
                if ( EnemyCommander.cmdIssued == Commands.MARCH_STR || EnemyCommander.cmdIssued == Commands.CHARGE_STR ) {
                    if ( lastBeat == 0 ) {
                        currMarkerPos += marchDistInMarkers;

                        if ( currMarkerPos >= 0 ) {
                            marchDist = GameObject.Find("Lanes").transform.GetChild(0).gameObject.transform.GetChild(currMarkerPos).gameObject.transform.position.x - GameObject.Find("Lanes").transform.GetChild(0).gameObject.transform.GetChild(currMarkerPos - marchDistInMarkers).gameObject.transform.position.x;
                            marchDist = marchDist / 4f;
                        } else {
                            Destroy(gameObject);
                        }
                    }
                    
                    StartCoroutine( March( marchDist ) );

                /*STEADY*/
                } else if ( EnemyCommander.cmdIssued == Commands.STEADY_STR ) {
                    if ( lastBeat == 0 ) {
                        currMarkerPos += -1;

                        if ( currMarkerPos >= 0 ) {
                            marchDist = GameObject.Find("Lanes").transform.GetChild(0).gameObject.transform.GetChild(currMarkerPos).gameObject.transform.position.x - GameObject.Find("Lanes").transform.GetChild(0).gameObject.transform.GetChild(currMarkerPos + 1).gameObject.transform.position.x;
                            marchDist = marchDist / 4f;
                        } else {
                            Destroy(gameObject);
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

    private IEnumerator March( float marchTo ) {
        float nextPos = transform.position.x + marchTo;

        while ( transform.position.x > nextPos ) {
            if ( ( transform.position.x + marchSpd ) >= nextPos ) {
                transform.position += new Vector3( marchSpd, 0f, 0f );
            } else {
                transform.position += new Vector3( nextPos - transform.position.x, 0f, 0f ); 
            }
            
            yield return null;
        }
    }
}
