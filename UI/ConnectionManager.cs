using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.VisualScripting;
public class ConnectionManager : MonoBehaviour
{
    [SerializeField] GameObject mainCam;
    #region UI_Buttons
    [SerializeField] Button relayServBtn;
    [SerializeField] Button relayHostBtn;
    [SerializeField] Button relayJoinBtn;
    [SerializeField] Button relayUISwitch;
    [SerializeField] TMP_InputField relayJoinInput;
    [SerializeField] Button lanServBtn;
    [SerializeField] Button lanHostBtn;
    [SerializeField] Button lanJoinBtn;
    [SerializeField] Button lanUISwitch;
    [SerializeField] TMP_InputField lanJoinInput;
    #endregion
    [SerializeField] RectTransform ConnectionPanel;
    [SerializeField] TMP_Text codeText;

    private string currentUI = "";
    private RectTransform relayUI;
    private RectTransform lanUI;
    private GameObject UIparent;

    async void Start()
    {
        Debug.Log("Connection Manager Starting");
        // UI swich
        UIparent = ConnectionPanel.parent.gameObject;
        relayUI = ConnectionPanel.GetChild(0).GetComponent<RectTransform>();
        lanUI = ConnectionPanel.GetChild(1).GetComponent<RectTransform>();
        relayUISwitch.onClick.AddListener(() => ChangeUI("relay"));
        lanUISwitch.onClick.AddListener(() => ChangeUI("lan"));

        // LAN
        lanHostBtn.onClick.AddListener(() => LanConnector("host"));
        lanServBtn.onClick.AddListener(() => LanConnector("server"));
        lanJoinBtn.onClick.AddListener(() => LanConnector("client"));

        ChangeUI("relay");

        // RELAY
        // Internet connection required
        //try
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        relayHostBtn.onClick.AddListener(() => CreateRelay());
        relayServBtn.onClick.AddListener(() => CreateRelay("server"));
        relayJoinBtn.onClick.AddListener(() => JoinRelay(relayJoinInput.text));
        /*catch
        {
            Debug.LogWarning("Cannot connect to Unity Services !");
            relayHostBtn.onClick.AddListener(() => Debug.Log("NO INTERNET"));
            relayJoinBtn.onClick.AddListener(() => Debug.Log("NO INTERNET"));
        }*/

        Debug.Log("Connection Manager Started");
    }
    async void CreateRelay(string role = "host")
    {
        Debug.Log("Using Create Relay");

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = "Code: " + joinCode;

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
        if (role == "host")
        {
            NetworkManager.Singleton.StartHost();
            mainCam.SetActive(false);
        }
        else
            NetworkManager.Singleton.StartServer();
        UIparent.SetActive(false);
    }
    async void JoinRelay(string joinCode)
    {
        Debug.Log("Using Join Relay");

        if (joinCode != "")
        {            
            var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(JoinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            codeText.text = "Code: " + joinCode;
            UIparent.SetActive(false);
            mainCam.SetActive(false);
        }
    }
    void LanConnector(string role)
    {
        Debug.Log("Using LAN connector");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
        switch (role)
        {
            case "server": NetworkManager.Singleton.StartServer(); break;
            case "host":   NetworkManager.Singleton.StartHost();   mainCam.SetActive(false); break;
            case "client": NetworkManager.Singleton.StartClient(); mainCam.SetActive(false); break;
        }
        string ip_add = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
        codeText.text = "Server IP: " + ip_add;
        UIparent.SetActive(false);
    }
    void ChangeUI(string changeTo)
    {
        switch (changeTo)
        {
            case "relay":
                relayUI.gameObject.SetActive(true);
                lanUI.gameObject.SetActive(false);
                relayUISwitch.interactable = false;
                lanUISwitch.interactable = true; 
                relayUISwitch.transform.SetSiblingIndex(2);
                lanUISwitch.transform.SetSiblingIndex(0);
                break;
            case "lan":
                relayUI.gameObject.SetActive(false);
                lanUI.gameObject.SetActive(true);
                relayUISwitch.interactable = true;
                lanUISwitch.interactable = false;
                relayUISwitch.transform.SetSiblingIndex(0);
                lanUISwitch.transform.SetSiblingIndex(2);
                break;
        }
        //Debug.Log(currentUI + " => " + changeTo);
        currentUI = changeTo;
    }
}