using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

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
    private string hostingAddress;
    private bool running;

    public bool Solo => hostingAddress == LOCALHOST;
    public bool LanConnection => hostingAddress.Contains(".");
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

        if (Online)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /// <summary>
    /// Vrati aktuale pripojenie
    /// </summary>
    /// <returns></returns>
    public string GetConnection()
    {
        string conn = "";
        
        if (running)
            if (Solo)
            {
                conn += "solo-";
                conn += FileManager.World.worldName;
            }
            else
            {
                conn += netMan.IsServer ? "server-" : "client-";
                conn += hostingAddress;
            }

        return conn;
    }

    /// <summary>
    /// Zapne server na lokalnej sieti alebo na online prepojeni
    /// </summary>
    /// <param name="online"></param>
    /// <returns></returns>
    public async Task StartServer(bool online)
    {
        if (online)
            await CreateRelay();
        else
            CreateLAN(ServerIP);
    }
    /// <summary>
    /// Vytvori server pre vzdialene pripojenie  
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    async Task CreateRelay()
    {
        if (!Online) return;

        // Ziska data potrebne pre prepojenie s prepinacom
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

        // Ziska prihlasovaci kod pre prapinac
        hostingAddress = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Vytovri parametre pre prenos
        var relayServerData = new RelayServerData(allocation, "dtls");

        // Nastavi vytvorene parametre
        tporter.SetRelayServerData(relayServerData);

        // Zapne server
        netMan.StartHost();
    }
    /// <summary>
    /// Zapne lokalny server
    /// </summary>
    /// <param name="host"></param>
    void CreateLAN(string ip_address)
    {
        tporter.SetConnectionData(ip_address, 7777);
        hostingAddress = ip_address;
        netMan.StartHost();
    }
    /// <summary>
    /// Pokusi sa pripojit na server pomocou adresy alebo kodu
    /// </summary>
    /// <param name="connection">ADRESA alebo KOD pripojenia</param>
    /// <returns>PRAVDA ak sa pripojenie podarilo</returns>
    public async Task<bool> JoinConnection(string connection)
    {
        bool joined = false;

        try {
            if (LanConnection)  // nastaveine parametrov pre pripojenie na LOKALNEJ sieti
            {
                tporter.SetConnectionData(ServerIP, 7777);
            }            
            else                // nastaveine parametrov pre pripojenia prepinacim KODOM
            {
                // Ziska udaje z prepinaca o pripojeni podla zadanehu kodu
                var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(connection);

                // Vytvori parametere pre prenos
                var relayServerData = new RelayServerData(JoinAllocation, "dtls");

                // Nastavi vytvorene prarametre
                tporter.SetRelayServerData(relayServerData);
            }
            // pokusi sa o nadvizanie spojenia
            netMan.StartClient();
        } catch  {
            FileManager.Log($"Join connection failed with ipcode= {connection}", FileLogType.ERROR);
            joined = false;
        }

        return joined;
    }

    /// <summary>
    /// Vytvori hru pre jendneho hraca.
    /// Tym ze sa vytvori server na loopback adrese.
    /// </summary>
    public void CreateSolo()
    {
        CreateLAN(LOCALHOST);
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