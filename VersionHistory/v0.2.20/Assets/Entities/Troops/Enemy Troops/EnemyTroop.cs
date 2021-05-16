using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commander;

namespace MusicalKnights.Entity.Troop {

    [ RequireComponent(typeof(Troop)) ]

    public class EnemyTroop : MonoBehaviour {

        // INHERITED CLASSES
        [HideInInspector] public Troop troop;
        
        // INTERNAL METRNOME
        private Conductor _conductor;
        
        private void Start() {
            _conductor = Conductor.instance;

            //Inherit superclasses
            troop = GetComponent<Troop>();
        }

        private void Update() {

            int direction = 0;

            if ( troop.alliance == (int) Troop.Alliance.FRIEND ) {
                direction = 1;
            } else if ( troop.alliance == (int) Troop.Alliance.FOE ) {
                direction = -1;
            }

            //Do on every beat
            if ( _conductor.onBeat ) {

                //Follow adversary commands directly after enemy's turn
                if ( _conductor.measurePosInBeats == 0 && PlayerCommander.playerTurn ) {

                    //MARCH & CHARGE
                    if ( EnemyCommander.cmdIssued == Commands.MARCH_ID || EnemyCommander.cmdIssued == Commands.CHARGE_ID ) {
                        troop.MarchDefaultBehaviour(direction);

                    //STEADY
                    } else if ( EnemyCommander.cmdIssued == Commands.STEADY_ID ) {
                        troop.SteadyDefaultBehaviour(direction);

                    }

                }

            }

        }

    }
    
}


