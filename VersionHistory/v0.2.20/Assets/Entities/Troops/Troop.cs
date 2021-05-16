using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.World;

namespace MusicalKnights.Entity.Troop {

    public class Troop : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // BOP (TEMP)
        private SpriteRenderer _rend;
        private float _originScale = 0;
        private float _bopMin = 0;

        // TROOP SETTINGS & PARAMETERS
        public enum Alliance {
            FRIEND,
            FOE
        };
        [HideInInspector] public int alliance = 0;

        [Header("Stats")]
        public int health = 1;
        public int atkDmg = 1;

        // TROOP STATS & PARAMETERS

        //Negative speeds & distances will move to the left
        [Header("Movement")]
        public int marchDistInMarkers = 0;
        public int steadyDistInMarkers = 0;
        public float moveSpd = 0;

        [HideInInspector] public float destMarkerDist = 0f;
        [HideInInspector] public bool inCombat = false;

        // LANES
        [Header("Lane Settings")]
        [SerializeField] private string _laneParent = "Lanes";
        [HideInInspector] public LaneInterface laneInterface;
        
        private GameObject _laneParentObj;
        [HideInInspector] public GameObject currLaneObj;
        [HideInInspector] public GameObject currMarkerObj;
        [HideInInspector] public Lane currLane;
        [HideInInspector] public LaneMarker currMarker;
        [HideInInspector] public int laneHeight = 0;
        [HideInInspector] public int currMarkerID = 0;

        private void Start() {

            _conductor = Conductor.instance;

            //Rendering
            _originScale = transform.localScale.y;
            _bopMin = transform.localScale.y - 0.3f;
            _rend = GetComponent<SpriteRenderer>();

            //Load lane-related
            _laneParentObj = GameObject.Find(_laneParent);
            laneInterface = _laneParentObj.GetComponent<LaneInterface>();
            
            currLaneObj = laneInterface.laneObjs[laneHeight];
            currLane = currLaneObj.GetComponent<Lane>();

            currMarkerObj = currLaneObj.transform.GetChild(currMarkerID).gameObject;
            currMarker = currMarkerObj.GetComponent<LaneMarker>();

        }

        private void Update() {
            //Do on every beat
            if ( _conductor.onBeat ) {
                StartCoroutine( Bop() );
            }
        }

        /*
        * Troop default behaviour when following their commander's 'March' command
        */
        public void MarchDefaultBehaviour( int direction ) {

            int destMarkerID = currMarkerID + marchDistInMarkers;

            if ( CanReachMarker( currMarkerID, destMarkerID, direction ) ) {
                destMarkerDist = (laneInterface.distBtwMarkers * marchDistInMarkers) / (float) _conductor.beatsPerMeasure;
                StartCoroutine( MarchToMarker( destMarkerID, destMarkerDist, direction ) );
                
            } else {
                int nextClosestMarkerID = FindNextClosestMarker( currMarkerID, destMarkerID, direction );
                int newDistInMarkers = 0;
                
                if ( nextClosestMarkerID != -1 ) {
                    newDistInMarkers = nextClosestMarkerID - currMarkerID;
                    destMarkerID = currMarkerID + newDistInMarkers;
                    destMarkerDist = (laneInterface.distBtwMarkers * newDistInMarkers) / (float) _conductor.beatsPerMeasure;
                    StartCoroutine( MarchToMarker( destMarkerID, destMarkerDist, direction ) );
                }

            }

            //If marching/charging towards an enemy
            if ( !inCombat && IsEngagingOpponent( currMarkerID + direction ) ) {
                StartCoroutine( AttackOpponent( currMarkerID + direction ) );
            }

        }

