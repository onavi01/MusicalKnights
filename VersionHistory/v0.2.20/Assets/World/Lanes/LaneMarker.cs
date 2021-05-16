using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.Entity.Troop;

namespace MusicalKnights.World {
    public class LaneMarker : MonoBehaviour {

        // MARKERS
        [HideInInspector] public GameObject occupyingTroopObj = null;
        [HideInInspector] public Troop occupyingTroop;
        [HideInInspector] public bool isOccupied = false;
        [HideInInspector] public bool isFriendlyOccupied = false;
        [HideInInspector] public int markerID = 0;

        // RENDERING
        private SpriteRenderer _rend;

        private void Start() {
            _rend = GetComponent<SpriteRenderer>();
        }

        private void Update() {

            //Continuously monitor marker vacancy & allegiance
            if ( occupyingTroopObj != null ) {
                occupyingTroop = occupyingTroopObj.GetComponent<Troop>();
                isOccupied = true;

                if ( occupyingTroop.alliance == (int) Troop.Alliance.FRIEND ) {
                    isFriendlyOccupied = true;
                } else if ( occupyingTroop.alliance == (int) Troop.Alliance.FOE ) {
                    isFriendlyOccupied = false;
                }
                
            } else {
                isOccupied = false;
                isFriendlyOccupied = false;
            }

            //Debug: Colour marker based on occupying troop's allegiance
            if ( InputManager.debugToggled ) {
                if ( isOccupied ) {
                    if ( isFriendlyOccupied ) {
                        _rend.color = new Color(0f, 0f, 1f, 0.35f);
                    } else {
                        _rend.color = new Color(1f, 0f, 0f, 0.35f);
                    }
                    
                } else {
                    _rend.color = new Color(1f, 1f, 1f, 0.35f);
                }
                
            } else {
                _rend.color = new Color(0f, 0f, 0f, 0f);
            }
            
        }
    }
    
}


