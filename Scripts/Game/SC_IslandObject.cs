using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using e = SC_Enums;

public class SC_IslandObject : MonoBehaviour
{
    public delegate void  ClickHandler(int _Index);
    public static   event ClickHandler OnClick;

    public int islandIndex = 0;
    Transform islandPostion;

    public GameObject Tooltip;
    RectTransform tooltipPostion;
    Text tooltipText;
    Text tooltipTextTitle;

    private SC_UniplayBorad currnetBoard;

    #region Singleton - SC_IslandObject
    private static SC_IslandObject instance;
    public static SC_IslandObject Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_IslandObject").GetComponent<SC_IslandObject>();
            return instance;
        }
    }
    #endregion

    private void Start()
    {
        Tooltip = SC_GameLogic.Instance.GetGameObj("Tooltip2");
        tooltipText = SC_GameLogic.Instance.GetGameObj("tooltipText").GetComponent<Text>();
        tooltipTextTitle = SC_GameLogic.Instance.GetGameObj("tooltipTextTitle").GetComponent<Text>();
        islandPostion = GetComponent<Transform>();
        tooltipPostion = Tooltip.GetComponent<RectTransform>();

        currnetBoard = SC_GameLogic.Instance.curBoard; //currnet board from SC_GameLogic
    }

    private void Update()
    {
        currnetBoard = SC_GameLogic.Instance.curBoard; //currnet board from SC_GameLogic
    }
    void OnMouseUp() //Click
    {
        //Debug.Log("OnMouseUp on " + name);
        if (OnClick != null)
            OnClick(islandIndex);
    }

    void UpdateTooltip(int _Index)
    {

        tooltipTextTitle.text = currnetBoard.GetIslandName(_Index);
        tooltipText.text = "";
        e.IslandsType _islandType = currnetBoard.GetIslandType(_Index);
        //Update the tooltip UI by island type only
        if (_islandType == e.IslandsType.Residence || _islandType == e.IslandsType.Company)
        { 
            if (_islandType == e.IslandsType.Residence)
            {
                tooltipText.text += currnetBoard.GetIslandType(_Index) + "\n";
            }
            e.Player owner = currnetBoard.GetIslandOwner(_Index);
            string _OwnerName = SC_GameLogic.Instance.GetOwnerPlayerName(owner);

            tooltipText.text += "Status: " + currnetBoard.GetIslandStatus(_Index) +
                                 "\nPrice: U" + currnetBoard.GetIslandSellingPrice(_Index).ToString("n0") +
                                 "\nRent: U" + currnetBoard.GetIslandRentalPrice(_Index).ToString("n0") +
                                 "\nOwner: " + _OwnerName;
        }
        else if (_islandType == e.IslandsType.Start)
            tooltipText.text += "\nIf you get here, you'll get U 10,000";

        else if (_islandType == e.IslandsType.Prison)
        {
            tooltipText.text += "\nIf you get here, you'll have to pay a U 20,000 bail";
        }
    }

    void OnMouseOver()
    {
        Vector3 pos; //Position correction, because of the difference between the canvas and the SPRITE
        UpdateTooltip(islandIndex);
        ShowTooltip();

        //top islands
        if (name == "Island_Aera (6)" || name == "Island_Aera (8)" || name == "Island_Aera (9)")
            pos = new Vector3(-440f, -450f, 0);
        else
            pos = new Vector3(-440f, -300f, 0);
        tooltipPostion.localPosition = islandPostion.position + pos;
    }

    void OnMouseExit()
    {
        HideTooltip();
    }

     void ShowTooltip()
    {
        Tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        Tooltip.SetActive(false);
    }
}

