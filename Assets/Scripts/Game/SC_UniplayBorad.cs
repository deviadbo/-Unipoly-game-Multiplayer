using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using e = SC_Enums;

public class SC_UniplayBorad
{

    const int MAX_CITY_ISLAND = 3;
    private List<SC_Island> boardUniplay;

    public SC_UniplayBorad()
    {
        boardUniplay = new List<SC_Island>();
        IslandList_Init();
    }

    public void UpdateIsland(int _Index, e.IslandStatus _newStatus, e.Player _newOwner)
    {
        if (_Index >= 0 && _Index < boardUniplay.Count)
        {
            boardUniplay[_Index].status = _newStatus;
            boardUniplay[_Index].owner = _newOwner;
        }
    }


    /// <summary>
    /// Count the amount of islands purchased in specific city
    /// </summary>
    public int CountPurchasedCity(e.IslandCity _city)
    {
        int PurchasedCity = 0;
        foreach (SC_Island island in boardUniplay)
        {
            if (island.city == _city && island.status == e.IslandStatus.Purchased)
                PurchasedCity++;
        }
        return PurchasedCity;
    }


    /// <summary>
    /// Once all the islands have been purchased, the function updates everyone's status for a building permit.
    /// </summary>
    public void ChangeTo_BuildingPermitInCity(e.IslandCity _city)
    {
        foreach (SC_Island island in boardUniplay)
        {
            if (island.city == _city)
                island.status = e.IslandStatus.Build_Permit;
        }
    }

    /// <summary>
    /// The function returns answer if all islands were purchased or not
    /// </summary>
    public bool BuildingPermitInCity(e.IslandCity _city)
    {
        if (CountPurchasedCity(_city) == MAX_CITY_ISLAND)
        {
            ChangeTo_BuildingPermitInCity(_city);
            return true; 
        }
        else
            return false;
    }


    public e.IslandStatus GetIslandStatus(int _Index)
    {
        if (_Index >= 0 && _Index < boardUniplay.Count)
            return boardUniplay[_Index].status;
        return e.IslandStatus.Error;
    }
    public string GetIslandName(int _Index) => boardUniplay[_Index].name;
    public e.IslandCity GetIslandCity(int _Index) => boardUniplay[_Index].city;
    public e.IslandsType GetIslandType(int _Index) => boardUniplay[_Index].type;
    public e.Player GetIslandOwner(int _Index) => boardUniplay[_Index].owner;
    public int GetIslandSellingPrice(int _Index) => boardUniplay[_Index].sellingPrice;
    public int GetIslandRentalPrice(int _Index)
    {
        int _RentPay = 0;
        _RentPay = boardUniplay[_Index].rentalPrice;
        //If the owner has building, The is multiplied
        _RentPay += boardUniplay[_Index].rentalPrice * boardUniplay[_Index].buildingsNumber;
        return _RentPay;
    }

    public int GetBuildingsNumberInIsland(int _Index) => boardUniplay[_Index].buildingsNumber;
    public void NewBuilding(int _Index)
    {
        if (_Index >= 0 && _Index < boardUniplay.Count)
        {
            if (boardUniplay[_Index].type == e.IslandsType.Residence)
            { boardUniplay[_Index].buildingsNumber++; }
            Debug.Log(boardUniplay[_Index].name + " has " + (boardUniplay[_Index].buildingsNumber) + " buildings");
        }
    }
    public void PrintIslandStatus()
    {
        string data = "Current Island Data";
        foreach (SC_Island island in boardUniplay)
        {
            data = $"{data}\n{island.index}: {island.name}, {island.city}, {island.status}, {island.type}, {island.sellingPrice} U, {island.rentalPrice}, {island.owner}";
        }
        Debug.Log(data);
    }

