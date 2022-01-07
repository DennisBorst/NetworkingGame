using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class Account : MonoBehaviour
{
    private static Account instance;

    private void Awake()
    {
        instance = this;
    }

    public static Account Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject();
                instance = obj.AddComponent<Account>();
            }
            return instance;
        }
    }

    private const string ACCOUNTLOGIN_URL = "https://studenthome.hku.nl/~dennis.borst/DataBase/Login.php";
    private const string ACCOUNTREGISTER_URL = "https://studenthome.hku.nl/~dennis.borst/DataBase/Register.php";
    private const string GETSTATS_URL = "https://studenthome.hku.nl/~dennis.borst/DataBase/GetStats.php";
    private const string GETHIGHSCORES_URL = "https://studenthome.hku.nl/~dennis.borst/DataBase/GetHighscores.php";

    public UnityEvent LoggedIn = new UnityEvent();
    public UnityEvent LoggedOut = new UnityEvent();
    public UnityEvent LoginError = new UnityEvent();

    public UnityEvent RegistrationSuccess = new UnityEvent();
    public UnityEvent RegistrationFailed = new UnityEvent();

    public UnityEvent StatsUpdated = new UnityEvent();
    public UnityEvent HighscoresUpdated = new UnityEvent();

    public bool IsLoggedIn = false;
    public int Id = -1;
    public string Username = string.Empty;
    public int Wins = 0;
    public int TotalGames = 0;
    public string HighscoreList = "";

    public void Login(string username, string password)
    {
        if (!IsValidLogin(username, password)) { return; }
        StartCoroutine(LoginRequest(username, password));
    }

    public void Logout()
    {
        IsLoggedIn = false;
        Username = string.Empty;
        LoggedOut.Invoke();
    }

    public void Register(string username, string password)
    {
        if (!IsValidRegistration(username, password)) { return; }
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        StartCoroutine(WebRequest(
            ACCOUNTREGISTER_URL,
            RegistrationSuccess.Invoke,
            RegistrationFailed.Invoke,
            form));
    }

    public void UpdateStats()
    {
        StartCoroutine(UpdateStatsRoutine());
    }

    public void UpdateHighscore()
    {
        StartCoroutine(UpdateHighscoreRoutine());
    }

    [RuntimeInitializeOnLoadMethod]
    private void Initialize()
    {
        Application.quitting += OnApplicationQuit;
    }

    private bool IsValidLogin(string username, string password)
    {
        if (username.Length <= 0)
        {
            LoginError.Invoke();
            return false;
        }
        else if (password.Length <= 0)
        {
            LoginError.Invoke();
            return false;
        }

        return true;
    }

    private bool IsValidRegistration(string username, string password)
    {
        if (username.Length < 4)
        {
            RegistrationFailed.Invoke();
            return false;
        }
        else if (username.Length > 24)
        {
            RegistrationFailed.Invoke();
            return false;
        }
        else if (password.Length < 6)
        {
            RegistrationFailed.Invoke();
            return false;
        }
        else if (password.Length > 40)
        {
            RegistrationFailed.Invoke();
            return false;
        }

        return true;
    }

    private void OnApplicationQuit()
    {
        if (IsLoggedIn)
        {
            Logout();
        }
    }

    private IEnumerator UpdateHighscoreRoutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", Id);

        UnityWebRequest httpRequest = UnityWebRequest.Post(GETHIGHSCORES_URL, form);
        httpRequest.timeout = 15;
        yield return httpRequest.SendWebRequest();

        if (httpRequest.result == UnityWebRequest.Result.Success)
        {
            string highscoreInfo = httpRequest.downloadHandler.text;
            highscoreInfo = highscoreInfo.Remove(highscoreInfo.Length - 1);
            Debug.Log(highscoreInfo);
            string[] playerHighscoreInfo = highscoreInfo.Split('-');
            HighscoreList = "";

            for (int i = 0; i < playerHighscoreInfo.Length; i++)
            {

                string[] playerInfo = playerHighscoreInfo[i].Split('_');
                string playerName = playerInfo[0];

                if (!int.TryParse(playerInfo[1], out int wins))
                {
                    wins = -1;
                }

                HighscoreList += $"{i + 1}. {playerName}  -  {wins}\n";
            }

            HighscoresUpdated.Invoke();
        }
        else
        {
            Debug.Log(httpRequest.result);
            Debug.Log($"Could not retrieve highscores");
        }
    }

    private IEnumerator UpdateStatsRoutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", Id);

        UnityWebRequest httpRequest = UnityWebRequest.Post(GETSTATS_URL, form);
        httpRequest.timeout = 15;
        yield return httpRequest.SendWebRequest();

        if (httpRequest.result == UnityWebRequest.Result.Success)
        {
            string statsInfo = httpRequest.downloadHandler.text;
            string[] stats = statsInfo.Split('_');

            if (!int.TryParse(stats[0], out int totalGames))
            {
                totalGames = 0;
            }

            if (!int.TryParse(stats[1], out int wins))
            {
                wins = 0;
            }

            Debug.Log($"{totalGames}, {wins}");
            TotalGames = totalGames;
            Wins = wins;

            StatsUpdated.Invoke();
        }
        else
        {
            Debug.Log($"Could not retrieve account info");
        }
    }

    private IEnumerator LoginRequest(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest httpRequest = UnityWebRequest.Post(ACCOUNTLOGIN_URL, form);
        httpRequest.timeout = 15;
        yield return httpRequest.SendWebRequest();
        if (httpRequest.result == UnityWebRequest.Result.Success)
        {
            string accountId = httpRequest.downloadHandler.text;
            Debug.Log(accountId);
            Id = int.Parse(accountId);

            IsLoggedIn = true;
            Username = username;
            Debug.Log($"Logged in! with {Username}, {Id}");
            LoggedIn.Invoke();
        }
        else
        {
            LoginError.Invoke();
        }
    }

    private IEnumerator WebRequest(string url, Action onSuccess = null, Action onFail = null, WWWForm form = null)
    {
        UnityWebRequest httpRequest = UnityWebRequest.Post(url, form);
        httpRequest.timeout = 15;
        yield return httpRequest.SendWebRequest();
        if (httpRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Succesfull");
            onSuccess?.Invoke();
        }
        else
        {
            onFail?.Invoke();
        }
    }

}