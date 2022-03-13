using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_GameController : MonoBehaviour
{
    public void Button_Roll()
    {
        SC_GameLogic.Instance.Button_Roll();
    }

    public void Button_Build()
    {
        SC_GameLogic.Instance.Button_Build();
    }

    public void Button_BuyCompany()
    {
        SC_GameLogic.Instance.Button_BuyCompany();
    }

    public void Button_BuyIsland()
    {
        SC_GameLogic.Instance.Button_BuyIsland();
    }

    public void Button_PayRent()
    {
        SC_GameLogic.Instance.Button_PayRent();
    }

    public void Button_Yes()
    {
        SC_GameLogic.Instance.Button_Yes();
    }

    public void Button_No()
    {
        SC_GameLogic.Instance.Button_No();
    }

    public void Button_OK()
    {
        SC_GameLogic.Instance.Button_OK();
    }

    public void Button_OK_NameInput()
    {
        SC_GameLogic.Instance.Button_OK_NameInput();
    }

    public void Print_Button()
    {
        SC_GameLogic.Instance.Print_Button();
    }

    public void Button_Cash()
    {
        SC_GameLogic.Instance.Button_Cash();
    }
}
