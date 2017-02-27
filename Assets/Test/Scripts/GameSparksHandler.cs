using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GameSparks.Core;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.RT;

using UnityEngine;

[RequireComponent(typeof(GameSparksRTUnity), typeof(GameSparksUnity))]
internal sealed class GameSparksHandler : MonoBehaviour
{
    [SerializeField]
    private LoginManager loginManager;

    [SerializeField]
    private ChatManager chatManager;

    [SerializeField]
    private LobbyManager lobbyManager;

    private bool newSession;

    private readonly List<PlayerInfo> players = new List<PlayerInfo>();
    
    private void Start()
    {
        newSession = true;

        GS.GameSparksAvailable += isAvailable =>
        {
            if (isAvailable)
            {
                if (!newSession)
                    return;

                loginManager.onRegistration += n =>
                {
                    lobbyManager.gameObject.SetActive(true);

                    lobbyManager.UpdatePlayerText(n);
                    lobbyManager.UpdatePlayersCount(0);

                    FindPlayers();
                };

                newSession = false;
                loginManager.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("Game Sparks Disconnected!!!");
            }
        };

        MatchFoundMessage.Listener += OnMatchFoundMessage;
        MatchNotFoundMessage.Listener += OnMatchNotFoundMessage;
    }

    public GameSparksRTUnity RTSession
    {
        get { return GetComponent<GameSparksRTUnity>(); }
    }

    public PlayerInfo[] Players
    {
        get { return players.ToArray(); }
    }

    private void OnMatchFoundMessage(MatchFoundMessage msg)
    {       
        players.Clear();
        players.AddRange(msg.Participants.Where(p => p.PeerId != null).Select(p => new PlayerInfo { DisplaName = p.DisplayName, Id = p.Id, Peer = (int)p.PeerId, OnLine = true}));

        RTSession.Configure(msg, OnPlayerConnect, OnPlayerDisconnect, OnReady, OnRTPacket);

        lobbyManager.UpdatePlayersCount(players.Count);

        StartCoroutine(StartSession());
    }

    private void OnMatchNotFoundMessage(MatchNotFoundMessage msg)
    {
        FindPlayers();
    }

    private void OnPlayerConnect(int peerId)
    {
        var find = players.Find(p => p.Peer == peerId);

        if (find != null)
            find.OnLine = true;

        chatManager.UpdadePlayersList();
    }

    private void OnPlayerDisconnect(int peerId)
    {
        var find = players.Find(p => p.Peer == peerId);

        if (find != null)
            find.OnLine = false;

        chatManager.UpdadePlayersList();
    }

    private void OnReady(bool ready)
    {
        if (!ready)
            return;
        Debug.Log("GSM| RT Session Start...");

        chatManager.gameObject.SetActive(true);

        StartCoroutine(SendPackets());
    }

    private void OnRTPacket(RTPacket packet)
    {
        if (packet.OpCode == 1)
            chatManager.OnMessageReceived(packet);
    }

    private IEnumerator SendPackets()
    {
        for (var i = 1; i <= 150; i++)
        {
            RTSession.SendData(1, GameSparksRT.DeliveryIntent.RELIABLE, new RTData().SetInt(1, i));
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator StartSession()
    {
        yield return new WaitForSeconds(2f);

        RTSession.Connect();
        lobbyManager.gameObject.SetActive(false);
    }

    private void FindPlayers()
    {
        var request = new MatchmakingRequest();

        request.SetMatchShortCode("test_match");
        request.SetSkill(0);

        request.Send(r =>
        {
            if (r.HasErrors)
                Debug.LogError("GSM | MatchMaking Error" + r.Errors.JSON);
        });
    }

    public void OnEndChat()
    {
        chatManager.gameObject.SetActive(false);

        lobbyManager.gameObject.SetActive(true);
        lobbyManager.UpdatePlayersCount(0);

        RTSession.Disconnect();
        FindPlayers();
    }
}
