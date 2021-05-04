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
    LaneInterface laneInterface;

    //BEAT TIMER
    private int currBeat = 0, lastBeat = -1;
    private float measurePos = 0f;

    private void Start() {
        /*Find friendlySpawners*/
        friendlySpawnerHead = GameObject.Find(FRIENDLY_SPAWNER_HEAD);
        enemySpawnerHead = GameObject.Find(ENEMY_SPAWNER_HEAD);

        for ( int i = 0; i < 3; i++ ) {
            friendlySpawners[i] = friendlySpawnerHead.transform.GetChild(i).gameObject;
            enemySpawners[i] = enemySpawnerHead.transform.GetChild(i).gameObject;
        }

        laneInterface = GameObject.Find("Lanes").GetComponent<LaneInterface>();
    }

    private void Update() {

        float loopPos = Conductor.instance.loopPositionInBeats;

        /*Apply input calibration is applied to measurePos and input accuracy*/
        currBeat = (int) ( loopPos % 4 );
        measurePos = (loopPos - Conductor.instance.playerInputOffset) % 8;

        /*On every beat*/
        if ( lastBeat != currBeat) {
            lastBeat = currBeat;

            /*SpawnFriendly friendly soldier at the specified lane at the end of the Player's turn*/
            if ( lastBeat == 0 && ( measurePos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || measurePos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                if ( PlayerCommander.cmdIssued == Commands.SPAWN_TOP_STR ) {
                    SpawnFriendly( friendlySoldier, 0 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_MID_STR ) {
                    SpawnFriendly( friendlySoldier, 1 );
                } else if ( PlayerCommander.cmdIssued == Commands.SPAWN_BOTTOM_STR ) {
                    SpawnFriendly( friendlySoldier, 2 );
                }
            } else if ( lastBeat == 0 && ( measurePos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && measurePos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                Debug.Log("Adversary says \"" + EnemyCommander.cmdIssued + "\"");
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
    * Spawns an friendly entity at a given lane
    */
    public void SpawnFriendly( GameObject toSpawn, int lane ) {
        GameObject laneObj = GameObject.Find("Lanes").transform.GetChild(lane).gameObject;
        GameObject entityToSpawn = Instantiate( toSpawn, friendlySpawners[lane].transform.position, friendlySpawners[lane].transform.rotation );
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();

        entityToSpawn.transform.position = new Vector3(laneObj.transform.GetChild(0).gameObject.transform.position.x, entityToSpawn.transform.position.y, 0f);
        laneObj.transform.GetChild(0).gameObject.GetComponent<LaneMarker>().isOccupied = true;
        
        entityToSpawn.transform.parent = GameObject.Find(FRIENDLY_PARENT).transform;
        
        /*Set draw order and layer*/
        rend.sortingLayerName = SortLayers.FRIENDLY[lane];
        rend.sortingOrder = friendlyDrawOrder[lane];

        /*Update layers count*/
        friendlyDrawOrder[lane]--;
        SnapEntityToGround( entityToSpawn, lane );
    }

    /*
    * Spawns an enemy entity at a given lane
    */
    public void SpawnEnemy( GameObject toSpawn, int lane ) {
        GameObject laneObj = GameObject.Find("Lanes").transform.GetChild(lane).gameObject;
        GameObject entityToSpawn = Instantiate( toSpawn, enemySpawners[lane].transform.position, enemySpawners[lane].transform.rotation );
        SpriteRenderer rend = entityToSpawn.GetComponent<SpriteRenderer>();

        entityToSpawn.transform.position = new Vector3(laneObj.transform.GetChild(laneInterface.numMarkers - 1).gameObject.transform.position.x, entityToSpawn.transform.position.y, 0f);
        laneObj.transform.GetChild(laneInterface.numMarkers - 1).gameObject.GetComponent<LaneMarker>().isOccupied = true;
        
        entityToSpawn.transform.parent = GameObject.Find(ENEMY_PARENT).transform;
        
        /*Set draw order and layer*/
        rend.sortingLayerName = SortLayers.ENEMY[lane];
        rend.sortingOrder = enemyDrawOrder[lane];

        /*Update layers count*/
        enemyDrawOrder[lane]--;
        SnapEntityToGround( entityToSpawn, lane );
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
