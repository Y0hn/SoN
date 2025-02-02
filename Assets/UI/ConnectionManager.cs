using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

/// <summary>
/// Sluzi na zhrnutie parametrov potrebnych pre pripojenie
/// </summary>
public class Connector : MonoBehaviour
{
    public static Connector instance;
    public Transform spawnPoint;
    [SerializeField] Vector2 spawnRange = new(5,5);
    public TMP_Text codeText;
    [SerializeField] int maxConnections = 10;
    public NetworkManager netMan;
    /// <summary>
    /// Ziska si lokanu ip adresu pocitaca
    /// </summary>
    /// <returns>IP_ADRESA</returns>
    private string serverIP { get { return IPManager.GetIP(IPManager.AddressForm.IPv4); } }

    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("More than one instance of Connection Manager");
    }
    async void Start()
    {
        // RELAY
        // Potrebne pripojenie na Internet
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        netMan = NetworkManager.Singleton;

        netMan.OnClientConnectedCallback += SpawnPlayer;
    }
    /*
           ____         __             
          / __ \ ___   / /____ _ __  __
         / /_/ // _ \ / // __ `// / / /
        / _, _//  __// // /_/ // /_/ / 
       /_/ |_| \___//_/ \__,_/ \__, /  
                              /____/   
    */
    /// <summary>
    /// Vytvori server pre vzdialene pripojenie  
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    async void CreateRelay(bool host = true)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var relayServerData = new RelayServerData(allocation, "dtls");

        netMan.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
        if (host)
            netMan.StartHost();
        else
            netMan.StartServer();

        codeText.text = joinCode;        
    }
    /// <summary>
    /// Pripoji sa n a vzdialeny server
    /// </summary>
    /// <param name="joinCode"></param>
    /// <returns></returns>
    async void JoinRelay(string joinCode)
    {
        var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = new RelayServerData(JoinAllocation, "dtls");
        netMan.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        if (netMan.StartClient())
            codeText.text = joinCode;
    }
    /*
            __     ___     _   __
           / /    /   |   / | / /
          / /    / /| |  /  |/ / 
         / /___ / ___ | / /|  /  
        /_____//_/  |_|/_/ |_/   
    */
    /// <summary>
    /// Zapne lokalny server
    /// </summary>
    /// <param name="host"></param>
    void CreateLAN(bool host = true)
    {
        netMan.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);

        if (host)
            netMan.StartHost(); 
        else
            netMan.StartServer();

        codeText.text = serverIP;
    }
    /// <summary>
    /// Pripoji sa na server v lokalnej sieti
    /// </summary>
    /// <param name="serverIP"></param>
    void JoinLAN(string serverIP)
    {
        netMan.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        if (netMan.StartClient())
            codeText.text = serverIP;
    }
    /// <summary>
    /// Zapne server na lokalnej sieti alebo na online prepojeni
    /// </summary>
    /// <param name="online"></param>
    /// <returns></returns>
    public bool StartConnection(bool online)
    {
        bool start = true;

        if (online)
            CreateRelay();
        else
            CreateLAN();
        
        if (start)
            FileManager.RegeneradeSettings();
        return start;
    }
    /// <summary>
    /// Pokusi sa pripojit na adresu alebo relay kod 
    /// </summary>
    /// <param name="connection">adresa alebo kod pripojejia</param>
    /// <param name="errorCode">chybovy kod</param>
    /// <returns></returns>
    public bool JoinConnection(string connection, out string errorCode)
    {
        bool join = true;
        errorCode = "expresion";

        try
        {
            // LAN CONNNECTION
            if (connection.Contains("."))
            {
                errorCode = "ip address";
                JoinLAN(connection);
            }
            // RELAY CONNECTION
            else // if (connection.Lenght == 6)
            {
                errorCode = "code";
                JoinRelay(connection);
            }/*
            else 
            {
                errorCode += " in worng format";
            }*/
        }
        catch
        {
            errorCode += " is invalid";
            join = false;
        }

        return join;
    }
    /// <summary>
    /// Vytvori hru pre jendneho hraca 
    /// Tym ze sa vytvori server na loopback adrese
    /// </summary>
    public void CreateSolo()
    {
        netMan.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
        netMan.StartHost(); 
    }
    /// <summary>
    /// Nacita udaje z pamate pri nacitani sveta zo suboru
    /// </summary>
    /// <param name="load"></param>
    /// <param name="host"></param>
    private void LoadWorld(bool load = false, bool host = true)
    {
        if (host && load)
            FileManager.WorldAct("", FileManager.WorldAction.Load/*get path to world save file*/);
        else if (host && !load)
             FileManager.WorldAct("", FileManager.WorldAction.Create);
        // nacitat svet
    }
    /// <summary>
    /// Na klientovy vypne pripojenie a na servere odpoji konkretneho klienta
    /// </summary>
    /// <param name="id">ID hraca</param>
    public void Quit(ulong id)
    {
        if (!netMan.IsServer)
            netMan.DisconnectClient(id);
        else
            netMan.Shutdown();
        //Environment.Exit(0);
    }

    /// <summary>
    /// Je volana na servery hned po pripojeni hraca do hry.
    /// Nastavuje jeho poziciu v ramci hranic stanovenych Vektorom "spawnRange"
    /// </summary>
    /// <param name="id">ID hraca</param>
    private void SpawnPlayer(ulong id)
    {
        Transform t = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform;
        Vector2 pos = spawnPoint.position;
        pos = new(
            pos.x + Random.Range(-spawnRange.x, spawnRange.x), 
            pos.y + Random.Range(-spawnRange.y, spawnRange.y));
        t.position = pos;
    }
}