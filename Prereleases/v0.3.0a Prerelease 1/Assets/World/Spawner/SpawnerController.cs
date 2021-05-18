using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.Entity.Troop;
using MusicalKnights.Entity.Commanders;

namespace MusicalKnights.World {
    public class SpawnerController : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // TROOPS
        [Header("Troop Spawnables")]
        public GameObject friendlyTroopObj;
        public GameObject enemyTroopObj;

        public int numTroopsSpawned = 0;

        // DRAW ORDER
        [HideInInspector] public int[] friendlyDrawOrder = {0, 0, 0};
        [HideInInspector] public int[] enemyDrawOrder = {0, 0, 0};

        // COMMANDERS
        private CommanderController _commanderController;
        private Commander _playerCommander;
        private Commander _adversaryCommander;

        // SPAWNERS
        [Header("Spawn Settings")]
        [SerializeField] private string _friendlySpawnParent = "Friendly Spawner";      
        [SerializeField] private string _friendlyEntityParent = "Friendlies";
        [SerializeField] private string _enemySpawnParent = "Enemy Spawner";
        [SerializeField] private string _enemyEntityParent = "Enemies";

        private GameObject _friendlySpawnerParentObj;
        private GameObject _enemySpawnerParentObj;
        [HideInInspector] public GameObject[] friendlySpawnerObjs = new GameObject[3];
        [HideInInspector] public GameObject[] enemySpawnerObjs = new GameObject[3];

        // LANES
        [Header("Lane Settings")]
        [SerializeField] private string _laneParent = "Lanes";
        [HideInInspector] public LaneController laneInterface;
        private GameObject _laneParentObj;
        
        private void Start() {
            _conductor = Conductor.instance;

            //Load commander-related
            _commanderController = GameObject.Find("Commanders").GetComponent<CommanderController>();
            _playerCommander = _commanderController.playerObj.GetComponent<Commander>();
            _adversaryCommander = _commanderController.adversaryObj.GetComponent<Commander>();

            //Load spawner-related
            _friendlySpawnerParentObj = GameObject.Find(_friendlySpawnParent);
            _enemySpawnerParentObj = GameObject.Find(_enemySpawnParent);

            for ( int i = 0; i < 3; i++ ) {
                friendlySpawnerObjs[i] = _friendlySpawnerParentObj.transform.GetChild(i).gameObject;
                enemySpawnerObjs[i] = _enemySpawnerParentObj.transform.GetChild(i).gameObject;
            }

            //Load lane-related
            _laneParentObj = GameObject.Find(_laneParent);
            laneInterface = _laneParentObj.GetComponent<LaneController>();
        }

        private void Update() {

            //Do on every beat
            if ( _conductor.onBeat && _conductor.songStarted ) {

                //Troops are spawned at the beginning of their enemy's turn, requirng the following mildly reverse-logic
                if ( _conductor.measurePosInBeats == 0 ) {

                    if ( _commanderController.enemyTurn ) {
                        if ( _playerCommander.cmdIssued == Commands.SPAWN_TOP_ID ) {
                            SpawnTroop( friendlyTroopObj, 0, (int) Commander.Alliance.FRIEND );
                        } else if ( _playerCommander.cmdIssued == Commands.SPAWN_MID_ID ) {
                            SpawnTroop( friendlyTroopObj, 1, (int) Commander.Alliance.FRIEND );
                        } else if ( _playerCommander.cmdIssued == Commands.SPAWN_BTM_ID ) {
                            SpawnTroop( friendlyTroopObj, 2, (int) Commander.Alliance.FRIEND );
                        }
                    } else if ( _commanderController.playerTurn ) {
                        if ( _adversaryCommander.cmdIssued == Commands.SPAWN_TOP_ID ) {
                            SpawnTroop( enemyTroopObj, 0, (int) Commander.Alliance.FOE );
                        } else if ( _adversaryCommander.cmdIssued == Commands.SPAWN_MID_ID ) {
                            SpawnTroop( enemyTroopObj, 1, (int) Commander.Alliance.FOE );
                        } else if ( _adversaryCommander.cmdIssued == Commands.SPAWN_BTM_ID ) {
                            SpawnTroop( enemyTroopObj, 2, (int) Commander.Alliance.FOE );
                        }
                    }

                }

            }

        }

