using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friendly : MonoBehaviour {
    
    //BEAT TIMER
    private int nextBeat = 0, lastBeat = 0, personalBeat = 0;
    private int beatOffset;

    //BOP (TEMP)
    private float originScale = 0;
    private float bopMin = 0;

    //PARAMETERS
    [SerializeField] public int cost = 0;
    [SerializeField] public int health = 1;
    [SerializeField] public bool[] lanePosition = new bool[3]; //index 0 = top, 1 = mid, 2 = bottom
    [SerializeField] public int positionInLane;

    //PROJECTILE
    [SerializeField] private GameObject projectile;
    [SerializeField] public int fireBeatScope;

    //MISC
    private SpriteRenderer rend;

    private void Start() {
        originScale = transform.localScale.y;
        bopMin = transform.localScale.y - 0.3f;

        beatOffset = (int) Random.Range(0f, 3.99f);
        rend = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        nextBeat = (int) Conductor.instance.loopPositionInBeats % 4;

        if ( lastBeat != nextBeat ) {
            lastBeat = nextBeat;
            beatOffset++;
            personalBeat = beatOffset % fireBeatScope;

            StartCoroutine( Bop() ); //temp

            if ( personalBeat == 0 ) {
                Fire();
            }
        }

        if ( health <= 0 ) {
            Destroy( gameObject );
        }
    }

    private IEnumerator Bop() {
        while ( transform.localScale.y > bopMin ) {
            transform.localScale += new Vector3( 0, -0.03f, 0);
            yield return null;
        }

        while ( transform.localScale.y < originScale ) {
            transform.localScale += new Vector3( 0, 0.03f, 0);
            yield return null;
        }
    }
    
    /*
    * Used (mainly by other GameObjects) to call TakeDamage()
    */
    public void Hit() {
        StartCoroutine( TakeDamage() );
    }

    private IEnumerator TakeDamage() {
        float shade = 0;

        while ( shade < 1 ) {
            rend.color = new Color( 1, shade, shade, 1 );
            shade += 0.03f;
            yield return null;
        }
    }

    /*
    * Fire projectile
    */
    public void Fire() {
        Instantiate( projectile, new Vector3( transform.position.x, transform.position.y + Random.Range(-0.1f, 0.1f) ), transform.rotation );
    }

}
