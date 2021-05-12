using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class LaneMarker : MonoBehaviour {

        //MARKER PROPERTIES
        public GameObject occupyingEntity;
        public bool isOccupied = false, friendlyOccupied = false;
        public int markerID = 0;

        //RENDERING
        private SpriteRenderer rend;

        private void Start() {
            rend = GetComponent<SpriteRenderer>();
        }

        private void Update() {
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
    
}


