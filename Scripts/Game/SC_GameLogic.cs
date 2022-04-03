using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using e = SC_Enums;
using G = SC_Globals;
using System;
using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;


public class SC_GameLogic : MonoBehaviour
{
    private string nextTurn;
    private float startTime = 0;
    private int guestCube = 0;

    //Multiplayer - Auxiliary variable
    private bool I_wantToRestart = false;
    private bool OpponentWantRestart = false;

    const int maxValue = 6;
    int RADIUS_SPACE = 30;

    //Dictionary for Game Objects
    private Dictionary<string, GameObject> boardgameObjects = null;

    public SC_UniplayBorad curBoard;

    private SC_UniPlayer Player1;
    private SC_UniPlayer Player2;

    private SC_UniPlayer CurrentPlayer;

    int MAX_CASH_WINNER = 1000000; //Maximum account status for the winner
    int MAX_CUBE_ROLL = 7; //A variable that is responsible for the number of "rolls" of the cube
    double PROFIT_MULTIPLIER = 0.07;
    int cube_random_number = 0; //A variable that holds the number of the cube

    e.Buttons CurrrentButton = e.Buttons.none;
    private bool Gameover = false;

    public SpriteRenderer CubeSprite = null;
    public Sprite[] spriteCubeArray;

    public SpriteRenderer Gamebackground;
    public Sprite[] spriteGamebackgroundArray;

    //TESTING HELPER VAR
    bool TEST_MODE = false;
    int TEST_X_JUMP = 1;   //In each turn jump X islands

    //Buttons refernce
    private Button Button_buyIsland;
    private Button Button_payRent;
    private Button Button_buyCompany;
    private Button Button_build;
    private Button Button_Roll_Obj;

    //Couner for Owner marks
    private int markCounter = 0;

    private bool LockNameInput = false;

