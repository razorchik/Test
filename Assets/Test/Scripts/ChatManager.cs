using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using GameSparks.RT;

internal sealed class ChatManager : MonoBehaviour
{
    [SerializeField]
    private InputField messageInput;

    [SerializeField]
    private Dropdown recipientOption;

    [SerializeField]
    private Text chatLogOutput;

    [SerializeField]
    private GameSparksHandler gameSparksHandler;

    private const int elementsInChatLog = 7;

    private readonly Queue<string> chatLog = new Queue<string>();

    private void OnEnable()
    {
        chatLogOutput.text = string.Empty;
    }

    // Use this for initialization
    private void Start()
    {
        UpdadePlayersList();
    }

    public void UpdadePlayersList()
    {
        var options = recipientOption.options;

        options.Clear();

        var players = gameSparksHandler.Players;
        var session = gameSparksHandler.RTSession;

        options.Add(new Dropdown.OptionData("To All"));
        options.AddRange(players.Where(p => p.Peer != session.PeerId && p.OnLine).Select(p => new Dropdown.OptionData {text = p.DisplaName}));
    }

    public void OnMessageReceived(RTPacket packet)
    {
        Debug.Log("Message Received...\n" + packet.Data.GetString(1));

        var player = gameSparksHandler.Players.FirstOrDefault(e => e.Peer == packet.Sender);

        if (player == null)
            return;
        
        UpdateChatLog(player.DisplaName, packet.Data.GetString(1), packet.Data.GetString(2));
    }

    public void SendMessage()
    {
        if (messageInput.text == string.Empty)
        {
            Debug.Log("Not Chat Message To Send...");
            return;
        }

        using (var data = RTData.Get())
        {
            data.SetString(1, messageInput.text); 
            data.SetString(2, DateTime.Now.ToString(CultureInfo.InvariantCulture));
            data.SetInt(10, 1010101);

            var session = gameSparksHandler.RTSession;
            
            if (recipientOption.options[recipientOption.value].text == "To All")
            {
                Debug.Log("Sending Message to All Players... \n" + messageInput.text);

                UpdateChatLog("Me", messageInput.text, DateTime.Now.ToString());

                session.SendData(1, GameSparksRT.DeliveryIntent.RELIABLE, data);
            }
            else
            {
                var player =
                    gameSparksHandler.Players.FirstOrDefault(
                        e => e.DisplaName.Equals(recipientOption.options[recipientOption.value].text));

                if(player == null)
                    return;
                
                Debug.Log("Sending Message to " + player.DisplaName + " ... \n" + messageInput.text);

                UpdateChatLog("To " + player.DisplaName, messageInput.text, DateTime.Now.ToString());

                session.SendData(1, GameSparksRT.DeliveryIntent.RELIABLE, data, player.Peer);
            }
        }

        messageInput.text = string.Empty;
    }

    private void UpdateChatLog(string sender, string message, string date)
    {
        if(string.IsNullOrEmpty(message))
            return;

        chatLog.Enqueue("<b>" + sender + ':' + "</b>\n<color=black>" + message + "</color>" + "\n<i>" + date + "</i>");
        if (chatLog.Count > elementsInChatLog)
            chatLog.Dequeue();

        chatLogOutput.text = string.Empty;

        chatLog.ToList().ForEach(e => chatLogOutput.text += e + '\n');
    }
}
