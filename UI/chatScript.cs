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
    private bool selected;
    void Start()
    {
        selected = false;
        input.text = "";
        text.text = "";

        OnMessage += HandleNewMessage;

        send.action.started += Send;
        
        input.onSelect.AddListener(Selector);
        input.onDeselect.AddListener(DeSelector);
        input.onValueChanged.AddListener(TextUpdate);
    }
    void Selector   (string arg = "")    { selected = true;  }
    void DeSelector (string arg = "")    { selected = false; }
    void TextUpdate (string arg = "")
    {
        
    }
    void HandleNewMessage(string message)
    {
        text.text += message;
    }
    public void Send(InputAction.CallbackContext context = new())
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