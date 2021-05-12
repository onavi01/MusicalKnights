using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Troop {

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

        /*Negative distances will move to the left*/
        [SerializeField] public int marchDistInMarkers = 0;
        [SerializeField] public int steadyDistInMarkers = 0;

        [SerializeField] [HideInInspector] public int alliance = 0;
        [SerializeField] public float marchSpd = 0;
        [SerializeField] public int health = 1;
        [SerializeField] public int atkDmg = 1;
        [HideInInspector] public float marchDist = 0f;
        [HideInInspector] public bool inCombat = false;

        //LANES
        /*Parent GameObject holding the three lane objects*/
        [SerializeField] private string LANE_PARENT = "Lanes";

        [HideInInspector] private GameObject laneParent;
        [HideInInspector] public GameObject currLaneObj;
        [HideInInspector] public GameObject currMarkerObj;
        [HideInInspector] private LaneInterface laneInterface;
        [HideInInspector] public Lane currLane;
        [HideInInspector] public LaneMarker currMarker;

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
        * Troop default behaviour when following their commander's 'March' command
        */
        public void MarchDefault( int direction ) {
            /*Try marching to the destination marker directly*/
            if ( CanMarchToMarker( currMarkerID, currMarkerID + marchDistInMarkers, direction ) ) {
                marchDist = (laneInterface.distBtwMarkers * marchDistInMarkers) / 4f;               //divide march distance into 4 steps
                StartCoroutine( MarchTo( currMarkerID + marchDistInMarkers, marchDist, direction ) );

            /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
            } else {
                int nextClosestMarkerID = FindNextBestMarker( currMarkerID, currMarkerID + marchDistInMarkers, direction );
                int nextClosestDistInMarkers = 0;

                if ( nextClosestMarkerID != -1 ) {
                    nextClosestDistInMarkers = nextClosestMarkerID - currMarkerID;
                    marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                    StartCoroutine( MarchTo( currMarkerID + nextClosestDistInMarkers, marchDist, direction ) );
                }

                /*If marching/charging towards an enemy*/
                if ( !inCombat && IsEngagingOpponent( currMarkerID + direction ) ) {
                    StartCoroutine( AttackOpponent( currMarkerID + direction ) );
                }
            }
        }

        /*
        * Troop default behaviour when following their commander's 'Steady' command
        */
        public void SteadyDefault( int direction ) {
            /*Try marching to the destination marker directly*/
            if ( CanMarchToMarker( currMarkerID, currMarkerID + steadyDistInMarkers, direction ) ) {
                marchDist = (laneInterface.distBtwMarkers * steadyDistInMarkers) / 4f;              //divide march distance into 4 steps
                StartCoroutine( MarchTo( currMarkerID + steadyDistInMarkers, marchDist, direction ) );

            /*Try marching to the closest unoccupied marker from the destination marker, if there are any*/
            } else {
                int nextClosestMarkerID = FindNextBestMarker( currMarkerID, currMarkerID + steadyDistInMarkers, direction );
                int nextClosestDistInMarkers = 0;

                if ( nextClosestMarkerID != -1 ) {
                    nextClosestDistInMarkers = nextClosestMarkerID - currMarkerID;
                    marchDist = (laneInterface.distBtwMarkers * nextClosestDistInMarkers) / 4f;     //divide march distance into 4 steps
                    StartCoroutine( MarchTo( currMarkerID + nextClosestDistInMarkers, marchDist, direction ) );
                }
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

            return -1; //already at closest unoccupied marker
            
        }

        /*
        * Moves the troop the specified distance to the left or right & updates lane marker occupancy
        */
        public IEnumerator MarchTo( int nextMarkerID, float nextMarkerDist, int direction ) {

            int beatTick = -1;

            /*If at or beyond opposition's side, damage lane & destroy this troop*/
            if ( ( direction == 1 && nextMarkerID >= laneInterface.numMarkers ) || ( direction == -1 && nextMarkerID < 0 ) ) {
                if ( alliance == (int) Alliance.FRIEND ) {
                    currLane.DamageEnemyLane(atkDmg);
                } else if ( alliance == (int) Alliance.FOE ) {
                    currLane.DamageFriendlyLane(atkDmg);
                }

                Kill();
                yield break; //exit coroutine
            }

            GameObject nextMarkerObj = currLane.transform.GetChild(nextMarkerID).gameObject;
            LaneMarker nextMarker = nextMarkerObj.GetComponent<LaneMarker>();

            /*Vacate & update previous marker*/
            currMarker.isOccupied = false;
            currMarker.friendlyOccupied = false;
            currMarker.occupyingEntity = null;
            
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

                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

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

                    /*Halt on game pause*/
                    while ( InputManager.gamePaused ) {
                        yield return null;
                    }

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

                    /*Halt on game pause*/
                    while ( InputManager.gamePaused ) {
                        yield return null;
                    }

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

        /*
        * Determines if there exists a troop in the specified lane marker that belongs to the opposition
        */
        public bool IsEngagingOpponent( int adjacentMarkerID ) {
            GameObject adjacentMarkerObj = currLaneObj.transform.GetChild(adjacentMarkerID).gameObject;
            LaneMarker adjacentMarker = adjacentMarkerObj.GetComponent<LaneMarker>();

            if ( alliance == (int) Alliance.FRIEND ) {
                return !adjacentMarker.friendlyOccupied;
            } else if ( alliance == (int) Alliance.FOE ) {
                return adjacentMarker.friendlyOccupied;
            }

            return false;
        }

        /*
        * Attacks the troop occupying a specific lane
        */
        public IEnumerator AttackOpponent( int targetMarkerID ) {

            GameObject targetMarkerObj = currLaneObj.transform.GetChild(targetMarkerID).gameObject;
            LaneMarker targetMarker = targetMarkerObj.GetComponent<LaneMarker>();
            bool cueAttack = false;
            int beatTick = -1;

            inCombat = true;

            /*Keep attacking until opponent is dead*/
            while ( true ) {

                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                /*On every beat*/
                if ( beatTick != conductor.currBeat ) {
                    beatTick = conductor.currBeat;

                    /*Continue attacking*/
                    if ( cueAttack ) {
                        GameObject opponentObj = targetMarker.occupyingEntity;
                        Troop opponent = opponentObj.GetComponent<Troop>();
                        conductor.musicSources[0].PlayOneShot(conductor.attackSfx, 0.07f);

                        opponent.health -= atkDmg;

                        if ( opponent.health > 0 ) {
                            StartCoroutine( opponent.TakeDamage(atkDmg) );
                        } else {
                            inCombat = false;
                            opponent.Kill();
                            yield break;
                        }
                    }

                    /*Begin attacking*/
                    if ( !cueAttack && beatTick == 3 ) {
                        conductor.musicSources[0].PlayOneShot(conductor.attackSfx, 0.07f);
                        cueAttack = true;
                    }
                }

                yield return null;

            }
            
        }

        /*
        * Terminates the troop & updates various lane-related info
        */
        private void Kill() {
            /*Vacate & update current lane marker*/
            currMarker.isOccupied = false;
            currMarker.friendlyOccupied = false;
            currMarker.occupyingEntity = null;

            /*Update troop count*/
            currLane.numTroops--;

            if ( alliance == (int) Alliance.FRIEND ) {
                currLane.numFriendlies--;
            } else if ( alliance == (int) Alliance.FOE ) {
                currLane.numEnemies--;
            }  

            StopAllCoroutines();
            Destroy(gameObject);
        }

        /*Stub function*/
        public IEnumerator TakeDamage( int dmg ) {
            Color originalColor = rend.color;
            rend.color = Color.red;

            yield return new WaitForSeconds(0.2f);

            rend.color = originalColor;

            yield return null;
        }

        /*Temporary function*/
        public IEnumerator Bop() {
            /*Squish*/
            while ( transform.localScale.y > bopMin ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                transform.localScale += new Vector3( 0, -0.03f, 0);
                yield return null;
            }

            /*Stretch*/
            while ( transform.localScale.y < originScale ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                transform.localScale += new Vector3( 0, 0.03f, 0);
                yield return null;
            }
        }
    }
    
}
