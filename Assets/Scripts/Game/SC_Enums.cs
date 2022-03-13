using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************
* Project: 	Unipoly                  
* Enums Script                                     
*********************************/

public class SC_Enums: MonoBehaviour
{
    public enum GameMode
    {
        None, SinglePlayer, MultiPlayer
    };

    public enum Screens
    {
        MainMenu, Loading, Multiplayer, Options, StudentInfo, BoardGame
    };

    //Boradgame Settings
    public enum IslandStatus
    {
        For_Sell, Purchased, Not_For_Sell, Build_Permit ,Error
    };

    public enum IslandCity
    {
        Start, AmeCity, UniSpace, Prison, OrangeCity, UniGreen, SkyCity, UniCircus, UniGame, RubyCity, UniStockMarket, UnicornLTD
    };

    public enum IslandsType
    {
        Start, Prison, Residence, Company
    };

    public enum Player
    {
        Player1, Player2, Static, Nobody
    };

    public enum PopupClick
    {
        Yes, No, none
    };

    public enum Buttons
    {
        Button_BuyIsland, Button_BuyCompany, Button_Build, Button_PayRent, Dividend, none
    };
}
