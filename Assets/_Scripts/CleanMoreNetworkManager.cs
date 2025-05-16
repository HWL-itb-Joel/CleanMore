using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Steamworks;

public class CleanMoreNetworkManager : NetworkManager
{
    [SerializeField] private TMP_InputField AddressField;

    public List<GameObject> PlayerList { get; set; } = new List<GameObject>();
    public MultiplayerFPSMovement[] list;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public static readonly List<Transform> AllPlayers = new List<Transform>();

    public static void Register(Transform player)
    {
        if (!AllPlayers.Contains(player))
            AllPlayers.Add(player);
    }

    public static void Unregister(Transform player)
    {
        if (AllPlayers.Contains(player))
            AllPlayers.Remove(player);
    }


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

        GameObject playerPref = conn.identity.GetComponent<GameObject>();

        PlayerList.Add(playerPref);

        if (PlayerList.Count >= 2)
        {
            //Aquí tendrá que ir para poder iniciar una partida
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        GameObject playerPref = conn.identity.GetComponent<GameObject>();

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

    public void JoinLobby()
    {
        NetworkManager.singleton.networkAddress = AddressField.text;
        NetworkManager.singleton.StartClient();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            singleton.StopHost();
        }
        else
        {
            singleton.StopClient();
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

    public override void OnStopServer()
    {
        base.OnStopServer();
        CleanMoreNetworkManager.singleton.StopClient();
    }

    #region Callbacks
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        singleton.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostIP", SteamUser.GetSteamID().ToString());
        print(SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostIP"));
    }

    private void OnGameLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string HostIP = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostIP");

        singleton.networkAddress = HostIP;
        singleton.StartClient();
    }
    #endregion
}
