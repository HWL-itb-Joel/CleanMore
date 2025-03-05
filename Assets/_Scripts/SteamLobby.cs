using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;
    //Callbacks - Llamadas de Steam
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    //Variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    //GameObjects
    public GameObject HostButton;
    public TextMeshProUGUI LobbyNameText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        manager = GetComponent<CustomNetworkManager>();

        if (!SteamManager.Initialized) { return; }

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        //Aqui se crea el host, con el tipo de privacidad (solo amigos), y cogemos del manager el numero maximo de conexiones.
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        //Esta linea de codigo hace que finalice la funcion si la lobby no se ha creado correctamente, si no continuara
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("Lobby Created Succesfully");

        //Aqui hacemos que empiece el  host.
        manager.StartHost();

        //Esta linea de codigo Setea la direccion del servidor y hace que sea la misma que el usuario de steam.
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        //Esta linea de codigo Setea el nombre del servidor, y hace que sea el nombre de Steam del Host de la partida.
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request To Join Lobby");
        //Esta funcion es muy sencilla, simplemente manda un request para unirse a la partida del host.
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        //Esta funcion la ejecutaran todos los jugadores, incluido el host, asi que haremos ejecutaremos secciones de la funcion para el host, otras para todos los demas.
        //Everyone - Todos
        //HostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        //LobbyNameText.gameObject.SetActive(true);
        //LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        //Clientes
        if (NetworkServer.active) { return; }

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        manager.StartClient();
    }
}
