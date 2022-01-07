using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Networking.Transport;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Login")]
    [SerializeField] private TMP_InputField loginUserNameInputField;
    [SerializeField] private TMP_InputField loginPasswordInputField;
    [SerializeField] private TMP_InputField registerUserNameInputField;
    [SerializeField] private TMP_InputField registerPasswordInputField;

    [Header("InfoPanel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI accountNameText;
    [SerializeField] private TextMeshProUGUI accountWinText;
    [SerializeField] private TextMeshProUGUI accountTotalGamesText;

    [Header("Highscore")]
    [SerializeField] private TextMeshProUGUI highScoreText; 

    [Header("Menu")]
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject endResultPanel;
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField portNumberTextfield;

    [Header("Game")]
    [SerializeField] private Card[] cards;
    [SerializeField] private GameObject opponentsReadyImg;
    [SerializeField] private Card selectedCard;
    [SerializeField] private TextMeshProUGUI winLoseRoundText;
    [SerializeField] private Button playCardButton;

    [Header("End results")]
    [SerializeField] private TextMeshProUGUI winLoseGameText;

    private ServerBehaviour serverBehaviour;
    private ClientBehaviour clientBehaviour;

    public void Login()
    {
        Account.Instance.Login(loginUserNameInputField.text, loginPasswordInputField.text);
    }

    public void Logout()
    {
        Account.Instance.Logout();
    }
    
    public void Register()
    {
        Account.Instance.Register(registerUserNameInputField.text, registerPasswordInputField.text);
    }

    public void HighScoreUpdate()
    {
        Account.Instance.UpdateHighscore();
    }

    public void CreateHost()
    {
        ushort number = 9000;
        bool var = ushort.TryParse(portNumberTextfield.text, out number);
        if (!var) { number = 9000; }
        serverBehaviour = gameObject.AddComponent<ServerBehaviour>();
        serverBehaviour.CreateServer(number);
        CreateClient();
    }

    public void CreateClient()
    {
        if (GetComponent<ClientBehaviour>())
        {
            return;
        }

        ushort number = 9000;
        bool var = ushort.TryParse(portNumberTextfield.text, out number);
        if (!var) { number = 9000; }
        clientBehaviour = gameObject.AddComponent<ClientBehaviour>();
        clientBehaviour.TryToConnect(number); 
        clientBehaviour.onConnectedEvent.AddListener(AddClientConnected);
        clientBehaviour.playersUpdatedEvent.AddListener(UpdatePlayerList);
        clientBehaviour.onHostEvent.AddListener(HostSettingsEnabled);
        clientBehaviour.startGameEvent.AddListener(StartGame);
        clientBehaviour.playerQuitGameEvent.AddListener(PlayerQuitGame);
        clientBehaviour.opponentReadyEvent.AddListener(OpponentsReady);
        clientBehaviour.startBattleEvent.AddListener(StartBattle);
        clientBehaviour.startNewRoundEvent.AddListener(StartNewRound);
        clientBehaviour.gameHasEndedEvent.AddListener(TheGameHasEnded);
        clientBehaviour.restartLobbyEvent.AddListener(RestartLobby);
    }

    public void StartGameButton()
    {
        clientBehaviour.StartGame();
    }

    public void QuitLobby()
    {
        clientBehaviour.QuitGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayCard()
    {
        if(selectedCard.cardRef == null) { return; }
        clientBehaviour.PlayCard(selectedCard);
        selectedCard.cardRef.gameObject.SetActive(false);
        DisableCardInput(true);
    }

    private void OnLoggedIn()
    {
        Account.Instance.UpdateStats();
    }

    private void HostSettingsEnabled(bool correct)
    {
        startButton.interactable = correct;
    }

    private void OnStatsUpdated()
    {
        infoPanel.SetActive(true);
        accountNameText.text = Account.Instance.Username;
        accountWinText.text = $"Wins: {Account.Instance.Wins}";
        accountTotalGamesText.text = $"Total Games: {Account.Instance.TotalGames}";
    }

    private void OnHighscoresUpdated()
    {
        highScoreText.text = Account.Instance.HighscoreList;
    }

    private void DisableCardInput(bool correct)
    {
        playCardButton.interactable = !correct;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].GetComponent<Button>() != null)
            {
                cards[i].GetComponent<Button>().interactable = !correct;
            }
        }
    }

    private void StartGame()
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);
        AddCards();
    }

    private void PlayerQuitGame()
    {
        lobbyPanel.SetActive(false);
        joinPanel.SetActive(true);
        startButton.interactable = false;
    }

    private void AddClientConnected()
    {
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    private void UpdatePlayerList()
    {
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "---";
        }

        Debug.Log(clientBehaviour.players.Count);
        for (int i = 0; i < clientBehaviour.players.Count; i++)
        {
            playerNameTexts[i].text = clientBehaviour.players[i].name;
        }
    }

    private void AddCards()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].GetRandomCardType();
        }
    }

    private void AddCard()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (!cards[i].gameObject.activeInHierarchy)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].GetRandomCardType();
                break;
            }
        }
    }

    private void OpponentsReady()
    {
        opponentsReadyImg.SetActive(true);
    }

    private void StartBattle(Endstate endState)
    {
        if (endState == Endstate.Win)
        {
            winLoseRoundText.text = "You won this round!";
        }
        else if(endState == Endstate.Lose)
        {
            winLoseRoundText.text = "You lost this round";
        }
        else
        {
            AddCard();
            winLoseRoundText.text = "Its a draw!";
        }
    }

    private void StartNewRound()
    {
        winLoseRoundText.text = "";
        selectedCard.ResetSelectedCard();
        DisableCardInput(false);
        opponentsReadyImg.SetActive(false);
    }

    private void TheGameHasEnded(bool hasWon)
    {
        gamePanel.SetActive(false);
        endResultPanel.SetActive(true);

        if (hasWon)
        {
            winLoseGameText.text = "You won this game!";
        }
        else
        {
            winLoseGameText.text = "You lost this game";
        }
    }

    private void RestartLobby()
    {
        endResultPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        Account.Instance.UpdateStats();


        StartNewRound();
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        Account.Instance.LoggedIn.AddListener(OnLoggedIn);
        Account.Instance.StatsUpdated.AddListener(OnStatsUpdated);
        Account.Instance.HighscoresUpdated.AddListener(OnHighscoresUpdated);
    }

}