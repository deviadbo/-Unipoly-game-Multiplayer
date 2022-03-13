using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using e = SC_Enums;


public class SC_UniPlayer
{
    public int      player_id;
    public e.Player player;
    public string   player_name;
    public int      player_balnce;
    public int      player_currIsland;

    public SC_UniPlayer(int _id, e.Player _player, string _name, int _balnce, int _curr)
    {
        player_id = _id;
        player = _player;
        player_name = _name;
        player_balnce = _balnce;
        player_currIsland = _curr;
    }

    public string GetPlayerName()
        { return player_name; }

    public int GetReallyIsland()
    {
        return player_currIsland % 19;
    }
}
