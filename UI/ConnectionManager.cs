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
using UnityEngine.Rendering;
using System.Threading.Tasks;
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
    [SerializeField] public TMP_Text codeText;
    [SerializeField] TMP_Text titleText;

    private string currentUI = "";
    private RectTransform relayUI;
    private RectTransform lanUI;
    private GameObject UIparent;

    async void Start()
    {
        // UI setup
        UIparent = ConnectionPanel.parent.gameObject;
        relayUI = ConnectionPanel.GetChild(0).GetComponent<RectTransform>();
        lanUI = ConnectionPanel.GetChild(1).GetComponent<RectTransform>();
        relayUISwitch.onClick.AddListener(() => ChangeUI("relay"));
        lanUISwitch.onClick.AddListener(() => ChangeUI("lan"));
        ChangeUI("relay");

        // LAN
        lanHostBtn.onClick.AddListener(() => CreateLAN("host"));
        lanServBtn.onClick.AddListener(() => CreateLAN("server"));
        lanJoinBtn.onClick.AddListener(() => JoinLAN(lanJoinInput.text));


        // RELAY
        // Internet connection required
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        relayHostBtn.onClick.AddListener(() => CreateRelay());
        relayServBtn.onClick.AddListener(() => CreateRelay("server"));
        relayJoinBtn.onClick.AddListener(() => JoinRelay(relayJoinInput.text));
    }
    async void CreateRelay(string role = "host")
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = joinCode;

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
                relayJoinInput.text = "Incorrect Code";
                return;
            }

            if (NetworkManager.Singleton.StartClient())
            {
                codeText.text = joinCode;
                UIparent.SetActive(false);
                mainCam.SetActive(false);
            }
            else
            {
                relayJoinInput.text = "Incorrect Code";
            }
        }
    }
    void CreateLAN(string role)
    {
        string serverIP = IPManager.GetIP(IPManager.AddressForm.IPv4);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        switch (role)
        {
            case "server": 
                NetworkManager.Singleton.StartServer(); 
                break;
            case "host":  
                NetworkManager.Singleton.StartHost(); 
                mainCam.SetActive(false); 
                break;
        }
        codeText.text = serverIP;
        UIparent.SetActive(false);
    }
    void JoinLAN(string serverIP)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverIP, 7777);
        if (NetworkManager.Singleton.StartClient())
        {
            codeText.text = "Server IP: " + serverIP;
            UIparent.SetActive(false);
            mainCam.SetActive(false);
        }
        else
        {
            lanJoinInput.text = "Incorrect IP";
        }
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
                titleText.text = "Code: ";
                break;
            case "lan":
                relayUI.gameObject.SetActive(false);
                lanUI.gameObject.SetActive(true);
                relayUISwitch.interactable = true;
                lanUISwitch.interactable = false;
                relayUISwitch.transform.SetSiblingIndex(0);
                lanUISwitch.transform.SetSiblingIndex(2);
                titleText.text = "Server ip: ";
                break;
        }
        //Debug.Log(currentUI + " => " + changeTo);
        currentUI = changeTo;
    }
}