using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //This allows the IComparable Interface

public class SC_Island
{
    public int                   index;
    public string                name;
    public SC_Enums.IslandCity   city;
    public SC_Enums.IslandStatus status;
    public SC_Enums.IslandsType  type;
    public int                   sellingPrice;
    public int                   rentalPrice;
    public int                   buildingsNumber;
    public SC_Enums.Player       owner;

    public SC_Island(int index,string name, SC_Enums.IslandCity city, SC_Enums.IslandStatus status, SC_Enums.IslandsType type, int sellingPrice, int rentalPrice, SC_Enums.Player owner)
    {
        this.index = index;
        this.name = name;
        this.city = city;
        this.status = status;
        this.type = type;
        this.sellingPrice = sellingPrice;
        this.rentalPrice = rentalPrice;
        this.owner = owner;
        this.buildingsNumber = 0;
    }
}
