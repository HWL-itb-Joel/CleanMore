using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Steamworks;

public class CleanMoreNetworkManager : NetworkManager
{
    [SerializeField] private GameObject EnterAddressPanel, landingPage;
    [SerializeField] private TMP_InputField AddressField;

    public List<FirstPlayerController> PlayerList = new List<FirstPlayerController>();

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        if (!SteamManager.Initialized) return;
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        FirstPlayerController playerPref = conn.identity.GetComponent<FirstPlayerController>();

        PlayerList.Add(playerPref);

        if (PlayerList.Count >= 2)
        {
            //Aquí tendrá que ir para poder iniciar una partida
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        FirstPlayerController playerPref = conn.identity.GetComponent<FirstPlayerController>();

        PlayerList.Remove(playerPref);
    }

    public void HostLobby()
    {
        if (SteamManager.Initialized)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
            return;
        }

        NetworkManager.singleton.StartHost();
    }

    public void JoinButton()
    {
        EnterAddressPanel.SetActive(true);
        landingPage.SetActive(false);
    }

    public void JoinLobby()
    {
        NetworkManager.singleton.networkAddress = AddressField.text;
        NetworkManager.singleton.StartClient();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        landingPage.SetActive(true);
        EnterAddressPanel.SetActive(false);
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    #region Callbacks
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostIP", SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string HostIP = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostIP");

        NetworkManager.singleton.networkAddress = HostIP;
        NetworkManager.singleton.StartClient();
    }
    #endregion
}
