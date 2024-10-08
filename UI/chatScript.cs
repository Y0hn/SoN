using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class ChatBehavior : NetworkBehaviour
{
    [SerializeField] ColorChainReference refer;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_InputField input;
    [SerializeField] InputActionReference send;
    private static event Action<string> OnMessage;
    void Start()
    {
        OnMessage += HandleNewMessage;
        send.action.started += Send;
    }
    void Update()
    {

    }
    void HandleNewMessage(string message)
    {
        text.text += "\n" + message;
    }
    public void Send(InputAction.CallbackContext context)
    {
        if (string.IsNullOrWhiteSpace(input.text))
            return;
        SendMsgServerRpc(input.text);
        input.text = string.Empty;
    }
    [ServerRpc]
    private void SendMsgServerRpc(string msg)
    {
        // validate if profanity ...
        HandleMsgClientRpc($"[{GameManager.instance.PlayerName}]: {msg}");
    }
    [ClientRpc]
    private void HandleMsgClientRpc(string msg)
    {
        OnMessage?.Invoke($"\n{msg}");
    }
}