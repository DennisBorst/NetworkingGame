using UnityEngine;

using Unity.Networking.Transport;
using UnityEngine.Events;
using System.Collections.Generic;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;

    public UnityEvent onConnectedEvent = new UnityEvent();
    public UnityEvent<bool> onHostEvent = new UnityEvent<bool>();
    public UnityEvent playersUpdatedEvent = new UnityEvent();
    public UnityEvent startGameEvent = new UnityEvent();
    public UnityEvent playerQuitGameEvent = new UnityEvent();
    public UnityEvent opponentReadyEvent = new UnityEvent();
    public UnityEvent<Endstate> startBattleEvent = new UnityEvent<Endstate>();
    public UnityEvent startNewRoundEvent = new UnityEvent();
    public UnityEvent<bool> gameHasEndedEvent = new UnityEvent<bool>();
    public UnityEvent restartLobbyEvent = new UnityEvent();
    public List<Player> players = new List<Player>();

    private uint playerID;

    public void TryToConnect(ushort portNumber = 9000)
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = portNumber;
        m_Connection = m_Driver.Connect(endpoint);



    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    public void StartGame()
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        writer.WriteUInt((int)ClientEvents.StartGame);
        m_Driver.EndSend(writer);
    }

    public void QuitGame()
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        writer.WriteUInt((int)ClientEvents.QuitGame);
        m_Driver.EndSend(writer);
    }

    public void PlayCard(Card selectedCard)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        writer.WriteUInt((int)ClientEvents.PlayCard);
        writer.WriteUInt((uint)selectedCard.cardType);
        m_Driver.EndSend(writer);
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                DataStreamWriter writer;
                Debug.Log("We are now connected to the server");
                m_Driver.BeginSend(m_Connection, out writer);
                writer.WriteUInt((int)ClientEvents.PlayerConnected);
                writer.WriteUInt((uint)Account.Instance.Id);
                writer.WriteFixedString32(Account.Instance.Username);
                m_Driver.EndSend(writer);
                onConnectedEvent.Invoke();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                ServerEvents networkEvent = (ServerEvents)stream.ReadUInt();

                Debug.Log($"Clientbehavior received {networkEvent} event");

                switch (networkEvent)
                {
                    case ServerEvents.SendPlayers:
                        UpdatePlayers(stream);
                        break;
                    case ServerEvents.OpponentLeft:
                        OpponentQuit(stream);
                        break;
                    case ServerEvents.StartGame:
                        StartGameHandler(stream);
                        break;
                    case ServerEvents.PlayerQuitGame:
                        PlayerQuitGameHandler();
                        break;
                    case ServerEvents.OpponentReady:
                        OpponentReadyHandler(stream);
                        break;
                    case ServerEvents.StartBattle:
                        StartBattleHandler(stream);
                        break;
                    case ServerEvents.StartNewRound:
                        StartNewRound(stream);
                        break;
                    case ServerEvents.GameEnded:
                        GameHasEnded(stream);
                        break;
                    case ServerEvents.RestartLobby:
                        GoToLobby(stream);
                        break;
                    default:
                        break;
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                PlayerQuitGameHandler();
                m_Connection = default(NetworkConnection);
            }
        }
    }

    private void UpdatePlayers(DataStreamReader stream)
    {
        players.Clear();
        playerID = stream.ReadUInt();
        uint playerCount = stream.ReadUInt();
        if(playerID == 0 && playerCount >= 2) { onHostEvent.Invoke(true); }
        for (int i = 0; i < playerCount; i++)
        {
            Player player = new Player();
            player.connectionId = (int)stream.ReadUInt();
            player.name = stream.ReadFixedString32().ToString();
            Debug.Log(player.connectionId + "" + player.name);
            players.Add(player);
        }

        playersUpdatedEvent.Invoke();
    }

    private void PlayerQuitGameHandler()
    {
        m_Driver.Disconnect(m_Connection);
        playerQuitGameEvent.Invoke();
        Destroy(this);
    }

    private void OpponentQuit(DataStreamReader stream)
    {
        playerID = stream.ReadUInt();
        players.Remove(players[(int)playerID]);

        playersUpdatedEvent.Invoke();
        onHostEvent.Invoke(false);
    }

    private void StartGameHandler(DataStreamReader stream)
    {
        startGameEvent.Invoke();
    }

    private void OpponentReadyHandler(DataStreamReader stream)
    {
        opponentReadyEvent.Invoke();
    }

    private void StartBattleHandler(DataStreamReader stream)
    {
        int playerWinId = stream.ReadInt();
        Endstate currentEndstate;
        if(playerWinId == -1) { currentEndstate = Endstate.Draw; }
        else if(playerWinId == playerID) { currentEndstate = Endstate.Win; }
        else { currentEndstate = Endstate.Lose; }
        startBattleEvent.Invoke(currentEndstate);
    }

    private void StartNewRound(DataStreamReader stream)
    {
        startNewRoundEvent.Invoke();
    }

    private void GameHasEnded(DataStreamReader stream)
    {
        int playerWinID = stream.ReadInt();
        bool IWon = playerWinID == playerID;
        gameHasEndedEvent.Invoke(IWon);
    }

    private void GoToLobby(DataStreamReader stream)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].playedCard = CardTypes.Count;
            players[i].wins = 0;
        }

        restartLobbyEvent.Invoke();
    }
}

public enum Endstate
{
    Draw,
    Win,
    Lose
}