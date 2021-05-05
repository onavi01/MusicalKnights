using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commands {
    /*Command sequences*/
    public static char[] MARCH = {'w', 'w', 'w', 's'};
    public static char[] STEADY = {'s', 'a', 's', 'a'};
    public static char[] CHARGE = {'d', 'd', 'a', 'd'};
    public static char[] SPAWN_TOP = {'s', 's', 's', 'a'};
    public static char[] SPAWN_MID = {'s', 's', 's', 'w'};
    public static char[] SPAWN_BOTTOM = {'s', 's', 's', 'd'};

    /*Command string IDs*/
    public static string MARCH_STR = "march";
    public static string STEADY_STR = "steady";
    public static string CHARGE_STR = "charge";
    public static string SPAWN_TOP_STR = "spawn top";
    public static string SPAWN_MID_STR = "spawn mid";
    public static string SPAWN_BOTTOM_STR = "spawn bottom";
}
