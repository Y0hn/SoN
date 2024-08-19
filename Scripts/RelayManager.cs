using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
public class RelayManager : MonoBehaviour
{
    [SerializeField] GameObject mainCam;
    [SerializeField] Button hostBtn;
    [SerializeField] Button joinBtn;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TMP_Text codeText;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostBtn.onClick.AddListener(CreateRelay);
        joinBtn.onClick.AddListener(() => JoinRelay(joinInput.text));

    }
    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = "Code: " + joinCode;

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
        NetworkManager.Singleton.StartHost();
        Debug.Log("Code: " + joinCode);
        mainCam.SetActive(false);
    }
    async void JoinRelay(string joinCode)
    {
        if (joinCode != "")
        {            
            var JoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(JoinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            mainCam.SetActive(false);
        }
    }
    /*
    public void Networking(string role)
    {
        hostingPanel.SetActive(false);
        mainCam.SetActive(false);
        switch (role)
        {
            case "host":
                NetworkManager.Singleton.StartHost();
                Debug.Log("Host started");
                break;
            case "client":
                NetworkManager.Singleton.StartClient();
                Debug.Log("Client started");
                break;
            case "server":
                NetworkManager.Singleton.StartServer();
                Debug.Log("Server started");
                mainCam.SetActive(true);
                break;
            default:
                Debug.LogWarning("Networking defaulted!");
                hostingPanel.SetActive(true);
                mainCam.SetActive(true);
                break;
        }
    }
    */
}