        /*
        * Troop default behaviour when following their commander's 'Steady' command
        */
        public void SteadyDefaultBehaviour( int direction ) {

            int destMarkerID = currMarkerID + steadyDistInMarkers;

            //Try marching to the destination marker directly
            if ( CanReachMarker( currMarkerID, destMarkerID, direction ) ) {
                destMarkerDist = (laneInterface.distBtwMarkers * steadyDistInMarkers) / (float) _conductor.beatsPerMeasure;
                StartCoroutine( MarchToMarker( destMarkerID, destMarkerDist, direction ) );

            //Try marching to the closest unoccupied marker from the destination marker, if there are any
            } else {
                int nextClosestMarkerID = FindNextClosestMarker( currMarkerID, destMarkerID, direction );
                int newDistInMarkers = 0;

                if ( nextClosestMarkerID != -1 ) {
                    newDistInMarkers = nextClosestMarkerID - currMarkerID;
                    destMarkerID = currMarkerID + newDistInMarkers;
                    destMarkerDist = (laneInterface.distBtwMarkers * newDistInMarkers) / (float) _conductor.beatsPerMeasure;
                    StartCoroutine( MarchToMarker( destMarkerID, destMarkerDist, direction ) );
                }
            }
            
        }

        /*
        * Checks if the markers leading up to and including a destination marker are free 
        * (i.e. checks if the troop can reach the destination marker)
        */
        public bool CanReachMarker( int currMarkerID, int destMarkerID, int direction ) {

            //To the right
            if ( direction == 1 ) {

                //Avoid searching out of lane bounds
                if ( destMarkerID >= laneInterface.numMarkers ) {
                    destMarkerID = laneInterface.numMarkers - 1;
                }

                for ( int i = currMarkerID + 1; i <= destMarkerID; i++ ) {
                    GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                    LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                    if ( marker.isOccupied ) {
                        return false;
                    }
                }

            //To the left
            } else if ( direction == -1 ) {

                //Avoid searching out of lane bounds
                if ( destMarkerID < 0 ) {
                    destMarkerID = 0;
                }

                for ( int i = currMarkerID - 1; i >= destMarkerID; i-- ) {
                    GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                    LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                    if ( marker.isOccupied ) {
                        return false;
                    }
                }

            }
            
            return true;
            
        }

        /*
        * Finds the closest unoccupied marker from a destination marker
        * Returns -1 if already at closest unoccupied marker
        */
        public int FindNextClosestMarker( int currMarkerID, int destMarkerID, int direction ) {

            //To the right
            if ( direction == 1 ) {

                //Avoid searching out of lane bounds
                if ( destMarkerID >= laneInterface.numMarkers ) {
                    destMarkerID = laneInterface.numMarkers - 1;
                }

                for ( int i = destMarkerID - 1; i > currMarkerID; i-- ) {
                    GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                    LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                    if ( !marker.isOccupied ) {
                        return i;
                    }
                }
            
            //To the left
            } else if ( direction == -1 ) {

                //Avoid searching out of lane bounds
                if ( destMarkerID < 0 ) {
                    destMarkerID = 0;
                }

                for ( int i = destMarkerID + 1; i < currMarkerID; i++ ) {
                    GameObject markerObj = currLaneObj.transform.GetChild(i).gameObject;
                    LaneMarker marker = markerObj.GetComponent<LaneMarker>();

                    /*Return furthest vacant marker*/
                    if ( !marker.isOccupied ) {
                        return i;
                    }
                }

            }

            return -1;
            
        }

