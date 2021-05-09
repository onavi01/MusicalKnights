using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerInterface : MonoBehaviour {

    //SPAWNABLES
    [SerializeField] public GameObject friendlyTroop;
    [SerializeField] public GameObject enemyTroop;

    //DRAW ORDER
    public int[] friendlyDrawOrder = {0, 0, 0};
    public int[] enemyDrawOrder = {0, 0, 0};

    //SPAWNERS

    /*Parent GameObjects holding the three lane spawners for friendly & enemy troops respectively*/
    [SerializeField] private string FRIENDLY_SPAWNER_HEAD = "Friendly Spawner";      
    [SerializeField] private string ENEMY_SPAWNER_HEAD = "Enemy Spawner";

    /*Parent GameObjects holding all active friendly & enemy troops respectively*/
    [SerializeField] private string FRIENDLY_PARENT = "Friendlies";                  
    [SerializeField] private string ENEMY_PARENT = "Enemies";

    private GameObject friendlySpawnerHead, enemySpawnerHead;
    public GameObject[] friendlySpawners = new GameObject[3];
    public GameObject[] enemySpawners = new GameObject[3];

    //LANES

    /*Parent GameObject holding the three lane objects*/
    [SerializeField] private string LANE_PARENT = "Lanes";

    GameObject laneParent;
    LaneInterface laneInterface;

    //BEAT TIMER
    private Conductor conductor;
    private int lastBeat = -1;

    private void Start() {
        /*Load Conductor object*/
        conductor = Conductor.instance;

        /*Find spawners*/
        friendlySpawnerHead = GameObject.Find(FRIENDLY_SPAWNER_HEAD);
        enemySpawnerHead = GameObject.Find(ENEMY_SPAWNER_HEAD);

        for ( int i = 0; i < 3; i++ ) {
            friendlySpawners[i] = friendlySpawnerHead.transform.GetChild(i).gameObject;
            enemySpawners[i] = enemySpawnerHead.transform.GetChild(i).gameObject;
        }

        /*Load lanes*/
        laneParent = GameObject.Find(LANE_PARENT);
        laneInterface = laneParent.GetComponent<LaneInterface>();
    }

    private void Update() {

        /*On every beat*/
        if ( lastBeat != conductor.currBeat ) {
            lastBeat = conductor.currBeat;

            /*Spawn soldier at the specified lane at the end of the commander's turn*/
            if ( lastBeat == 0 && ( conductor.measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || conductor.measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                if ( PlayerCommander.cmdIssued == Commands.SPAWN_TOP_STR ) {
                    SpawnFriendly( friendlyTroop, 0 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_MID_STR ) {
                    SpawnFriendly( friendlyTroop, 1 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_BOTTOM_STR ) {
                    SpawnFriendly( friendlyTroop, 2 );
                }
            } else if ( lastBeat == 0 && ( conductor.measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                if ( EnemyCommander.cmdIssued == Commands.SPAWN_TOP_STR ) {
                    SpawnEnemy( enemyTroop, 0 );
                } else if ( EnemyCommander.cmdIssued == Commands.SPAWN_MID_STR ) {
                    SpawnEnemy( enemyTroop, 1 );
                } else if ( EnemyCommander.cmdIssued == Commands.SPAWN_BOTTOM_STR ) {
                    SpawnEnemy( enemyTroop, 2 );
                }
            }
        }

    }

    /*
    * Spawns a friendly entity at a given lane
    */
    public void SpawnFriendly( GameObject toSpawn, int lane ) {
        GameObject laneObj = laneParent.transform.GetChild(lane).gameObject;    //load lane that entity will be spawned at
        GameObject startMarker = laneObj.transform.GetChild(0).gameObject;      //load marker that entity will first move to
        Lane laneScript = laneObj.GetComponent<Lane>();
        LaneMarker startMarkerScript = startMarker.GetComponent<LaneMarker>();

        /*Don't spawn anything if starting marker is still occupied*/
        if ( startMarkerScript.isOccupied ) {
            if ( lane == 0 ) {
                PlayerCommander.cmdIssued = "top lane occupied";
            } else if ( lane == 1 ) {
                PlayerCommander.cmdIssued = "mid lane occupied";
            } else if ( lane == 2 ) {
                PlayerCommander.cmdIssued = "bottom lane occupied";
            }

            return;
        }

        GameObject entityToSpawn = Instantiate( toSpawn, friendlySpawners[lane].transform.position, friendlySpawners[lane].transform.rotation );    //spawn entity at specified lane
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();
        SnapEntityToGround( entityToSpawn, lane );

        /*Initialize entity stats & parameters*/
        Troop troop = entityToSpawn.GetComponent<Troop>();
        FriendlyTroop friendly = entityToSpawn.GetComponent<FriendlyTroop>();
        troop.lane = lane;
        troop.alliance = (int) Troop.Alliance.FRIEND;
        troop.currMarkerID = 0;
        friendly.lane = lane;
        friendly.currMarkerID = 0;
        friendly.inCombat = false;

        /*Move entity to starting lane marker (i.e. first lane marker for friendlies)*/
        entityToSpawn.transform.position = new Vector3( startMarker.transform.position.x, entityToSpawn.transform.position.y, 0f );
        startMarkerScript.isOccupied = true;
        startMarkerScript.friendlyOccupied = true;
        startMarkerScript.occupyingEntity = entityToSpawn;
        friendly.currMarker = startMarker;
        laneScript.numTroops++;
        laneScript.numFriendlies++;
        
        entityToSpawn.transform.SetParent(GameObject.Find(FRIENDLY_PARENT).transform); //set Friendly GameObject as parent
        
        /*Set draw order and layer*/
        rend.sortingLayerName = SortLayers.FRIENDLY[lane];
        rend.sortingOrder = friendlyDrawOrder[lane];

        /*Update layers count*/
        friendlyDrawOrder[lane]--;
    }

    /*
    * Spawns an enemy entity at a given lane
    */
    public void SpawnEnemy( GameObject toSpawn, int lane ) {
        GameObject laneObj = laneParent.transform.GetChild(lane).gameObject;                            //load lane that entity will be spawned at
        GameObject startMarker = laneObj.transform.GetChild(laneInterface.numMarkers - 1).gameObject;   //load marker that entity will first move to
        Lane laneScript = laneObj.GetComponent<Lane>();
        LaneMarker startMarkerScript = startMarker.GetComponent<LaneMarker>();

        /*Don't spawn anything if starting marker is still occupied*/
        if ( startMarkerScript.isOccupied ) {
            if ( lane == 0 ) {
                EnemyCommander.cmdIssued = "top lane occupied";
            } else if ( lane == 1 ) {
                EnemyCommander.cmdIssued = "mid lane occupied";
            } else if ( lane == 2 ) {
                EnemyCommander.cmdIssued = "bottom lane occupied";
            }

            return;
        }

        GameObject entityToSpawn = Instantiate( toSpawn, enemySpawners[lane].transform.position, enemySpawners[lane].transform.rotation );  //spawn entity at specified lane
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();
        SnapEntityToGround( entityToSpawn, lane );

        /*Initialize entity stats & parameters*/
        Troop troop = entityToSpawn.GetComponent<Troop>();
        EnemyTroop enemy = entityToSpawn.GetComponent<EnemyTroop>();
        troop.lane = lane;
        troop.alliance = (int) Troop.Alliance.FOE;
        troop.currMarkerID = laneInterface.numMarkers - 1;
        enemy.lane = lane;
        enemy.currMarkerID = laneInterface.numMarkers - 1;
        enemy.inCombat = false;

        /*Move entity to starting lane marker (i.e. last lane marker for enemies)*/
        entityToSpawn.transform.position = new Vector3( startMarker.transform.position.x, entityToSpawn.transform.position.y, 0f );
        startMarkerScript.isOccupied = true;
        startMarkerScript.friendlyOccupied = false;
        startMarkerScript.occupyingEntity = entityToSpawn;
        enemy.currMarker = startMarker;
        laneScript.numTroops++;
        laneScript.numEnemies++;
        
        entityToSpawn.transform.SetParent(GameObject.Find(ENEMY_PARENT).transform);
        
        /*Set draw order and layer*/
        rend.sortingLayerName = SortLayers.ENEMY[lane];
        rend.sortingOrder = enemyDrawOrder[lane];

        /*Update layers count*/
        enemyDrawOrder[lane]--;
    }

    /*
    * Aligns entity's "feet" (bottom of collider) to floor of respective lane
    */
    private void SnapEntityToGround( GameObject entityToSnap, int lane ) {
        Collider2D laneCol = GameObject.Find(SortLayers.LANE[lane]).GetComponent<BoxCollider2D>();
        Collider2D entityCol = entityToSnap.GetComponent<BoxCollider2D>();

        float laneUpperBound = laneCol.bounds.center.y + laneCol.bounds.extents.y;
        float entityLowerBound = entityCol.bounds.center.y - entityCol.bounds.extents.y;

        entityToSnap.transform.position -= new Vector3( 0, entityLowerBound - laneUpperBound, 0 );
    }

}
