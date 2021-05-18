using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;


namespace MusicalKnights.Entity.Commanders {
    public class CommanderController : MonoBehaviour, IInspectorEditable {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // COMMANDERS
        [HideInInspector] public bool playerTurn = false;
        [HideInInspector] public bool enemyTurn = false;

        [Header("Commander Settings")]
        public GameObject playerObj;
        public GameObject adversaryObj;

        [Range(3.7f, 3.9f)]
        public float playerTurnStartThreshold = 3.7f;

        [Range(7.7f, 7.9f)]
        public float playerTurnEndThreshold = 7.7f;
        public int mistakeLimit = 2;

        private void Start() {
            _conductor = Conductor.instance;
            ValidateProperties();
        }

        private void Update() {
            if ( _conductor.turnPos >= playerTurnStartThreshold && _conductor.turnPos <= playerTurnEndThreshold ) {
                playerTurn = true;
                enemyTurn = false;
            } else {
                playerTurn = false;
                enemyTurn = true;
            }
        }

        public void ValidateProperties() {
            if ( mistakeLimit < 0 || mistakeLimit > _conductor.beatsPerMeasure ) {
                mistakeLimit = 2;
            }
        }
    }

}


