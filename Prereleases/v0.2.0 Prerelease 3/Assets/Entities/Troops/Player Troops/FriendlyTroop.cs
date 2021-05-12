using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Troop {

    [ RequireComponent(typeof(Troop)) ]

    public class FriendlyTroop : MonoBehaviour {

        //INHERITED CLASSES
        Troop troop;
        
        //BEAT TIMER
        private Conductor conductor;
        private int lastBeat = 0;

        void Start() {
            /*Load Conductor object*/
            conductor = Conductor.instance;

            /*Inherit superclasses*/
            troop = GetComponent<Troop>();
        }

        void Update() {

            int direction = 0;

            /*Calculate default direction to march in*/
            if ( troop.alliance == (int) Troop.Alliance.FRIEND ) {
                direction = 1;
            } else if ( troop.alliance == (int) Troop.Alliance.FOE ) {
                direction = -1;
            }
            
            /*On every beat*/
            if ( lastBeat != conductor.currBeat ) {
                lastBeat = conductor.currBeat;

                /*Follow player commands directly after player's turn*/
                if ( lastBeat == 0 && EnemyCommander.enemyTurn ) {
                    
                    /*MARCH/CHARGE*/
                    if ( PlayerCommander.cmdIssued == Commands.MARCH_STR || PlayerCommander.cmdIssued == Commands.CHARGE_STR ) {
                        troop.MarchDefault(direction);
                        
                    /*STEADY*/
                    } else if ( PlayerCommander.cmdIssued == Commands.STEADY_STR ) {
                        troop.SteadyDefault(direction);
                    
                    }
                    
                }
                
            }

        }
        
    }

}


