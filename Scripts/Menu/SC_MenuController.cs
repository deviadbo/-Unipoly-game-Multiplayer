using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*********************************
* Project: 	Unipoly      		 *
* Menu Controller Script         *
*********************************/

public class SC_MenuController : MonoBehaviour
{

    public void Button_Quit()
    {
        SC_MenuLogic.Instance.Button_Quit();
    }

    public void Button_SinglePlayer()
    {
        SC_MenuLogic.Instance.Button_SinglePlayer();
    }

    public void Button_Multiplayer()
    {
        SC_MenuLogic.Instance.Button_Multiplayer();
    }

    public void Button_Opts()
    {
        SC_MenuLogic.Instance.Button_Opts();
    }

    public void Button_info()
    {
        SC_MenuLogic.Instance.Button_info();
    }

    public void Button_MonoBack()
    {
        SC_MenuLogic.Instance.Button_MonoBack();
    }

    public void Slider_MpBet()
    {
        SC_MenuLogic.Instance.Slider_MpBet();
    }

    public void Slider_opt_music()
    {
        SC_MenuLogic.Instance.Slider_opt_music();
    }

    public void Slider_opt_sfx()
    {
        SC_MenuLogic.Instance.Slider_opt_sfx();
    }

    public void Button_Linkedin()
    {
        SC_MenuLogic.Instance.Button_Linkedin();
    }
    public void Button_git()
    {
        SC_MenuLogic.Instance.Button_git();
    }

    public void Play_Game()
    {
        SC_MenuLogic.Instance.Button_play();
    }

}