    public int OwnerValue(e.Player _owner)
    {
        int _ownerValue = 0;
        foreach (SC_Island island in boardUniplay)
        {
            if (island.owner == _owner)
            {
                _ownerValue += island.sellingPrice;
                //if the owner has building in the Island
                //if he has 0 buldings is dosen't matter
                _ownerValue += island.sellingPrice * island.buildingsNumber;
            }
        }
        return _ownerValue;
    }
    void IslandList_Init()
    {
        //                                   Name                   IslandCity                  IslandStatus                Type                     Sell       Rent     Owner
        SC_Island idx_0  = new SC_Island(0,  "Start Island",        e.IslandCity.Start,         e.IslandStatus.Not_For_Sell, e.IslandsType.Start,     0,         0,       e.Player.Static);
        SC_Island idx_1  = new SC_Island(1,  "Ame City - Q1",       e.IslandCity.AmeCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 50000,     5058,    e.Player.Nobody);
        SC_Island idx_2  = new SC_Island(2,  "Ame City - Q2",       e.IslandCity.AmeCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 60000,     5900,    e.Player.Nobody);
        SC_Island idx_3  = new SC_Island(3,  "Ame City - Q3",       e.IslandCity.AmeCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 70000,     6900,    e.Player.Nobody);
        SC_Island idx_4  = new SC_Island(4,  "UniSpace",            e.IslandCity.UniSpace,      e.IslandStatus.For_Sell,     e.IslandsType.Company,   102000,    9000,    e.Player.Nobody);
        SC_Island idx_5  = new SC_Island(5,  "Prison",              e.IslandCity.Prison,        e.IslandStatus.Not_For_Sell, e.IslandsType.Prison,    0,         0,       e.Player.Static);
        SC_Island idx_6  = new SC_Island(6,  "Orange City - Q1",    e.IslandCity.OrangeCity,    e.IslandStatus.For_Sell,     e.IslandsType.Residence, 80000,     8000,    e.Player.Nobody);
        SC_Island idx_7  = new SC_Island(7,  "Orange City - Q2",    e.IslandCity.OrangeCity,    e.IslandStatus.For_Sell,     e.IslandsType.Residence, 90000,     9000,    e.Player.Nobody);
        SC_Island idx_8  = new SC_Island(8,  "Orange City - Q3",    e.IslandCity.OrangeCity,    e.IslandStatus.For_Sell,     e.IslandsType.Residence, 100000,    9900,    e.Player.Nobody);
        SC_Island idx_9  = new SC_Island(9,  "UniGreen",            e.IslandCity.UniGreen,      e.IslandStatus.For_Sell,     e.IslandsType.Company,   120000,    12000,   e.Player.Nobody);
        SC_Island idx_10 = new SC_Island(10, "Sky City - Q1",       e.IslandCity.SkyCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 50000,     4980,    e.Player.Nobody);
        SC_Island idx_11 = new SC_Island(11, "Sky City - Q2",       e.IslandCity.SkyCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 55000,     5800,    e.Player.Nobody);
        SC_Island idx_12 = new SC_Island(12, "UniCircus",           e.IslandCity.UniCircus,     e.IslandStatus.For_Sell,     e.IslandsType.Company,   110000,    9900,    e.Player.Nobody);
        SC_Island idx_13 = new SC_Island(13, "Sky City - Q3",       e.IslandCity.SkyCity,       e.IslandStatus.For_Sell,     e.IslandsType.Residence, 65000,     6800,    e.Player.Nobody);
        SC_Island idx_14 = new SC_Island(14, "UniGame",             e.IslandCity.UniGame,       e.IslandStatus.For_Sell,     e.IslandsType.Company,   130000,    12500,   e.Player.Nobody);
        SC_Island idx_15 = new SC_Island(15, "Ruby City - Q1",      e.IslandCity.RubyCity,      e.IslandStatus.For_Sell,     e.IslandsType.Residence, 92000,     9320,    e.Player.Nobody);
        SC_Island idx_16 = new SC_Island(16, "Ruby City - Q2",      e.IslandCity.RubyCity,      e.IslandStatus.For_Sell,     e.IslandsType.Residence, 95000,     9480,    e.Player.Nobody);
        SC_Island idx_17 = new SC_Island(17, "Ruby City - Q3",      e.IslandCity.RubyCity,      e.IslandStatus.For_Sell,     e.IslandsType.Residence, 97000,     9890,    e.Player.Nobody);
        SC_Island idx_18 = new SC_Island(18, "UniStockMarket",      e.IslandCity.UniStockMarket,e.IslandStatus.For_Sell,     e.IslandsType.Company,   118000,    12500,   e.Player.Nobody);
        SC_Island idx_19 = new SC_Island(19, "Unicorn LDT",         e.IslandCity.UnicornLTD,    e.IslandStatus.For_Sell,     e.IslandsType.Company,   155000,    15000,   e.Player.Nobody);


        //maxIslands = 20;
        boardUniplay.Add(idx_0);
        boardUniplay.Add(idx_1);
        boardUniplay.Add(idx_2);
        boardUniplay.Add(idx_3);
        boardUniplay.Add(idx_4);
        boardUniplay.Add(idx_5);
        boardUniplay.Add(idx_6);
        boardUniplay.Add(idx_7);
        boardUniplay.Add(idx_8);
        boardUniplay.Add(idx_9);
        boardUniplay.Add(idx_10);
        boardUniplay.Add(idx_11);
        boardUniplay.Add(idx_12);
        boardUniplay.Add(idx_13);
        boardUniplay.Add(idx_14);
        boardUniplay.Add(idx_15);
        boardUniplay.Add(idx_16);
        boardUniplay.Add(idx_17);
        boardUniplay.Add(idx_18);
        boardUniplay.Add(idx_19);
    }
}


