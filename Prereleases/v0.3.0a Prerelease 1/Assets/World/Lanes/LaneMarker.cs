using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.Entity.Commanders;
using MusicalKnights.Entity.Troop;

namespace MusicalKnights.World {
    public class LaneMarker : MonoBehaviour {

        // MARKERS
        public int id = 0;
        [HideInInspector] public GameObject occupyingTroopObj = null;
        [HideInInspector] public Troop occupyingTroop;
        [HideInInspector] public bool isOccupied = false;
        [HideInInspector] public bool isFriendlyOccupied = false;

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

                if ( occupyingTroop.alliance == (int) Commander.Alliance.FRIEND ) {
                    isFriendlyOccupied = true;
                } else if ( occupyingTroop.alliance == (int) Commander.Alliance.FOE ) {
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

        public void Occupy( GameObject troopObj ) {
            Troop troop = troopObj.GetComponent<Troop>();
            
            troop.currMarkerID = id;
            troop.currMarkerObj = gameObject;
            troop.currMarker = GetComponent<LaneMarker>();
            
            occupyingTroopObj = troopObj;
        }

        public void Vacate() {
            occupyingTroopObj = null;
        }
    }
    
}