    #region Singleton - SC_GameLogic
    private static SC_GameLogic instance;
    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_GameLogic").GetComponent<SC_GameLogic>();
            return instance;
        }
    }
    #endregion


    #region MonoBehaviour
    private void OnEnable()
    {
        SC_IslandObject.OnClick += OnClick;
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnChatReceived += OnChatReceived;
        Listener.OnUserLeftRoom += OnUserLeftRoom;
    }

    private void OnDisable()
    {
        SC_IslandObject.OnClick -= OnClick;
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnChatReceived -= OnChatReceived;
        Listener.OnUserLeftRoom -= OnUserLeftRoom;
    }

    void Awake()  //Calls one's
    {
        Init();
        InitPlayerUI();
        UpdateBoard(0);
        ChangeBackground();
        //TestMode();  //Announces if I am in testing mode       
        PopNameInput();
        //StartGame()   // Call's after Name inupt
    }

    void Update()  //Calls one's
    {


        //Get update from the Menu Logic
        if (G.RestartGame == true)
        {
            G.RestartGame = false;
            RestartGame();
        }

        if (boardgameObjects["PopupBoxYesNo"].activeSelf || boardgameObjects["PopupBoxOK"].activeSelf ||
           boardgameObjects["PopupBoxNameInput"].activeSelf)
        {
            HideTooltip();
        }


        /* if (CurrentPlayer == Player1)
            TEST_X_JUMP = int.Parse(boardgameObjects["InputField"].GetComponent<InputField>().text);
        else TEST_X_JUMP = int.Parse(boardgameObjects["InputField"].GetComponent<InputField>().text); //Can be change for testing
        */

        //cube_random_number = int.Parse(boardgameObjects["InputField"].GetComponent<InputField>().text);

        if (Gameover == true)
            LockButton(Button_Roll_Obj);

        if (G.gameMode == e.GameMode.MultiPlayer)
        {
            HidePopup("NameInput");
            boardgameObjects["TextUserID0"].GetComponent<Text>().text = G.MyName + "(" + G.userID + ")";
        }
        else
        {
            boardgameObjects["timer_text"].GetComponent<Text>().text = "";
            boardgameObjects["TextUserID0"].GetComponent<Text>().text = "";
        }


        if (Gameover == false && G.gameMode == e.GameMode.MultiPlayer)
        {
            int _current = G.MAX_TURN_TIME - (int)(Time.time - startTime);
            if (_current < 0)
                boardgameObjects["timer_text"].GetComponent<Text>().text = "0";
            else boardgameObjects["timer_text"].GetComponent<Text>().text = _current.ToString();
        }

        if (I_wantToRestart == true && OpponentWantRestart == true)
        {
            I_wantToRestart = false;
            OpponentWantRestart = false;
            HidePopup("YesNo");
            WarpClient.GetInstance().startGame();
            Debug.Log("I_wantToRestart yes && OpponentWantRestart yes");
        }
    }
    #endregion

    #region Controller
    public void Button_Roll()
    {
        PlaySound("Cube");
        LockButton(Button_Roll_Obj);
        CleanCard();
        Text roll_t = boardgameObjects["roll_Text"].GetComponent<Text>();
        Vector3 v1 = boardgameObjects["Gambling_cube"].GetComponent<Transform>().position;
        for (int i = 0; i <= MAX_CUBE_ROLL; i++)
            StartCoroutine(RollCube_Animation(roll_t, i, v1));

        StartCoroutine(MovePlayerToIsland());
    }

    /// <summary>
    /// The Function Lock the input button
    /// </summary>
    private void LockButton(Button btn)
    {
        btn.interactable = false;
    }

    /// <summary>
    /// The Function Unlock the input button
    /// </summary>
    private void UnlockButton(Button btn)
    {
        FlashButton(btn);
        btn.interactable = true;
    }

    /*     Lower Buttons    */
    public void Button_BuyIsland()
    {
        CurrrentButton = e.Buttons.Button_BuyIsland;
        int _ilndIndx = CurrentPlayer.GetReallyIsland();
        string title = $"BUY ISLAND - {curBoard.GetIslandName(_ilndIndx)}";
        string text = $"Hi {CurrentPlayer.player_name}!\nDo you to buy {curBoard.GetIslandName(_ilndIndx)}?\nThe price is U{curBoard.GetIslandSellingPrice(_ilndIndx).ToString("n0")}";
        ShowPopup(title, text, "YesNo");
        LockButton(Button_buyIsland);
    }

    public void Button_BuyCompany()
    {
        CurrrentButton = e.Buttons.Button_BuyCompany;
        Debug.Log("Button_BuyCompany click");

        int _ilndIndx = CurrentPlayer.GetReallyIsland();
        string title = $"BUY COMPANY - {curBoard.GetIslandName(_ilndIndx)}";
        string text = $"Hi {CurrentPlayer.player_name}!\nDo you to buy {curBoard.GetIslandName(_ilndIndx)}?\nThe price is U{curBoard.GetIslandSellingPrice(_ilndIndx).ToString("n0")}";
        ShowPopup(title, text, "YesNo");
        LockButton(Button_buyCompany);
    }

    public void Button_Build()
    {
        CurrrentButton = e.Buttons.Button_Build;
        Debug.Log("Button_Build click");
        int _ilndIndx = CurrentPlayer.GetReallyIsland();
        string title = $"BUILDING CONSTRUCTION - {curBoard.GetIslandName(_ilndIndx)}";
        string text = $"Hi {CurrentPlayer.player_name}!\nDo you to build a building in {curBoard.GetIslandName(_ilndIndx)}?\nThe price is U{curBoard.GetIslandSellingPrice(_ilndIndx).ToString("n0")}";
        ShowPopup(title, text, "YesNo");
        LockButton(Button_build);
    }

    public void Button_PayRent()
    {
        CurrrentButton = e.Buttons.Button_PayRent;
        int _ilndIndx = CurrentPlayer.GetReallyIsland();
        string ownerName = GetOwnerPlayerName(curBoard.GetIslandOwner(_ilndIndx));

        string title = $"RENT PAYMENT - {curBoard.GetIslandName(_ilndIndx)}";
        string text = $"Hi {CurrentPlayer.player_name}!\nYou have to pay {ownerName}\nthe owner of {curBoard.GetIslandName(_ilndIndx)} U{curBoard.GetIslandRentalPrice(_ilndIndx).ToString("n0")}";
        ShowPopup(title, text, "OK");
        Debug.Log("Button_PayRent click");
        LockButton(Button_payRent);
    }

    /*  Popup Buttons */

    /// <summary>
    /// A function that is activated by pressing YES in the popup,
    /// and refers to the execution of the relevant operation before the button that activated the popup.
    /// </summary>
    public void Button_Yes()
    {
        Debug.Log("Button_Yes click");

        if (CurrrentButton == e.Buttons.Button_BuyIsland)
        {
            Buy_Island_Logic();
        }
        else if (CurrrentButton == e.Buttons.Button_BuyCompany)
        {
            Buy_Company_Logic();
        }

        /* Pay_Rent_Logic(); is only with OK popup*/

        else if (CurrrentButton == e.Buttons.Button_Build)
        {
            Build_Logic();
        }
        // if have'nt button refernce, is mean the popup is of a Winner popup
        else
        {
            //The Single Player want to RestartGame
            if (G.gameMode == e.GameMode.SinglePlayer)
            {
                RestartGame();
            }
            //The Player in the MultiPlayer want to RestartGame
            else if (G.gameMode == e.GameMode.MultiPlayer && Gameover == true)
            {
                Debug.Log("i want to restart. send to oponnent chat");
                HidePopup("YesNO");
                I_wantToRestart = true;
                SendInfoChat("Restart", "Yes");
                //The Restart will turn-on from Update()
            }
        }
    }

    public void Button_No()
    {
        Debug.Log("Button_No click");
        if (Gameover == false)
        {
            CurrrentButton = e.Buttons.none;
            ClosePopupAndMoveTurn();
        }
        else if (Gameover == true)
        {
            if (G.gameMode == e.GameMode.SinglePlayer)
            {
                Debug.Log("No want to restart. Bye!");
                HidePopup("YesNO");
                GoToMainMenu();
                //QuitGame();
            }
            else if (G.gameMode == e.GameMode.MultiPlayer)
            {
                //If MultiPlayer mode -> stopGame
                Debug.Log("MultiPlayer - I dont want to restart. Bye!");
                I_wantToRestart = false;
                HidePopup("YesNO");
                SendInfoChat("Restart", "No");
                GoToMainMenu();
                //QuitGame();
            }
        }
    }

    //Popop OK Logic
    public void Button_OK()
    {
        Debug.Log("Button_OK click");
        int _Index = CurrentPlayer.GetReallyIsland();
        string islandName = curBoard.GetIslandName(_Index);

        if (CurrrentButton == e.Buttons.Button_PayRent)
        {
            Pay_Rent_Logic();
        }
        else if (islandName == "Prison")
        {
            Prison_Logic();
        }
        else if (islandName == "Start Island")
        {
            //Debug.Log("Button_OK click start");
            Start_Logic();
        }
        else if (CurrrentButton == e.Buttons.Dividend)
        {
            Dividends_Logic();
        }
        else
        //Get here IF - "Not allowed to build"  OR "Have no money"
        {
            ClosePopupAndMoveTurn();
        }
    }

    //Works in Single Player Mode
    public void Button_OK_NameInput()
    {
        Debug.Log("Button_OK_NameInput click");
        // Player 1 name
        if (LockNameInput == false)
        {
            Player1.player_name = boardgameObjects["NameInputField"].GetComponent<InputField>().text;
            HidePopup("NameInput");
            boardgameObjects["NameInputField"].GetComponent<InputField>().text = "";
            LockNameInput = true;
            boardgameObjects["Player1_Name"].GetComponent<Text>().text = Player1.player_name;
            ShowPopup("Hello Player 2!", "Enter Your name: ", "NameInput");
        }
        else //player 2
        {
            Player2.player_name = boardgameObjects["NameInputField"].GetComponent<InputField>().text;
            HidePopup("NameInput");
            boardgameObjects["Player2_Name"].GetComponent<Text>().text = Player2.player_name;
            /**/       //Start Game
            StartGame();
        }
    }

    /*   **********    */
    public void Print_Button()
    {
        curBoard.PrintIslandStatus();
    }

    public void Button_Cash()
    {
        string _text = "";
        _text += Player1.player_name + " value is  U " + CashLogic(Player1).ToString("n0") + "\n\n\n";
        _text += Player2.player_name + " value is  U " + CashLogic(Player2).ToString("n0");

        ShowPopup("Players Value", _text, "Cash");

        Debug.Log(Player1.player_name + " value is " + CashLogic(Player1).ToString("n0"));
        Debug.Log(Player2.player_name + " value is " + CashLogic(Player2).ToString("n0"));
    }

    #endregion Controller

    #region Logic
    void Init()
    {
        //by defult SinglePlayer
        //G.gameMode = e.GameMode.SinglePlayer;

        //Avoid form re-init
        if (boardgameObjects == null)
        {
            boardgameObjects = new Dictionary<string, GameObject>();
            GameObject[] _objs = GameObject.FindGameObjectsWithTag("BoardgameObjects");
            foreach (GameObject g in _objs)
                boardgameObjects.Add(g.name, g);


            CubeSprite = boardgameObjects["Gambling_cube"].GetComponent<SpriteRenderer>();
            spriteCubeArray = Resources.LoadAll<Sprite>("Sprites/cube");

            Gamebackground = boardgameObjects["BoardgameBackground"].GetComponent<SpriteRenderer>();
            spriteGamebackgroundArray = Resources.LoadAll<Sprite>("Background");
        }

        curBoard = new SC_UniplayBorad();
        /**/
        //                         id                   name       blance  currIsland                                    
        Player1 = new SC_UniPlayer(1, e.Player.Player1, "Player1", 750000, 0);
        Player2 = new SC_UniPlayer(2, e.Player.Player2, "Player2", 750000, 0);

        CurrentPlayer = Player1;

        Button_buyIsland = boardgameObjects["Button_BuyIsland"].GetComponent<Button>();
        Button_payRent = boardgameObjects["Button_PayRent"].GetComponent<Button>();
        Button_buyCompany = boardgameObjects["Button_BuyCompany"].GetComponent<Button>();
        Button_build = boardgameObjects["Button_Build"].GetComponent<Button>();
        Button_Roll_Obj = boardgameObjects["Button_Roll"].GetComponent<Button>();
        LockButton(Button_build);
        LockButton(Button_buyIsland);
        LockButton(Button_payRent);
        LockButton(Button_buyCompany);

        HidePopup("YesNo");
        HidePopup("OK");
        HidePopup("NameInput");
        HidePopup("Cash");
    }

    private void ShowPopup(string _title, string _text, string _type)
    {
        boardgameObjects["PopupBox" + _type].SetActive(true);
        boardgameObjects["PopupTitle" + _type].GetComponent<Text>().text = _title;
        boardgameObjects["PopupText" + _type].GetComponent<Text>().text = _text;

        if (_type != "NameInput")
            PlaySound("Popup");

        if (_type == "Cash")
            StartCoroutine(HideCashPopup());
    }

    IEnumerator HideCashPopup()
    {
        yield return new WaitForSeconds(3);
        HidePopup("Cash");
    }
    private void PopNameInput()
    {
        //Name Input
        ShowPopup("Hello Player 1!", "Enter Your name: ", "NameInput");
    }

    private void StartGame()
    {
        UpdateBoard(0);
        WakePlayer();
        FlashPlayerName(CurrentPlayer);
        FlashButton(Button_Roll_Obj);
    }

    private void HidePopup(string _type)
    {
        if (Gameover == false)
            boardgameObjects["PopupBox" + _type].SetActive(false);
    }

    //Announces if I am in testing mode
    private void TestMode()
    {
        if (TEST_MODE)
        {
            MAX_CUBE_ROLL = 1;
            boardgameObjects["Print_Button"].SetActive(true);
            Debug.Log("Test Mode is ACTIVE, it's mean that the cube is roll faster and the step of the cube is " + TEST_X_JUMP);
        }
        else
        {
            boardgameObjects["Print_Button"].SetActive(false);
            boardgameObjects["InputField"].SetActive(false);
            Debug.Log("Test Mode is OFF. The Game works as usual");
        }

    }

    /*
     A function that is responsible for updating the UI of the board,
     as well as the data of the current island according to the index.
    */
    private void UpdateBoard(int _Index)
    {
        //Update Island Card
        Text cardTitle = boardgameObjects["CardTitle"].GetComponent<Text>();
        Text cardText = boardgameObjects["IslandDet"].GetComponent<Text>();
        cardText.text = "";
        cardTitle.text = curBoard.GetIslandName(_Index);

        e.IslandsType _islandType = curBoard.GetIslandType(_Index);
        string _OwnerName = GetOwnerPlayerName(curBoard.GetIslandOwner(_Index));

        //Update the board UI by island type only
        //In case of Start or Prison, a balance update function is also enabled.
        if (_islandType == e.IslandsType.Residence || _islandType == e.IslandsType.Company)
        {

            //Extra for ISLAND
            if (_islandType == e.IslandsType.Residence)
            {
                cardText.text = cardText.text = "City: " + curBoard.GetIslandCity(_Index)
                                + "\nType: " + curBoard.GetIslandType(_Index);
            }
            //Text for Com_ and island



            cardText.text = cardText.text + "\nStatus: " + curBoard.GetIslandStatus(_Index) +
                            "\nPrice Tag:  U" + curBoard.GetIslandSellingPrice(_Index).ToString("n0") +
                            "\nRent Price: U" + curBoard.GetIslandRentalPrice(_Index).ToString("n0") +
                            "\nOwner: " + _OwnerName;
        }
        else if (_islandType == e.IslandsType.Start)
        {
            CurrrentButton = e.Buttons.none;
            if (CurrentPlayer.player_currIsland == 0)
                cardText.text = CurrentPlayer.player_name + "\nEnjoy your trip!";
            else
            {
                cardText.text = CurrentPlayer.player_name + " Enjoy your trip!\nGet 10,000 coins!";

                //If Single Player, Show popup. else if Multiplayer - this is my turn
                //if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
                //  || G.gameMode == e.GameMode.SinglePlayer)
                if ((G.userID == nextTurn && G.gameMode == e.GameMode.MultiPlayer)
               || G.gameMode == e.GameMode.SinglePlayer)
                { Start_Popup(); }
            }
        }
        else if (_islandType == e.IslandsType.Prison)
        {
            CurrrentButton = e.Buttons.none;
            cardText.text = "Bad news!\nPay U20,000 bail.";
            //If Single Player, Show popup. else if Multiplayer - this is my turn

            //if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            // || G.gameMode == e.GameMode.SinglePlayer)
            if ((G.userID == nextTurn && G.gameMode == e.GameMode.MultiPlayer)
                || G.gameMode == e.GameMode.SinglePlayer)
            { Prison_Popup(); }
        }

        //Updates the UI buttons according to the status of the island

        //If Single Player, Show popup.
        //If Multiplayer - this is my turn, else do nothing
        if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
           || G.gameMode == e.GameMode.SinglePlayer)
        {
            e.IslandStatus _islandStatus = curBoard.GetIslandStatus(_Index);
            e.Player _islandOwner = curBoard.GetIslandOwner(_Index);

            if (_islandStatus == e.IslandStatus.Not_For_Sell)  //if Start OR Prison
            {
                CurrrentButton = e.Buttons.none;
                LockButton(Button_build);
                LockButton(Button_buyIsland);
                LockButton(Button_buyCompany);
                LockButton(Button_payRent);
            }

            //If it's for sale. Then activate the relevant button.
            else if (_islandStatus == e.IslandStatus.For_Sell)
            {
                if (_islandType == e.IslandsType.Residence)
                    UnlockButton(Button_buyIsland);
                else if (_islandType == e.IslandsType.Company)
                    UnlockButton(Button_buyCompany);
            }
            //If it's not for sale.
            //So check if the current player is the owner, if so then allow Build otherwise have to pay rent.
            else if ((_islandStatus == e.IslandStatus.Purchased || _islandStatus == e.IslandStatus.Build_Permit) && CurrentPlayer.player != _islandOwner)
                UnlockButton(Button_payRent);

            else if (_islandStatus == e.IslandStatus.Purchased && CurrentPlayer.player == _islandOwner && _islandType == e.IslandsType.Company)
                Dividends_Popup();

            else if (CurrentPlayer.player == _islandOwner && _islandType == e.IslandsType.Residence && (_islandStatus == e.IslandStatus.Build_Permit || _islandStatus == e.IslandStatus.Purchased))
                UnlockButton(Button_build);
        }
    }


    private void CleanCard()
    {
        //Clean Island Card
        Text cardTitle = boardgameObjects["CardTitle"].GetComponent<Text>();
        Text cardText = boardgameObjects["IslandDet"].GetComponent<Text>();
        cardText.text = "";
        cardTitle.text = "";
    }
    /*The function "rolls" the numbers.
     And calculates a random number between 1 and 6*/
    IEnumerator RollCube_Animation(Text roll_t, int sec, Vector3 v1)
    {
        int r = UnityEngine.Random.Range(1, maxValue);

        //Demo cube
        boardgameObjects["Gambling_cube"].GetComponent<Transform>().position = new Vector3(500, 380);
        boardgameObjects["Gambling_cube"].GetComponent<Transform>().localScale = new Vector3(90, 90);

        yield return new WaitForSeconds(sec / 4f);
        GamblingCubeDemo(r - 1);
        roll_t.text = r.ToString();
        if (sec == MAX_CUBE_ROLL)
        {
            cube_random_number = r;

            //Additon for MultiPlayer Game Mode

            //I'm not play, and get the cube from the server
            if (CurrentPlayer.player != G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            {
                cube_random_number = guestCube;
                //Debug.Log("guestCube=" + guestCube);

                //Chagne cube side to the guestCube
                if ((cube_random_number >= 1 && cube_random_number <= 6))
                    GamblingCubeDemo(cube_random_number - 1);
            }
            //if i'm playing, I send chat with the cube
            else if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            {
                SendInfoChat("CubeRoll", cube_random_number);
            }
            //Debug.Log("Roll Random Number = " + cube_random_number.ToString());
            yield return new WaitForSeconds(0.9f);
            boardgameObjects["Gambling_cube"].GetComponent<Transform>().position = v1;
            boardgameObjects["Gambling_cube"].GetComponent<Transform>().localScale = new Vector3(20, 20, 0);
        }
    }

    /*The function is responsible for moving the current player to the next island,
      depending on the result of the cube.
      The function will operate after 6 seconds
    */
    IEnumerator MovePlayerToIsland()
    {
        yield return new WaitForSeconds(MAX_CUBE_ROLL / 2f);
        int _Index = 0;
        int currIsland = 0;
        int destIsland = 0;

        currIsland = CurrentPlayer.player_currIsland;

        CurrentPlayer.player_currIsland += cube_random_number;

        /*if (TEST_MODE == false)
        {
            CurrentPlayer.player_currIsland += cube_random_number;
        }
        else if (TEST_MODE == true)
            CurrentPlayer.player_currIsland += TEST_X_JUMP;
        */

        _Index = CurrentPlayer.player_currIsland;

        destIsland = _Index;

        //Player position refernce
        Transform player_pos = boardgameObjects[CurrentPlayer.player + "_Tool"].GetComponent<Transform>();

        /*Stores in the index = index % 19,
        To complete a round on the board*/
        _Index %= 19;

        if (boardgameObjects.ContainsKey("Island_Aera (" + _Index + ")"))
        {

            //Debug.Log("I need to go to island " + _Index);
            for (int i = currIsland + 1; i <= destIsland; i++)
            {
                int idx = i % 19;

                Vector3 DestinationIsland = boardgameObjects["Island_Aera (" + idx + ")"].GetComponent<Transform>().position;

                //Position Correction of the Game Tools
                if (CurrentPlayer == Player1)
                    DestinationIsland += new Vector3(10, 5, 0);
                else if (CurrentPlayer == Player2)
                    DestinationIsland += new Vector3(-10, -5, 0);


                int delta = i - currIsland;
                //Sends a reference of the object, to move it to the next island
                StartCoroutine(DisplacementEffect(player_pos, DestinationIsland, delta, i == destIsland));
            }
            //string tmp = curBoard.GetIslandName(_Index) + ": " + curBoard.GetIslandStatus(_Index);
            //Debug.Log("im in " + tmp);

            //Dealy the board update
            StartCoroutine(UpdateBoardDelay(cube_random_number, _Index));
            //UpdateBoard(_Index);
        }
    }



    //UI init of player names, and their balance
    private void InitPlayerUI()
    {
        //Debug.Log("Call InitPlayerUI");
        BlanceUI_Update();
        boardgameObjects["Player1_Name"].GetComponent<Text>().text = Player1.player_name;
        boardgameObjects["Player2_Name"].GetComponent<Text>().text = Player2.player_name;
    }


    //UI update of player balance
    private void BlanceUI_Update()
    {
        boardgameObjects["Player1_Balnce"].GetComponent<Text>().text = Player1.player_balnce.ToString("n0");
        boardgameObjects["Player2_Balnce"].GetComponent<Text>().text = Player2.player_balnce.ToString("n0");

        //For any change in balance, check who wins
        //Avoid from check after init.
        if (G.gameMode != e.GameMode.None)
            CheckWinner();
    }

    public string GetOwnerPlayerName(e.Player p)
    {
        if (Player1.player == p)
        {
            return Player1.player_name;
        }
        else if (Player2.player == p)
        {
            return Player2.player_name;
        }
        else
            return "Nobody";
    }

    private void SingleMovePlayerTurn()
    {
        if (G.gameMode == e.GameMode.SinglePlayer)
        {
            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
            }
            else if (CurrentPlayer == Player2)
            {
                CurrentPlayer = Player1;
            }
            Debug.Log(CurrentPlayer.player.ToString() + ":" + GetOwnerPlayerName(CurrentPlayer.player) + " turn");
            FlashPlayerName(CurrentPlayer);
            UnlockButton(Button_Roll_Obj);
        }
    }

    private void MultiplayerPassTurn()
    {
        //if (_sender == G.userID)
        HidePopup("OK");
        HidePopup("YesNo");

        startTime = Time.time;
        if (CurrentPlayer == Player1)
        {
            CurrentPlayer = Player2;
        }
        else if (CurrentPlayer == Player2)
        {
            CurrentPlayer = Player1;
        }

        //If i'm the CurrentPlayer -> UnlockButton Roll
        if (CurrentPlayer.player == G.PlayerIdentity)
        {
            UnlockButton(Button_Roll_Obj);
        }
        // If the CurrentPlayer is not me -> then lock all the buttons
        else if (CurrentPlayer.player != G.PlayerIdentity)
        {
            LockButton(Button_Roll_Obj);
            LockButton(Button_buyIsland);
            LockButton(Button_payRent);
            LockButton(Button_buyCompany);
            LockButton(Button_build);
        }

        Debug.Log("Multiplayer: " + CurrentPlayer.player.ToString() + " - " + GetOwnerPlayerName(CurrentPlayer.player) + " turn");
        FlashPlayerName(CurrentPlayer);
    }

    private void FlashButton(Button btn)
    {
        StartCoroutine(FlashButtonCoroutine(btn));
    }

    private void FlashButton(e.Buttons btn)
    {
        string btn_name = btn.ToString();
        Debug.Log("FlashButton: " + btn_name + "   --- " + btn);
        Button btn2 = boardgameObjects[btn_name].GetComponent<Button>();
        StartCoroutine(FlashButtonCoroutine(btn2));
    }

    IEnumerator FlashButtonCoroutine(Button btn)
    {
        for (int i = 0; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.2f);
            if (i % 2 == 0)
                btn.gameObject.SetActive(false);
            else
                btn.gameObject.SetActive(true);
        }
        btn.gameObject.SetActive(true);
    }

    private void FlashPlayerName(SC_UniPlayer p)
    {
        boardgameObjects["Player1_Name"].GetComponent<Text>().color = Color.black;
        boardgameObjects["Player2_Name"].GetComponent<Text>().color = Color.black;
        StartCoroutine(FlashPlayerNameCoroutine(p));
    }

    IEnumerator FlashPlayerNameCoroutine(SC_UniPlayer p)
    {
        string player = p.player.ToString();

        for (int i = 0; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.2f);
            if (i % 2 == 0)
            {
                boardgameObjects[player + "_Name"].GetComponent<Text>().color = Color.magenta;
                boardgameObjects[player + "_Name"].SetActive(false);
            }
            else if (i % 2 == 1)
            {
                boardgameObjects[player + "_Name"].SetActive(true);
            }
        }
        boardgameObjects[player + "_Name"].GetComponent<Text>().color = Color.red;
        boardgameObjects[player + "_Name"].SetActive(true);
    }

    //Function for simulating the movement of the game tool on each island
    IEnumerator DisplacementEffect(Transform source, Vector3 _dest, int _sec, bool _done)
    {
        yield return new WaitForSeconds(_sec / 2f);
        PlaySound("Jump");
        //done_move = _done;
        source.position = _dest;
    }

    IEnumerator UpdateBoardDelay(int delay, int _index)
    {
        yield return new WaitForSeconds(delay / 2f);
        UpdateBoard(_index);
    }


    IEnumerator ChangeBackgroundCoroutine()
    {
        float second = 5f;
        yield return new WaitForSeconds(second);
        Gamebackground.sprite = spriteGamebackgroundArray[1];
        yield return new WaitForSeconds(second);
        Gamebackground.sprite = spriteGamebackgroundArray[2];
        yield return new WaitForSeconds(second);
        Gamebackground.sprite = spriteGamebackgroundArray[0];
        StartCoroutine(ChangeBackgroundCoroutine());
    }

    void ChangeBackground()
    {
        StartCoroutine(ChangeBackgroundCoroutine());
    }

    IEnumerator WakePlayerCoroutine()
    {
        float second = 1f;
        yield return new WaitForSeconds(second);
        boardgameObjects[CurrentPlayer.player + "_Tool"].GetComponent<SpriteRenderer>().flipX = !boardgameObjects[CurrentPlayer.player + "_Tool"].GetComponent<SpriteRenderer>().flipX;
        yield return new WaitForSeconds(second);
        boardgameObjects[CurrentPlayer.player + "_Tool"].GetComponent<SpriteRenderer>().flipX = !boardgameObjects[CurrentPlayer.player + "_Tool"].GetComponent<SpriteRenderer>().flipX;
        StartCoroutine(WakePlayerCoroutine());
    }

    void WakePlayer()
    {
        StartCoroutine(WakePlayerCoroutine());
    }

    private void GamblingCubeDemo(int number) => CubeSprite.sprite = spriteCubeArray[number];

    private void UpdateCurrnetPlayerBalance(int money)
    {
        CurrentPlayer.player_balnce += money;
        BlanceUI_Update();
    }

    private void Update_Owner_PlayerBalance(int money, e.Player p)
    {

        SC_UniPlayer _owner = null;
        if (p == e.Player.Player1)
            _owner = Player1;
        else if (p == e.Player.Player2)
            _owner = Player2;
        else
            Debug.Log("Error: no owner");

        _owner.player_balnce += money;
        BlanceUI_Update();
    }

    /// <summary>
    /// The function returns a reference to a game object by name
    /// </summary>
    public GameObject GetGameObj(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        else if (boardgameObjects[name])
        {
            return boardgameObjects[name];
        }
        return null;
    }

    #endregion

    #region Buttons Actions Logic

    private void ClosePopupAndMoveTurn()
    {
        //If i'm the Current Player, I send Move
        if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
        {
            //I finish my turn
            SendMove_To_Opponent();
        }
        else if (G.gameMode == e.GameMode.SinglePlayer)
        {
            SingleMovePlayerTurn();
        }
        HidePopup("OK");
        HidePopup("YesNo");
        CurrrentButton = e.Buttons.none;
    }

    private void SendMove_To_Opponent()
    {
        HidePopup("OK");
        HidePopup("YesNo");

        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add("Move Done", CurrentPlayer.player_name + " done play"); //curBoard
        string _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().sendMove(_toJson);
    }
    private void Buy_Island_Logic()
    {
        int _Index = CurrentPlayer.GetReallyIsland();
        Debug.Log("you want to buy " + curBoard.GetIslandName(_Index));
        int currentBalnce = CurrentPlayer.player_balnce;
        int currentIslandPrice = curBoard.GetIslandSellingPrice(_Index);
        if ((currentBalnce - currentIslandPrice) >= 0)
        {
            curBoard.UpdateIsland(_Index, e.IslandStatus.Purchased, CurrentPlayer.player);
            StartCoroutine(Play_2_Sounds("Coin", "BuyNews", 1));
            UpdateCurrnetPlayerBalance(-curBoard.GetIslandSellingPrice(_Index));
            MarkIslandOwner(_Index);

            //If Multiplayer - this is my turn
            if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            {
                //Send chat Buy Island
                SendInfoChat("Buy_Island_Logic", _Index);
            }

            Debug.Log("Purchased complete");

            //Check if NOW allowed to build in city
            if (curBoard.BuildingPermitInCity(curBoard.GetIslandCity(_Index)) == true)
            {
                StartCoroutine(Play_2_Sounds("Coin", "PrmitNews", 7));
                Debug.Log("GOOD NEWS: Building Permit In City: " + curBoard.GetIslandCity(_Index));
            }

            ClosePopupAndMoveTurn();
        }
        else
        {
            //I the Current Player, and I have'nt enough money
            if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
                || G.gameMode == e.GameMode.SinglePlayer)
            {
                ShowPopup("BAD NEWS", "You Don't have enough money!!\nMake more money, and try again", "OK");
                Debug.Log("you have no money");
            }
        }
    }
    private void Buy_Company_Logic()
    {
        int _Index = CurrentPlayer.GetReallyIsland();

        //Debug.Log("Current Player want to buy company: " + curBoard.GetIslandName(_Index));
        int currentBalnce = CurrentPlayer.player_balnce;
        int currentIslandPrice = curBoard.GetIslandSellingPrice(_Index);
        if ((currentBalnce - currentIslandPrice) >= 0)
        {
            curBoard.UpdateIsland(_Index, e.IslandStatus.Purchased, CurrentPlayer.player);
            UpdateCurrnetPlayerBalance(-curBoard.GetIslandSellingPrice(_Index));
            Debug.Log("Purchased complete");
            MarkIslandOwner(_Index);
            MarkIslandBuilding(_Index, "Company");
            StartCoroutine(Play_2_Sounds("Coin", "BuyNews", 1));

            //If Multiplayer - this is my turn
            if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            {
                //Send chat Buy Island
                SendInfoChat("Buy_Company_Logic", _Index);
            }

            ClosePopupAndMoveTurn();
        }
        else
        {
            //I the Current Player, and I have'nt enough money
            if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            || G.gameMode == e.GameMode.SinglePlayer)
            {
                ShowPopup("BAD NEWS", "You Don't have enough money!!\nMake more money, and try again", "OK");
                Debug.Log("you have no money");
            }
        }
    }

    private void Pay_Rent_Logic()
    {
        int _Index = CurrentPlayer.GetReallyIsland();
        Debug.Log("You pay about " + curBoard.GetIslandName(_Index));

        int IslandRent = curBoard.GetIslandRentalPrice(_Index);

        //Transfers money between tenant and owner
        UpdateCurrnetPlayerBalance(-IslandRent);
        Update_Owner_PlayerBalance(IslandRent, curBoard.GetIslandOwner(_Index));
        PlaySound("Coin");
        Debug.Log("The rent " + IslandRent + " was paid to the owner");

        //If Multiplayer - this is my turn
        if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
        {
            //Send chat Pay_Rent_Logic
            SendInfoChat("Pay_Rent_Logic", _Index);
        }
        ClosePopupAndMoveTurn();
    }

    private void Build_Logic()
    {
        Debug.Log("** Enter to Build_Logic **");
        int _Index = CurrentPlayer.GetReallyIsland();
        Debug.Log(CurrentPlayer.GetPlayerName() + " want to build in " + curBoard.GetIslandName(_Index));
        int currentBalnce = CurrentPlayer.player_balnce;
        int currentIslandPrice = curBoard.GetIslandSellingPrice(_Index);


        if (curBoard.GetIslandStatus(_Index) != e.IslandStatus.Build_Permit)
        {
            //Check if allowed to build. If not, show a popup
            if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
                || G.gameMode == e.GameMode.SinglePlayer)
            {
                Not_allowed_to_Build_Logic();
            }
        }
        //Check if the player have money
        else if ((currentBalnce - currentIslandPrice) >= 0)
        {
            StartCoroutine(BuildSFX());
            UpdateCurrnetPlayerBalance(-curBoard.GetIslandSellingPrice(_Index));
            /*add building*/
            MarkIslandBuilding(_Index, "Residence");
            curBoard.NewBuilding(_Index);
            Debug.Log("Build complete");

            //If Multiplayer - this is my turn AND I update the Oponnent
            if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
            {
                //Send chat Build_Logic
                SendInfoChat("Build_Logic", _Index);
            }
            ClosePopupAndMoveTurn();
        }
        //I the Current Player, and I have'nt enough money
        else
        {
            if ((CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
              || G.gameMode == e.GameMode.SinglePlayer)
            {
                ShowPopup("BAD NEWS", "You Don't have enough money to build!!\nMake more money, and try again", "OK");
                Debug.Log("you have no money to build");
            }
        }
    }

    IEnumerator BuildSFX()
    {
        Debug.Log("BuildSFX");
        yield return new WaitForSeconds(1);
        PlaySound("Coin");
        yield return new WaitForSeconds(1);
        PlaySound("Build");
        yield return new WaitForSeconds(1);
        PlaySound("BuildNews");
    }


    IEnumerator Play_2_Sounds(string one, string two, int first_sec)
    {
        yield return new WaitForSeconds(first_sec);
        PlaySound(one);
        yield return new WaitForSeconds(1);
        PlaySound(two);
    }

    #endregion
    #region Popup Logic
    private void Dividends_Popup()
    {
        CurrrentButton = e.Buttons.Dividend;
        int _Index = CurrentPlayer.GetReallyIsland();
        string title = $"GOOD NEWS ABOUT: " + curBoard.GetIslandName(_Index);
        int dividend = ((int)(curBoard.GetIslandSellingPrice(_Index) * PROFIT_MULTIPLIER * cube_random_number));
        string text = $"Hi {CurrentPlayer.player_name}!\nThe company was very successful and as the owner\nyou received a dividend of U {dividend.ToString("n0")}";
        ShowPopup(title, text, "OK");
    }

    private void Dividends_Logic()
    {
        PlaySound("Coin");
        int _Index = CurrentPlayer.GetReallyIsland();
        int dividend = ((int)(curBoard.GetIslandSellingPrice(_Index) * PROFIT_MULTIPLIER * cube_random_number));
        UpdateCurrnetPlayerBalance(dividend);

        //If Multiplayer - this is my turn
        if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
        {
            //Send chat Dividends_Logic
            SendInfoChat("Dividends_Logic", _Index);
        }

        ClosePopupAndMoveTurn();
    }
    private void Prison_Popup()
    {
        CurrrentButton = e.Buttons.none;
        string title = $"RELEASE FROM PRISON";
        string text = $"Hi {CurrentPlayer.player_name}!\nYou have to pay U 20,000 bail\nto get out from prison";
        ShowPopup(title, text, "OK");
    }
    private void Prison_Logic()
    {
        StartCoroutine(Play_2_Sounds("Jail", "PrisonNews", 1));
        UpdateCurrnetPlayerBalance(-20000);

        //If Multiplayer - this is my turn
        if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
        {
            //Send chat Prison_Logic
            int _Index = 5; //Prison Island
            SendInfoChat("Prison_Logic", _Index);
        }

        ClosePopupAndMoveTurn();
    }

    private void Start_Popup()
    {
        CurrrentButton = e.Buttons.none;
        string title = $"GOOD NEWS";
        string text = $"Hi {CurrentPlayer.player_name}!\nEnjoy your trip!\nYou got U 10,000 to go";
        ShowPopup(title, text, "OK");
    }
    private void Start_Logic()
    {
        //Debug.Log("im in Start_Logic");
        PlaySound("Coin");
        UpdateCurrnetPlayerBalance(10000);

        //If Multiplayer - this is my turn
        if (CurrentPlayer.player == G.PlayerIdentity && G.gameMode == e.GameMode.MultiPlayer)
        {
            //Send chat Start_Logic
            int _Index = 0; //Start Island
            SendInfoChat("Start_Logic", _Index);
        }
        ClosePopupAndMoveTurn();
    }


    private void Not_allowed_to_Build_Logic()
    {
        ShowPopup("BAD NEWS", "You are not allowed to build,\nonly after the whole city is purchased,\nit is allowed to build.", "OK");
    }
    #endregion

    #region Extra Game Logic
    private void MarkIslandOwner(int _index)
    {
        //Take from Resources -> Prefabs -> OwnerMark
        //Use the folder to create objects while running, and I do not need to hold references
        GameObject _mark = Instantiate(Resources.Load("Prefabs/OwnerMark")) as GameObject;
        if (_mark != null)
        {
            Vector3 DestinationIsland = boardgameObjects["Island_Aera (" + _index + ")"].GetComponent<Transform>().position;

            _mark.name = "IslandMark" + markCounter++;

            int _posY = 30;

            _mark.transform.position = new Vector3(0, _posY, 0) + DestinationIsland;

            Sprite[] spriteMarklArray = Resources.LoadAll<Sprite>("Sprites/Marks");
            if (CurrentPlayer == Player1)
                _mark.GetComponent<SpriteRenderer>().sprite = spriteMarklArray[0];
            else if (CurrentPlayer == Player2)
                _mark.GetComponent<SpriteRenderer>().sprite = spriteMarklArray[1];

            _mark.transform.tag = "Runtime_Mark";
            if (boardgameObjects.ContainsKey("IslandsMarks"))
            {
                _mark.transform.SetParent(boardgameObjects["IslandsMarks"].transform);
            }
        }

    }

    private void MarkIslandBuilding(int _index, string _type)
    {
        //Take from Resources -> Prefabs
        //Use the folder to create objects while running, and I do not need to hold references

        //Defult Residence
        GameObject _mark = Instantiate(Resources.Load("Prefabs/sprite_building")) as GameObject;

        if (_type == "Company")
        {
            _mark = Instantiate(Resources.Load("Prefabs/sprite_buildingCom")) as GameObject;
            RADIUS_SPACE = 20;
        }
        else if (_type == "Residence")
        {
            //For each player diffent house icon
            Sprite[] spriteHouseArray = Resources.LoadAll<Sprite>("Sprites/House");
            if (CurrentPlayer == Player1)
                _mark.GetComponent<SpriteRenderer>().sprite = spriteHouseArray[0];
            else if (CurrentPlayer == Player2)
                _mark.GetComponent<SpriteRenderer>().sprite = spriteHouseArray[1];
        }
        if (_mark != null)
        {
            Vector3 DestinationIsland = boardgameObjects["Island_Aera (" + _index + ")"].GetComponent<Transform>().position;

            _mark.name = "Building" + _type + markCounter++;

            int _posX = UnityEngine.Random.Range(-RADIUS_SPACE, RADIUS_SPACE);
            int _posY = UnityEngine.Random.Range(-RADIUS_SPACE, RADIUS_SPACE);

            _mark.transform.position = new Vector3(_posX, _posY, 0) + DestinationIsland;
            //_mark.transform.tag = "Runtime_Mark";

            if (boardgameObjects.ContainsKey("Buildings"))
            {
                _mark.transform.SetParent(boardgameObjects["Buildings"].transform);
            }
        }
        RADIUS_SPACE = 30;
    }

    void PlaySound(string _sound)
    {
        SC_MenuLogic.Instance.PlaySound(_sound);
    }

    private int CashLogic(SC_UniPlayer uni_player)
    {
        e.Player player = uni_player.player;
        int player_value = uni_player.player_balnce;
        player_value += curBoard.OwnerValue(player);
        return player_value;
    }

    void DestroyMarks(string destroyTag)
    {
        GameObject[] destroyObject;
        destroyObject = GameObject.FindGameObjectsWithTag(destroyTag);
        foreach (GameObject g in destroyObject)
            Destroy(g);
    }
    private void CheckWinner()
    {
        int player1Cash = CashLogic(Player1);
        int player2Cash = CashLogic(Player2);
        SC_UniPlayer winner = null;
        Debug.Log("CheckWinner:");
        Debug.Log(Player1.player_name + " value is " + CashLogic(Player1).ToString("n0"));
        Debug.Log(Player2.player_name + " value is " + CashLogic(Player2).ToString("n0"));

        if (player1Cash >= MAX_CASH_WINNER)
            winner = Player1;
        else if (player2Cash >= MAX_CASH_WINNER)
            winner = Player2;

        if (winner != null)
        {
            HidePopup("OK");
            PlaySound("Winner");
            WarpClient.GetInstance().stopGame();
            Debug.Log(winner.player_name + " Winner!!");
            string title = "The Winner is";
            string text = $"The winner is {winner.player_name}!!\nWith a value of { CashLogic(winner).ToString("n0")} U\nDo you want to Restart game?";
            Gameover = true;
            ShowPopup(title, text, "YesNo");
        }
        else
            Debug.Log("No Winner");
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame() called");

        Gameover = false;  /*It is very important to reset it first
                           Because the next function depends on it*/
        HidePopup("YesNo");

        // Init Globals vars
        cube_random_number = 0;
        CurrrentButton = e.Buttons.none;
        markCounter = 0;
        LockNameInput = false;

        I_wantToRestart = false;
        OpponentWantRestart = false;

        //Destory Runtime marks
        DestroyMarks("Runtime_Mark");

        //init tools place
        Transform player_pos = boardgameObjects["Player1_Tool"].GetComponent<Transform>();
        player_pos.position = new Vector3(720, 108, 0);
        player_pos = boardgameObjects["Player2_Tool"].GetComponent<Transform>();
        player_pos.position = new Vector3(709, 130, 0);

        //Set Active Popups object
        //Because in INIT() the Dictionary of the Game Obj was rebuilt
        boardgameObjects["PopupBoxOK"].SetActive(true);
        boardgameObjects["PopupBoxYesNo"].SetActive(true);
        boardgameObjects["PopupBoxNameInput"].SetActive(true);

        UnlockButton(Button_Roll_Obj);
        //Like Awake
        Init();
        InitPlayerUI();
        UpdateBoard(0);

        if (G.gameMode == e.GameMode.SinglePlayer)
            PopNameInput();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void GoToMainMenu()
    {
        G.GoToMainMenu = true;
    }
    private void HideTooltip()
    {
        boardgameObjects["Tooltip2"].SetActive(false);
    }

    #endregion
    #region Events
    private void OnClick(int _Index)
    {
        //Debug.Log("You click on Island number: " + _Index);
        //Debug.Log(curBoard.GetIslandName(_Index) + ": " + curBoard.GetIslandStatus(_Index));
    }
    #endregion

    #region Callbacks

    //Multiplayer Mode

    private void SendInfoChat(string key, object value)
    {
        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add(key, value);
        string _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().SendChat(_toJson);
    }

    private void SendInfoChat(Dictionary<string, object> _toSend)
    {
        string _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().SendChat(_toJson);
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        Gameover = false;
        nextTurn = _NextTurn;
        startTime = Time.time;

        //MultiPlayer Auxiliary variable
        I_wantToRestart = false;
        OpponentWantRestart = false;

        RestartGame();

        //No need to request names.
        HidePopup("NameInput");

        //_NextTurn is the Owner of the room, and the Player1
        CurrentPlayer = Player1;

        Debug.Log("The game have started.\n" + _NextTurn + " is starting the game.");

        if (G.userID == _NextTurn)
        {
            G.PlayerIdentity = e.Player.Player1;
            Player1.player_name = G.MyName;
            boardgameObjects["Player1_Name"].GetComponent<Text>().text = "Me"; //Player1.player_name;

            Player2.player_name = "Guest";
            boardgameObjects["Player2_Name"].GetComponent<Text>().text = Player2.player_name;
        }
        else
        {
            G.PlayerIdentity = e.Player.Player2;
            Player2.player_name = G.MyName;
            boardgameObjects["Player2_Name"].GetComponent<Text>().text = "Me"; //Player2.player_name;

            Player1.player_name = G.GusetName;
            boardgameObjects["Player1_Name"].GetComponent<Text>().text = Player1.player_name;
            //Lock Roll button for Player2
            LockButton(Button_Roll_Obj);
        }
        //Send chat with my name to the Opponent
        SendInfoChat("Name", G.MyName);
        /***/
        StartGame();
    }

    private void OnUserLeftRoom(RoomData eventObj, string _UserName)
    {
        Debug.Log(_UserName + " left is loser");
        WarpClient.GetInstance().stopGame();
        GoToMainMenu();
    }

    private void OnMoveCompleted(MoveEvent _Move)
    {
        Debug.Log("OnMoveCompleted: " + _Move.getSender() + " done move. Next turn is: " + _Move.getNextTurn());


        //Everyone is notified of the end of the move
        if (_Move.getMoveData() != null)
        {
            nextTurn = _Move.getNextTurn();
            //Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Move.getMoveData());
            //if (_data.ContainsKey("Move Done"))
            MultiplayerPassTurn(); //_Move.getSender()
        }

        //If the MoveEvent is null its mean the Time is over
        else if (_Move.getMoveData() == null)
        {
            MultiplayerPassTurn(); //_Move.getSender()
        }

        /*
         It seems that all the condition is unnecessary, because there is really NO data on the event
         
         Maybe MultiplayerPassTurn() to everybody
         */
    }

    private void OnChatReceived(string _Sender, string _Message)
    {
        if (G.userID != _Sender)
        {

            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Message);

            /*  Dictionary Key Picker  */

            //Get Opponent name
            if (_data.ContainsKey("Name"))
            {
                G.GusetName = _data["Name"].ToString();
                Debug.Log(_Sender + ": " + G.GusetName);
                if (G.PlayerIdentity == e.Player.Player1)
                {
                    Player2.player_name = G.GusetName;
                    boardgameObjects["Player2_Name"].GetComponent<Text>().text = G.GusetName;
                }
                else if (G.PlayerIdentity == e.Player.Player2)
                {
                    Player1.player_name = G.GusetName;
                    boardgameObjects["Player1_Name"].GetComponent<Text>().text = G.GusetName;
                    boardgameObjects["IslandDet"].GetComponent<Text>().text = G.GusetName + "\nEnjoy your trip!";

                }
            }

            //Get Opponent cube roll
            else if (_data.ContainsKey("CubeRoll"))
            {
                //Get from the other player Roll Cube
                guestCube = int.Parse(_data["CubeRoll"].ToString());
                Debug.Log(G.GusetName + ": Roll " + guestCube);
                Button_Roll();
            }

            //Get if Opponent Buy Island
            else if (_data.ContainsKey("Buy_Island_Logic"))
            {

                //Get update the Opponent info
                int Opponent_index = int.Parse(_data["Buy_Island_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Buy_Island_Logic();
                }
            }
            //Get if Opponent Buy Company
            else if (_data.ContainsKey("Buy_Company_Logic"))
            {
                //Get update the Opponent info
                int Opponent_index = int.Parse(_data["Buy_Company_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Buy_Company_Logic();
                }
            }
            //Get if Opponent go to Person
            else if (_data.ContainsKey("Prison_Logic"))
            {

                //Get update the Opponent info
                int Opponent_index = int.Parse(_data["Prison_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Prison_Logic();
                }
            }
            //Get if Opponent go to Start
            else if (_data.ContainsKey("Start_Logic"))
            {
                //Get Opponent info
                int Opponent_index = int.Parse(_data["Start_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Start_Logic();
                }
            }
            //Get if Opponent get Dividends
            else if (_data.ContainsKey("Dividends_Logic"))
            {
                //Get Opponent info
                int Opponent_index = int.Parse(_data["Dividends_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Dividends_Logic();
                }
            }
            //Get if Opponent get Pay Rent
            else if (_data.ContainsKey("Pay_Rent_Logic"))
            {
                //Get Opponent info
                int Opponent_index = int.Parse(_data["Pay_Rent_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Pay_Rent_Logic();
                }
            }
            //Get if Opponent get Dividends
            else if (_data.ContainsKey("Build_Logic"))
            {
                //Get Opponent info
                int Opponent_index = int.Parse(_data["Build_Logic"].ToString());
                //Make sure if the Opponent on the right island
                if (Opponent_index == CurrentPlayer.GetReallyIsland())
                {
                    Debug.Log(G.MyName + " activate the function the opponent requested.");
                    Build_Logic();
                }
            }
            //Get if Opponent want To Restart
            else if (_data.ContainsKey("Restart"))
            {
                if (_data["Restart"].ToString() == "Yes")
                {
                    Debug.Log("Opponent want to Restart");
                    OpponentWantRestart = true;
                    //Update() will check
                }
                else if (_data["Restart"].ToString() == "No")
                {
                    OpponentWantRestart = false;
                    Debug.Log("Opponent DONT want to Restart");
                    WarpClient.GetInstance().stopGame();
                    GoToMainMenu();
                }
            }
        }
    }
    #endregion
}