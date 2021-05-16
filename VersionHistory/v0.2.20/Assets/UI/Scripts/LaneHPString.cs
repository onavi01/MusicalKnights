using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class LaneHPString : MonoBehaviour {

        // UI
        private Text _str;

        // LANES
        [HideInInspector] public LaneInterface laneInterface;

        private GameObject _laneParentObj;
        [HideInInspector] public GameObject currLaneObj;
        [HideInInspector] public Lane currLane;

        [Header("Lane Settings")]
        [SerializeField] private string _laneParent = "Lanes";
        public int laneHeight = 0;
        public bool belongsToPlayer = false;

        private void Start() {
            //Load lane-related
            _laneParentObj = GameObject.Find(_laneParent);
            laneInterface = _laneParentObj.GetComponent<LaneInterface>();
            currLaneObj = laneInterface.laneObjs[laneHeight];
            currLane = currLaneObj.GetComponent<Lane>();

            //Load UI-related
            _str = GetComponent<Text>();
        }

        private void Update() {
            if ( belongsToPlayer ) {
                _str.text = "Lane HP: " + currLane.friendlyLaneHP + "/" + currLane.maxFriendlyLaneHP;
            } else {
                _str.text = "Lane HP: " + currLane.enemyLaneHP + "/" + currLane.maxEnemyLaneHP;
            }
        }

    }

}

