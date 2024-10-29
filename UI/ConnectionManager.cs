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
    async void CreateRelay(string role = "host")
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = joinCode;

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
        if (role == "host")
        {
            NetworkManager.Singleton.StartHost();
        }
        else
            NetworkManager.Singleton.StartServer();
    }
    async void JoinRelay(string joinCode)
    {
        if (joinCode != "")
        {
            try 
            {
                var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = new RelayServerData(JoinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            }
            catch
            {
                //relayJoinInput.text = "Incorrect Code";
                return;
            }
            if (NetworkManager.Singleton.StartClient())
                codeText.text = joinCode;
        }
    }
    /*
            __     ___     _   __
           / /    /   |   / | / /
          / /    / /| |  /  |/ / 
         / /___ / ___ | / /|  /  
        /_____//_/  |_|/_/ |_/   
    */
    void CreateLAN(string role)
    {
        //string serverIP = IPManager.GetIP(IPManager.AddressForm.IPv4);
        string serverIP = "127.0.0.1";
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        switch (role)
        {
            case "server": 
                NetworkManager.Singleton.StartServer(); 
                break;
            case "host":  
                NetworkManager.Singleton.StartHost(); 
                break;
        }
        codeText.text = serverIP;
    }
    void JoinLAN(string serverIP)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        if (NetworkManager.Singleton.StartClient())
            codeText.text = serverIP;
    }
}