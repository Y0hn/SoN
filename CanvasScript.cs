using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
public class CanvasScript : MonoBehaviour
{
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject hostingPanel;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

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
}