        /*
        * Moves the troop to a destination marker & updates lane marker occupancy
        */
        public IEnumerator MarchToMarker( int destMarkerID, float destMarkerDist, int direction ) {

            int numStepsTaken = 0;

            //If out of lane bounds, damage opposition's lane & destroy this troop
            if ( ( direction == 1 && destMarkerID >= laneInterface.numMarkers ) || ( direction == -1 && destMarkerID < 0 ) ) {
                currLane.DamageLane( alliance, atkDmg );
                Kill();
                yield break;
            }

            GameObject destMarkerObj = currLane.transform.GetChild(destMarkerID).gameObject;
            LaneMarker destMarker = destMarkerObj.GetComponent<LaneMarker>();

            //Vacate & update previous marker
            currMarker.occupyingTroopObj = null;
            
            //Occupy destination marker
            currMarkerID = destMarkerID;
            currMarkerObj = destMarkerObj;
            currMarker = currMarkerObj.GetComponent<LaneMarker>();
            
            destMarker.occupyingTroopObj = gameObject;

            if ( alliance == (int) Alliance.FRIEND ) {
                destMarker.isFriendlyOccupied = true;
            } else if ( alliance == (int) Alliance.FOE ) {
                destMarker.isFriendlyOccupied = false;
            }

            //Move closer to the marker on every beat, over a given # of beats
            while ( true ) {

                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                if ( _conductor.onBeat ) {
                    numStepsTaken++;
                    StartCoroutine( March( destMarkerDist, direction ) );
                }

                if ( numStepsTaken == 4 ) {
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

            //To the right
            if ( direction == 1 ) {

                while ( transform.position.x < nextPos ) {

                    while ( InputManager.gamePaused ) {
                        yield return null;
                    }

                    //Avoid going beyond the specified distance
                    if ( ( transform.position.x + moveSpd ) <= nextPos ) {
                        transform.position += new Vector3( moveSpd, 0f, 0f );
                    } else {
                        transform.position += new Vector3( nextPos - transform.position.x, 0f, 0f ); 
                    }
                    
                    yield return null;

                }

            //To the left
            } else if ( direction == -1 ) {

                while ( transform.position.x > nextPos ) {

                    while ( InputManager.gamePaused ) {
                        yield return null;
                    }

                    //Avoid going beyond the specified distance
                    if ( ( transform.position.x + moveSpd ) >= nextPos ) {
                        transform.position += new Vector3( moveSpd, 0f, 0f );
                    } else {
                        transform.position += new Vector3( nextPos - transform.position.x, 0f, 0f ); 
                    }
                    
                    yield return null;

                }

            }
            
        }

        /*
        * Checks if there exists a troop belonging to the opposition in the specified lane marker
        */
        public bool IsEngagingOpponent( int targetMarkerID ) {

            if ( targetMarkerID < 0 || targetMarkerID >= laneInterface.numMarkers ) {
                return false;
            }

            GameObject tarketMarkerObj = currLaneObj.transform.GetChild(targetMarkerID).gameObject;
            LaneMarker targetMarker = tarketMarkerObj.GetComponent<LaneMarker>();

            if ( !targetMarker.isOccupied ) {
                return false;
            }

            bool engagingOpponent = false;

            //Check for troops belonging to the *other* commander
            if ( alliance == (int) Alliance.FRIEND ) {
                engagingOpponent = !targetMarker.isFriendlyOccupied;
            } else if ( alliance == (int) Alliance.FOE ) {
                engagingOpponent = targetMarker.isFriendlyOccupied;
            }

            return engagingOpponent;

        }

        /*
        * Attacks the troop occupying a specific lane
        */
        public IEnumerator AttackOpponent( int targetMarkerID ) {

            GameObject targetMarkerObj = currLaneObj.transform.GetChild(targetMarkerID).gameObject;
            LaneMarker targetMarker = targetMarkerObj.GetComponent<LaneMarker>();
            bool startedAtk = false;
            
            inCombat = true;

            //Keep attacking until the opponent is dead
            while ( true ) {

                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                //Do on every beat
                if ( _conductor.onBeat ) {

                    if ( !startedAtk ) {
                        if ( _conductor.measurePosInBeats == _conductor.beatsPerMeasure - 1 ) {
                            _conductor.musicSources[0].PlayOneShot(_conductor.troopAtkSfx, 0.07f);
                            startedAtk = true;
                        }

                    } else {
                        GameObject opponentObj = targetMarker.occupyingTroopObj;
                        Troop opponent = opponentObj.GetComponent<Troop>();

                        _conductor.musicSources[0].PlayOneShot(_conductor.troopAtkSfx, 0.07f);
                        opponent.health -= atkDmg;

                        if ( opponent.health > 0 ) {
                            StartCoroutine( opponent.TakeDamage(atkDmg) );
                        } else {
                            inCombat = false;
                            opponent.Kill();
                            yield break;
                        }
                    }

                }

                yield return null;

            }
            
        }

        /*
        * Destroys the troop & updates various lane-related info
        */
        private void Kill() {
            //Vacate & update current lane & lane marker
            currMarker.occupyingTroopObj = null;
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
            Color originalColor = _rend.color;
            _rend.color = Color.red;

            yield return new WaitForSeconds(0.2f);

            _rend.color = originalColor;

            yield return null;
        }

        /*Temporary function*/
        public IEnumerator Bop() {
            /*Squish*/
            while ( transform.localScale.y > _bopMin ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                transform.localScale += new Vector3( 0, -0.03f, 0);
                yield return null;
            }

            /*Stretch*/
            while ( transform.localScale.y < _originScale ) {
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
