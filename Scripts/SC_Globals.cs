using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using e = SC_Enums;

public class SC_Globals
{
    public static string roomID;
    public static string userID;
    public static int MAX_USRES = 2;
    public static int MAX_TURN_TIME = 120;

    public static string MyName;
    public static string GusetName;
    public static e.Player PlayerIdentity;

    //Multi Or Single Player
    public static e.GameMode gameMode;

    //Communication between the logic of the menu and the game
    public static bool RestartGame = false;
    public static bool GoToMainMenu = false;
}