        /*
        * Spawns a troop at a given lane
        */
        public void SpawnTroop( GameObject troopToSpawn, int laneHeight, int alliance ) {

            //Load spawn lane & marker
            GameObject laneObj = laneInterface.laneObjs[laneHeight];
            int spawnMarkerID = 0;
            
            if ( alliance == (int) Commander.Alliance.FRIEND ) {
                spawnMarkerID = 0;
            } else if ( alliance == (int) Commander.Alliance.FOE ) {
                spawnMarkerID = laneInterface.numMarkers - 1;
            }

            GameObject spawnMarkerObj = laneObj.transform.GetChild(spawnMarkerID).gameObject;
            LaneMarker spawnMarker = spawnMarkerObj.GetComponent<LaneMarker>();

            if ( IsSpawnMarkerOccupied( spawnMarker, laneHeight, alliance ) ) {
                return;
            }

            //Spawn troop at their issuing commander's side
            GameObject spawnedTroopObj = null;
            SpriteRenderer troopRend;

            if ( alliance == (int) Commander.Alliance.FRIEND ) {
                spawnedTroopObj = Instantiate( troopToSpawn, friendlySpawnerObjs[laneHeight].transform.position, friendlySpawnerObjs[laneHeight].transform.rotation );
            } else if ( alliance == (int) Commander.Alliance.FOE ) {
                spawnedTroopObj = Instantiate( troopToSpawn, enemySpawnerObjs[laneHeight].transform.position, enemySpawnerObjs[laneHeight].transform.rotation );
            }
            
            troopRend = spawnedTroopObj.GetComponent<SpriteRenderer>();
            SnapEntityToGround( spawnedTroopObj, laneHeight );
            numTroopsSpawned++;

            //Initialize troop stats & parameters
            Troop spawnedTroop = spawnedTroopObj.GetComponent<Troop>();
            spawnedTroop.laneHeight = laneHeight;
            spawnedTroop.alliance = alliance;
            spawnedTroop.currMarkerID = spawnMarkerID;
            spawnedTroop.id = numTroopsSpawned;

            laneInterface.lanes[laneHeight].numTroops++;

            if ( alliance == (int) Commander.Alliance.FRIEND ) {
                laneInterface.lanes[laneHeight].numFriendlies++;
                spawnedTroopObj.transform.SetParent(GameObject.Find(_friendlyEntityParent).transform);
            } else if ( alliance == (int) Commander.Alliance.FOE ) {
                laneInterface.lanes[laneHeight].numEnemies++;
                spawnedTroopObj.transform.SetParent(GameObject.Find(_enemyEntityParent).transform);
            }
            
            //Move entity to spawn marker
            spawnedTroopObj.transform.position = new Vector3( spawnMarkerObj.transform.position.x, spawnedTroopObj.transform.position.y, 0f );
            spawnMarker.Occupy(spawnedTroopObj);

            //Apply sorting layer & update draw order
            if ( alliance == (int) Commander.Alliance.FRIEND ) {
                troopRend.sortingLayerName = SortLayers.FRIENDLY[laneHeight];
                troopRend.sortingOrder = friendlyDrawOrder[laneHeight];
                friendlyDrawOrder[laneHeight]--;
            } else if ( alliance == (int) Commander.Alliance.FOE ) {
                troopRend.sortingLayerName = SortLayers.ENEMY[laneHeight];
                troopRend.sortingOrder = enemyDrawOrder[laneHeight];
                enemyDrawOrder[laneHeight]--;
            }

            laneInterface.lanes[laneHeight].activeTroopObjList.Add(spawnedTroopObj);
            
        }

        /*
        * Checks if a spawn marker belonging to either commander is occupied
        * Also updates command strings for debug purposes
        */
        public bool IsSpawnMarkerOccupied( LaneMarker spawnMarker, int laneHeight, int alliance ) {

            if ( spawnMarker.isOccupied ) {

                if ( alliance == (int) Commander.Alliance.FRIEND ) {
                    if ( laneHeight == 0 ) {
                        _playerCommander.cmdIssued = "top lane occupied";
                    } else if ( laneHeight == 1 ) {
                        _playerCommander.cmdIssued = "mid lane occupied";
                    } else if ( laneHeight == 2 ) {
                        _playerCommander.cmdIssued = "bottom lane occupied";
                    }

                } else if ( alliance == (int) Commander.Alliance.FOE ) {
                    if ( laneHeight == 0 ) {
                        _adversaryCommander.cmdIssued = "top lane occupied";
                    } else if ( laneHeight == 1 ) {
                        _adversaryCommander.cmdIssued = "mid lane occupied";
                    } else if ( laneHeight == 2 ) {
                        _adversaryCommander.cmdIssued = "bottom lane occupied";
                    }
                }

                return true;
            }

            return false;

        }

        /*
        * Aligns entity's "feet" (i.e. bottom of collider) to floor of respective lane
        */
        private void SnapEntityToGround( GameObject entityToSnap, int laneHeight ) {
            BoxCollider2D laneCol = laneInterface.laneColliders[laneHeight];
            BoxCollider2D entityCol = entityToSnap.GetComponent<BoxCollider2D>();

            float laneUpperBound = laneCol.bounds.center.y + laneCol.bounds.extents.y;
            float entityLowerBound = entityCol.bounds.center.y - entityCol.bounds.extents.y;

            entityToSnap.transform.position -= new Vector3( 0, entityLowerBound - laneUpperBound, 0 );
        }

    }

}


