using Unity.Netcode;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class ChatBehavior : NetworkBehaviour
{
    [SerializeField] InputActionReference inputUIchat;
    [SerializeField] InputActionReference inputUIsend;
    [SerializeField] RectTransform chatBox;
    [SerializeField] TMP_InputField input;
    [SerializeField] TMP_Text text;
    
    private float receiveTimer;
    private const float receiveShow = 5.0f;
    
    public override void OnNetworkSpawn()
    {
        inputUIchat.action.started += OpenChat;
        inputUIsend.action.started += SendMess;
        chatBox.gameObject.SetActive(false);
        input.gameObject.SetActive(false);

        input.text = "";
        text.text = "";

        OnMessage += HandleNewMessage;
        /*GetChatRecordServerRpc(out string record);
        text.text = record;*/
    }
    void Update()
    {
        if (receiveTimer != 0 && Time.time > receiveTimer)
        {
            receiveTimer = 0;
            chatBox.gameObject.SetActive(false);
        }
    }


    void OpenChat(InputAction.CallbackContext context)
    {
        GameManager.instance.chatting = true;
        input.gameObject.SetActive(true);
        input.Select();
        input.ActivateInputField();
    }
    void SendMess(InputAction.CallbackContext context)
    {
        GameManager.instance.chatting = false;
        input.gameObject.SetActive(false);        
        if (input.text.Trim() == "") return;
        SendMsgRpc(GameManager.instance.PlayerName, input.text);
        input.text = "";
        Debug.Log("Message send");
    }

    // EVENTs
    private static event Action<string> OnMessage;
    void HandleNewMessage(string message)
    {
        receiveTimer = Time.time + receiveShow;
        chatBox.gameObject.SetActive(true);
        text.text += message;
    }

    // RPCs
    [Rpc(SendTo.Server)] private void SendMsgRpc(string sender, string msg)
    {
        HandleMsgRpc($"[{sender}]: {msg}");
        Debug.Log("[SERVER]: " + msg);
    }
    [Rpc(SendTo.Everyone)] private void HandleMsgRpc(string msg)
    {
        OnMessage?.Invoke($"\n{msg}");
        Debug.Log(msg);
    }
}