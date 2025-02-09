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
    private const string LOCALHOST = "127.0.0.1";
    public static Connector instance;

    [SerializeField] int maxConnections = 10;
    [SerializeField] UnityTransport tporter;
    public NetworkManager netMan;
    private string serverIPcode;
    private bool running;

    public bool Online => Application.internetReachability != NetworkReachability.NotReachable;

    /// <summary>
    /// Ziska si lokanu ip adresu pocitaca
    /// </summary>
    /// <returns>IP_ADRESA</returns>
    private string ServerIP => IPManager.GetIP();

    void Awake()
    {
        instance = this;
        running = false;
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


    public string GetConnection()
    {
        string conn = "";
        
        if (running)
            if (serverIPcode != LOCALHOST)
            {
                conn += netMan.IsServer ? "server-" : "client-";
                conn += serverIPcode;
            }
            else
            {
                conn += "solo-";
                conn += FileManager.World.worldName;
            }

        return conn;
    }

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

        /*if (netMan.StartClient())
            codeText.text = joinCode;*/
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

        //codeText.text = ServerIP;
    }
    /// <summary>
    /// Pripoji sa na server v lokalnej sieti
    /// </summary>
    /// <param name="serverIP"></param>
    void JoinLAN(string serverIP)
    {
        tporter.SetConnectionData(ServerIP, 7777);
        /*if (netMan.StartClient())
            codeText.text = ServerIP;*/
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
    /// Vytvori hru pre jendneho hraca.
    /// Tym ze sa vytvori server na loopback adrese.
    /// </summary>
    public void CreateSolo()
    {
        tporter.SetConnectionData("127.0.0.1", 7777);
        netMan.StartHost();
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
}