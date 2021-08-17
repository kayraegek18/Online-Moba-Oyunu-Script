using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    [Header("Register")]
    public TMP_Text errorText;
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField passwordInput2;

    [Header("Login")]
    public TMP_Text errorText_Login;
    public TMP_InputField emailInput_Login;
    public TMP_InputField passwordInput_Login;
    public Toggle beniHatirla;

    private string userEmail;
    private string userPassword;
    private string username;

    public static string userID;
    public static string userName;

    [HideInInspector] public static bool getStatsFinished;
    public static string DebugMessage;
    public static Dictionary<string, int> playerData = new Dictionary<string, int>();

    public static int Coin = 0;

    public GetPlayerCombinedInfoRequestParams info;

    private void Awake()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "9DF13";
        }

        // Login to account if email is not null for login
        if (PlayerPrefs.HasKey("PLAYFAB_USER_EMAIL") && PlayerPrefs.GetInt("beniHatırla") == 1)
        {
            userEmail = PlayerPrefs.GetString("PLAYFAB_USER_EMAIL");
            userPassword = PlayerPrefs.GetString("PLAYFAB_USER_PASSWORD");
            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword, InfoRequestParameters = info };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginError);
        }
    }

	/// <summary>
	/// Login button void
	/// </summary>
	public void Login()
    {
        if (beniHatirla.isOn)
            PlayerPrefs.SetInt("beniHatırla", 1);
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword, InfoRequestParameters = info };
        request.InfoRequestParameters = info;
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginError);
    }


    /// <summary>
    /// Register button void
    /// </summary>
    public void Register()
    {
        if (!CheckPassword())
        {
            errorText.text = "Parolalarda sıkıntı var tekrar kontrol ediniz!";
            return;
        }

        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username, DisplayName = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterError);
    }

    // REGISTER SUCCESS

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        PlayerPrefs.SetString("PLAYFAB_USER_EMAIL", userEmail);
        PlayerPrefs.SetString("PLAYFAB_USER_PASSWORD", userPassword);
        PlayerPrefs.SetString("PLAYFAB_USER_USERNAME", username);
        DebugMessage = "You're now connected!";
        SetStat("coin", 5);
        SetStat("level", 1);
        GetAccountInfo();
        GetStats();
    }

    // LOGIN SUCCESS

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you're now connected!");
        PlayerPrefs.SetString("PLAYFAB_USER_EMAIL", userEmail);
        PlayerPrefs.SetString("PLAYFAB_USER_PASSWORD", userPassword);
        PlayerPrefs.SetString("PLAYFAB_USER_ID", result.PlayFabId);
        DebugMessage = "You're now connected!";
        GetAccountInfo();
        PlayerPrefs.SetInt("coin", Coin);
        GetStats();
        SceneManager.LoadScene("Menu");
    }

    // ERRORS

    void OnError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    void OnLoginError(PlayFabError error)
    {
        errorText_Login.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport());
    }
    void OnRegisterError(PlayFabError error)
    {
        errorText.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport());
    }

    // GET USER INFO

    public void GET_USER_EMAIL(string emailIn)
    {
        userEmail = emailIn;
    }
    public void GET_USER_PASSWORD(string passwordIn)
    {
        userPassword = passwordIn;
    }
    public void GET_USER_USERNAME(string usernameIn)
    {
        username = usernameIn;
    }

    // ACCOUNT INFO
    public static void GetAccountInfo()
    {
        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, Successs, error => { Debug.Log(error.GenerateErrorReport()); });
    }

    static void Successs(GetAccountInfoResult result)
    {
        userID = result.AccountInfo.PlayFabId;
        userName = result.AccountInfo.Username;

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("PLAYFAB_USER_USERNAME")))
            PlayerPrefs.SetString("PLAYFAB_USER_USERNAME", userName);

        Debug.Log("Username: " + PlayerPrefs.GetString("PLAYFAB_USER_USERNAME"));
    }

    // STATS

    /// <summary>
    /// Set user statistic
    /// </summary>
    /// <param name="statistcName"></param>
    /// <param name="statiscticValue"></param>
    public static void SetStat(string statistcName, int statiscticValue)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
        new StatisticUpdate { StatisticName = statistcName, Value = statiscticValue },
            }
        },
        result =>
        {
            // update or add this data to the dictionary
            try { playerData[statistcName] = statiscticValue; }
            catch { playerData.Add(statistcName, statiscticValue); }
        },
        error => { Debug.LogError(error.GenerateErrorReport()); DebugMessage = error.GenerateErrorReport(); });
    }

    /// <summary>
    /// Get user statistic
    /// </summary>
    public static void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => { Debug.LogError(error.GenerateErrorReport()); DebugMessage = error.GenerateErrorReport(); }
        );
    }

    static void OnGetStats(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        playerData.Clear();
        foreach (var eachStat in result.Statistics)
        {
            playerData.Add(eachStat.StatisticName, eachStat.Value);
        }
        foreach (var data in playerData)
        {
            Debug.Log(data.Key + ": " + data.Value);

            if (data.Key == "level")
                PlayerPrefs.SetInt("level", data.Value);

            if (data.Key == "admin")
                PlayerPrefs.SetInt("admin", data.Value);
        }
        if (PlayerPrefs.GetInt("coin") == 0)
            GetStats();

        getStatsFinished = true;
    }

    // UI INPUTS

    public bool CheckPassword()
	{
        if (passwordInput.text != passwordInput2.text)
            return false;
        else if (string.IsNullOrEmpty(passwordInput.text) || string.IsNullOrWhiteSpace(passwordInput.text))
            return false;
        else if (string.IsNullOrEmpty(passwordInput2.text) || string.IsNullOrWhiteSpace(passwordInput2.text))
            return false;

        return true;
    }

    public void SetEmailLogin()
    {
        userEmail = emailInput_Login.text;
    }

    public void SetPassLogin()
    {
        userPassword = passwordInput_Login.text;
    }

    public void SetEmailRegister()
    {
        userEmail = emailInput.text;
    }

    public void SetPassRegister()
    {
        userPassword = passwordInput.text;
    }

    public void SetUsername()
    {
        username = usernameInput.text;
    }
}

/*
* Copyright © Gameiva 2021 - 2022
*/
