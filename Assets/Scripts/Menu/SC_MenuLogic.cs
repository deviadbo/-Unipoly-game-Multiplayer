using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using G = SC_Globals;

/*********************************
* Project: 	Unipoly      		 *
* Menu Logic Script    		     *
*********************************/
public class SC_MenuLogic : MonoBehaviour
{
    //Dictionary for Game Objects
    private Dictionary<string, GameObject> unityObjects;

    //Screens
    private SC_Enums.Screens currentScreen;
    private SC_Enums.Screens prevScreen;
    private Stack<SC_Enums.Screens> prevScreensStack = new Stack<SC_Enums.Screens>();

    private bool pushToStack = true;
    #region Singleton - SC_MenuLogic
    private static SC_MenuLogic instance;
    public static SC_MenuLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_MenuLogic").GetComponent<SC_MenuLogic>();
            return instance;
        }
    }
    #endregion

    #region Monobehaviour
    void Awake()  //Calls one's
    {
        Init();
    }

    void Update()
    {
        //Hide the Button_MonoBack in the MainMenu Screen
        if (currentScreen == SC_Enums.Screens.MainMenu)
            unityObjects["Button_MonoBack"].SetActive(false);
        else
            unityObjects["Button_MonoBack"].SetActive(true);
        //end if

        //Hide the Button_Opts in the Options Screen
        if (currentScreen == SC_Enums.Screens.Options)
        {
            unityObjects["Button_Opts"].SetActive(false);
        }
        else
        {
            unityObjects["Button_Opts"].SetActive(true);
        }
        //end if

        //Hide the Game Objects in other screens
        if (currentScreen == SC_Enums.Screens.BoardGame)
        {
            unityObjects["Game"].SetActive(true);
        }
        else
        {
            unityObjects["Game"].SetActive(false);
        }
        //end if

        if (G.GoToMainMenu == true)
        {
            //Go back to menu
            Button_MonoBack();
            Button_MonoBack();
            G.GoToMainMenu = false;
        }
    }
    #endregion

    #region Controller

    public void Button_Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }
    public void Button_SinglePlayer()
    {
        //Debug.Log("SC_MenuLogic Button_SinglePlayer Pressed");
        Debug.Log("Single Player Mode");

        //Announces to GameLogic to Call to RestartGame()
        G.RestartGame = true;

        SC_Globals.gameMode = SC_Enums.GameMode.SinglePlayer;
        Start_Game_MoveToBoard();
    }

    public void Button_play()
    {
        //Debug.Log("SC_MenuLogic Button_play Pressed");
    }

    public void Button_Multiplayer()
    {
        //Debug.Log("SC_MenuLogic Button_Multiplayer Pressed");
        SC_Globals.gameMode = SC_Enums.GameMode.MultiPlayer;
        Debug.Log("Multiplayer Mode");
        //Init Multiplayer
        InitMultiplayerScreen();
        ChangeToScreen(SC_Enums.Screens.Multiplayer);
    }

    public void Button_Opts()
    {
        //Debug.Log("SC_MenuLogic Button_Opts Pressed");
        ChangeToScreen(SC_Enums.Screens.Options);
    }

    public void Button_info()
    {
        //Debug.Log("SC_MenuLogic Button_info Pressed");
        ChangeToScreen(SC_Enums.Screens.StudentInfo);
    }

    public void Button_MonoBack()
    {
        //Debug.Log("SC_MenuLogic Button_MonoBack Pressed");
        //If you press the Back button, do not save the next screen in the stack.
        pushToStack = false;
        if (prevScreensStack.Count > 0)
        {
            //Debug.Log("Go to Prev Screen= " + prevScreensStack.Peek());
            ChangeToScreen(prevScreensStack.Pop());
        }

    }

    public void Slider_MpBet()
    {
        int _value = Mathf.RoundToInt(unityObjects["Slider_MpBet"].GetComponent<Slider>().value);
        unityObjects["Text_SliderVal"].GetComponent<Text>().text = _value.ToString();
        //Debug.Log("SC_MenuLogic Slider_MpBet, Value: " + _value);
    }

    public void Slider_opt_music()
    {
        float _value = unityObjects["Slider_opt_music"].GetComponent<Slider>().value;
        unityObjects["background_Music"].GetComponent<AudioSource>().volume = _value;
    }

    public void Slider_opt_sfx()
    {
        float _value = unityObjects["Slider_opt_sfx"].GetComponent<Slider>().value;
        unityObjects["Sound_Coin"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Jump"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Cube"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Jail"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Popup"].GetComponent<AudioSource>().volume = _value;

        unityObjects["Sound_BuyNews"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_BuildNews"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_PrmitNews"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_PrisonNews"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Build"].GetComponent<AudioSource>().volume = _value;
        unityObjects["Sound_Winner"].GetComponent<AudioSource>().volume = _value;

        PlaySound("Coin");
        //Debug.Log("SC_MenuLogic Slider_opt_sfx, Value: " + _value);
    }



    public void Button_Linkedin()
    {
        //Debug.Log("SC_MenuLogic Button_Linkedin Pressed");
        Application.OpenURL("https://www.linkedin.com/in/deviadbo/");
    }

    public void Button_git()
    {
        //Debug.Log("SC_MenuLogic Button_git Pressed");
        Application.OpenURL("https://github.com/deviadb");
    }

    #endregion


    #region Logic

    public void PlaySound(string _sound)
    {
        unityObjects["Sound_" + _sound].GetComponent<AudioSource>().Play();
    }

    private void Init()
    {
        currentScreen = SC_Enums.Screens.MainMenu;

        //Remove line when finish
        //currentScreen = SC_Enums.Screens.BoardGame;


        //Debug.Log("currentScreen init= " + currentScreen);
        prevScreensStack.Clear();
        prevScreensStack.Push(currentScreen);

        /*if (prevScreensStack.Count > 0)
            Debug.Log("prevScreensStack TOP= " + prevScreensStack.Peek());
        */

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _objs = GameObject.FindGameObjectsWithTag("UnityObjects");
        foreach (GameObject g in _objs)
            unityObjects.Add(g.name, g);

        //Debug.Log("unityObjects Count = " + unityObjects.Count);

        //Hide other screens
        unityObjects["Screen_MainMenu"].SetActive(false);
        unityObjects["Screen_Multiplayer"].SetActive(false);
        unityObjects["Screen_Options"].SetActive(false);
        unityObjects["Screen_StudentInfo"].SetActive(false);
        unityObjects["Screen_BoardGame"].SetActive(false);

        //Show currentScreen
        unityObjects["Screen_" + currentScreen].SetActive(true);


        //Quiet music
        float Volume = 0.1f;
        unityObjects["background_Music"].GetComponent<AudioSource>().volume = Volume;
        unityObjects["Slider_opt_music"].GetComponent<Slider>().value = Volume;
    }

    private void ChangeToScreen(SC_Enums.Screens _ToScreen)
    {
        unityObjects["Screen_" + _ToScreen].SetActive(true);
        unityObjects["Screen_" + currentScreen].SetActive(false);

        if (pushToStack)
            prevScreensStack.Push(currentScreen);
        //else
        //  Debug.Log("ChangeToScreen: Back button pressed");


        /*Debug.Log("Now i'm in " + _ToScreen + " from " + currentScreen);
        if (prevScreensStack.Count > 0)
            Debug.Log("prevScreensStack TOP (prev)= " + prevScreensStack.Peek());
        */

        currentScreen = _ToScreen;
        pushToStack = true;
    }


    //Because there are situations where the connection to the server hangs,
    //this function is designed to reset everything. And start over.
    private void InitMultiplayerScreen()
    {
        Debug.Log("Open Multiplayer Screen");

        //Disconnect connection to the server hangs
        SC_MultiplayerLogic.Instance.WarpClient_Disconnect();

        unityObjects["emailInputField"].GetComponent<InputField>().interactable = true;
        unityObjects["Slider_MpBet"].GetComponent<Slider>().interactable = true;
        unityObjects["ButtonPlayMP"].GetComponent<Button>().interactable = true;
        unityObjects["InputFieldNameMP"].GetComponent<InputField>().interactable = true;

        int rnd = UnityEngine.Random.Range(1, 1000);
        unityObjects["emailInputField"].GetComponent<InputField>().text = "user" + rnd.ToString() + "@unipoly.com";
        unityObjects["TextUserID"].GetComponent<Text>().text = "";
        unityObjects["TextRoomID"].GetComponent<Text>().text = "";
        unityObjects["TextStatus"].GetComponent<Text>().text = "";

        //G.MyName = "";
        //G.GusetName = "";
        //G.PlayerIdentity = SC_Enums.Player.Nobody; 
}

    public void Start_Game_MoveToBoard()
    {
        ChangeToScreen(SC_Enums.Screens.BoardGame);
    }
    #endregion

}
