using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneMarker : MonoBehaviour {
    private SpriteRenderer rend;
    public GameObject occupyingEntity;
    public bool isOccupied = false, friendlyOccupied = false;
    public int markerID = 0;

    void Start() {
        rend = GetComponent<SpriteRenderer>();
    }

    void Update() {
        /*Temp - colour markers based on occupancy*/
        if ( isOccupied ) {
            if ( friendlyOccupied ) {
                rend.color = new Color(0f, 0f, 1f, 0.35f);
            } else {
                rend.color = new Color(1f, 0f, 0f, 0.35f);
            }
        } else {
            rend.color = new Color(1f, 1f, 1f, 0.35f);
        }
    }
}
