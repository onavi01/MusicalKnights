using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.Entity.Commander {    
    public class Commands {
        //Command sequences
        public static char[] MARCH = {'w', 'w', 'w', 's'};
        public static char[] STEADY = {'s', 'a', 's', 'a'};
        public static char[] CHARGE = {'d', 'd', 'a', 'd'};
        public static char[] SPAWN_TOP = {'s', 's', 's', 'a'};
        public static char[] SPAWN_MID = {'s', 's', 's', 'w'};
        public static char[] SPAWN_BTM = {'s', 's', 's', 'd'};

        //Command string IDs
        public static string MARCH_ID = "march";
        public static string STEADY_ID = "steady";
        public static string CHARGE_ID = "charge";
        public static string SPAWN_TOP_ID = "spawn top";
        public static string SPAWN_MID_ID = "spawn mid";
        public static string SPAWN_BTM_ID = "spawn bottom";
    }
    
}


