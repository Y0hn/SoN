using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager instance;
    [SerializeField] public TMP_Text codeText;
    [SerializeField] int maxConnections = 10;
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
        // Internet connection required
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
    async void CreateRelay(bool host = true)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
        if (host)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartServer();

        codeText.text = joinCode;        
    }
    async void JoinRelay(string joinCode)
    {
        var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = new RelayServerData(JoinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        if (NetworkManager.Singleton.StartClient())
            codeText.text = joinCode;
    }
    /*
            __     ___     _   __
           / /    /   |   / | / /
          / /    / /| |  /  |/ / 
         / /___ / ___ | / /|  /  
        /_____//_/  |_|/_/ |_/   
    */
    void CreateLAN(bool host = true)
    {
        //string serverIP = IPManager.GetIP(IPManager.AddressForm.IPv4);
        string serverIP = "127.0.0.1";
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);

        if (host)
            NetworkManager.Singleton.StartHost(); 
        else
            NetworkManager.Singleton.StartServer();

        codeText.text = serverIP;
    }
    void JoinLAN(string serverIP)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        if (NetworkManager.Singleton.StartClient())
            codeText.text = serverIP;
    }
    public bool StartConnection(bool online)
    {
        bool start = true;

        if (online)
            CreateRelay();
        else
            CreateLAN();

        return start;
    }
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
}