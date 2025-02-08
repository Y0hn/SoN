using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.IO;

/// <summary>
/// Sluzi na zhrnutie parametrov potrebnych pre pripojenie
/// </summary>
public class Connector : MonoBehaviour
{
    public static Connector instance;

    [SerializeField] Vector2 spawnRange = new(5,5);
    [SerializeField] int maxConnections = 10;
    [SerializeField] UnityTransport tporter;
    public NetworkManager netMan;
    public TMP_Text codeText;

    public Transform SpawnPoint { get; set;}

    /// <summary>
    /// Ziska si lokanu ip adresu pocitaca
    /// </summary>
    /// <returns>IP_ADRESA</returns>
    private string ServerIP => IPManager.GetIP();
    public Vector2 PlayerRandomSpawn => 
        new(MapScript.map.PlaySpawn.position.x + Random.Range(-spawnRange.x, spawnRange.x), 
            MapScript.map.PlaySpawn.position.y + Random.Range(-spawnRange.y, spawnRange.y));

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("More than one instance of Connection Manager");
        //FileManager.WorldAct(""/*, .Create*/);
    }
    async void Start()
    {
        // RELAY
        // Potrebne pripojenie na Internet
        netMan = NetworkManager.Singleton;

        netMan.OnServerStopped += delegate { /*FileManager.WorldAct("", FileManager.WorldAction.Save); */};

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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

        tporter.SetRelayServerData(relayServerData);
        
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
        tporter.SetConnectionData(ServerIP, 7777);

        if (host)
            netMan.StartHost(); 
        else
            netMan.StartServer();

        codeText.text = ServerIP;
    }
    /// <summary>
    /// Pripoji sa na server v lokalnej sieti
    /// </summary>
    /// <param name="serverIP"></param>
    void JoinLAN(string serverIP)
    {
        tporter.SetConnectionData(ServerIP, 7777);
        if (netMan.StartClient())
            codeText.text = ServerIP;
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
            else
            {
                errorCode = "code";
                JoinRelay(connection);
            }
        } catch {
            errorCode += " is invalid";
            join = false;
        }

        return join;
    }
    /// <summary>
    /// Vytvori hru pre jendneho hraca 
    /// Tym ze sa vytvori server na loopback adrese
    /// </summary>
    public void CreateSolo(bool loadLast = false)
    {
        tporter.SetConnectionData("127.0.0.1", 7777);
        netMan.StartHost();
        StartWorld(loadLast);
    }
    /// <summary>
    /// Nacita udaje z pamate pri nacitani sveta zo suboru
    /// </summary>
    /// <param name="load"></param>
    /// <param name="host"></param>
    private void StartWorld(bool load = false)
    {
        /*if      (host && load)
            FileManager.WorldAct("", FileManager.WorldAction.Load);
        else if (host && !load)
            FileManager.WorldAct("", FileManager.WorldAction.Create);*/
        // nacitat svet
        if (load)
        {

        }
        else
        {
            // vytvori novy svet
            FileManager.StartWorld();
        }
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
    }
    /// <summary>
    /// Je volana z hraca, ktory sa chce ozivit. <br />
    /// Nastavi jeho poziciu na vychodiskovu poziciu a ulozi jeho data
    /// </summary>
    /// <param name="id"></param>
    public void RespawnPlayer(ulong id)
    {
        Transform t = netMan.ConnectedClients[id].PlayerObject.transform;
        t.position = PlayerRandomSpawn;
        FileManager.World.SaveRewritePlayer(new (t.GetComponent<PlayerStats>()));
        FileManager.Log($"Player respawned {t.name} ", FileLogType.RECORD);
    }
}