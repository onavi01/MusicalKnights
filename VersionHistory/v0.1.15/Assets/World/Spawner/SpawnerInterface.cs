using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerInterface : MonoBehaviour {

    //SPAWNABLES
    [SerializeField] public GameObject friendlySoldier;
    [SerializeField] public GameObject enemySoldier;

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
                    SpawnFriendly( friendlySoldier, 0 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_MID_STR ) {
                    SpawnFriendly( friendlySoldier, 1 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_BOTTOM_STR ) {
                    SpawnFriendly( friendlySoldier, 2 );
                }
            } else if ( lastBeat == 0 && ( conductor.measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                if ( EnemyCommander.cmdIssued == Commands.SPAWN_TOP_STR ) {
                    SpawnEnemy( enemySoldier, 0 );
                } else if ( EnemyCommander.cmdIssued == Commands.SPAWN_MID_STR ) {
                    SpawnEnemy( enemySoldier, 1 );
                } else if ( EnemyCommander.cmdIssued == Commands.SPAWN_BOTTOM_STR ) {
                    SpawnEnemy( enemySoldier, 2 );
                }
            }
        }

    }

    /*
    * Spawns a friendly entity at a given lane
    */
    public void SpawnFriendly( GameObject toSpawn, int lane ) {
        GameObject laneObj = laneParent.transform.GetChild(lane).gameObject;      //load lane that entity will be spawned at
        GameObject entityToSpawn = Instantiate( toSpawn, friendlySpawners[lane].transform.position, friendlySpawners[lane].transform.rotation );    //spawn entity at specified lane
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();
        SnapEntityToGround( entityToSpawn, lane );

        /*Initialize entity stats & parameters*/
        FriendlySoldier friendly = entityToSpawn.GetComponent<FriendlySoldier>();
        friendly.lane = lane;
        friendly.currMarkerPos = 0;

        /*Move entity to starting lane marker (i.e. first lane marker for friendlies)*/
        GameObject startMarker = laneObj.transform.GetChild(0).gameObject;
        entityToSpawn.transform.position = new Vector3( startMarker.transform.position.x, entityToSpawn.transform.position.y, 0f );
        startMarker.GetComponent<LaneMarker>().isOccupied = true;
        
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
        GameObject laneObj = laneParent.transform.GetChild(lane).gameObject;      //load lane that entity will be spawned at
        GameObject entityToSpawn = Instantiate( toSpawn, enemySpawners[lane].transform.position, enemySpawners[lane].transform.rotation );  //spawn entity at specified lane
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();
        SnapEntityToGround( entityToSpawn, lane );

        /*Initialize entity stats & parameters*/
        EnemySoldier enemy = entityToSpawn.GetComponent<EnemySoldier>();
        enemy.lane = lane;
        enemy.currMarkerPos = laneInterface.numMarkers - 1;

        /*Move entity to starting lane marker (i.e. last lane marker for enemies)*/
        GameObject startMarker = laneObj.transform.GetChild(laneInterface.numMarkers - 1).gameObject;
        entityToSpawn.transform.position = new Vector3( startMarker.transform.position.x, entityToSpawn.transform.position.y, 0f );
        startMarker.GetComponent<LaneMarker>().isOccupied = true;
        
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
