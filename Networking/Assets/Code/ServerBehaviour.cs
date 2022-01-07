using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

public class ServerBehaviour : MonoBehaviour
{
    private const string ADDGAME_URL = "https://studenthome.hku.nl/~dennis.borst/DataBase/AddGame.php";

    public NetworkDriver m_Driver;
    public NativeList<NetworkConnection> m_Connections;

    public UnityEvent<NetworkConnection> onConnectedEvent = new UnityEvent<NetworkConnection>();

    public List<Player> players = new List<Player>();

    private int playerWinId;
    private bool connectionEmpty;

    public void CreateServer(ushort port = 9000)
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log($"Failed to bind to port {port}");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        // AcceptNewConnections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            onConnectedEvent.Invoke(c);
            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    ClientEvents networkEvent = (ClientEvents)stream.ReadUInt();

                    Debug.Log($"Serverbehavior received {networkEvent} event");

                    switch (networkEvent)
                    {
                        case ClientEvents.PlayerConnected:
                            ConnectedHandler(stream, m_Connections[i]);
                            break;
                        case ClientEvents.StartGame:
                            StartGameHandler(stream, m_Connections[i]);
                            break;
                        case ClientEvents.QuitGame:
                            PlayerQuitGameHandler(stream, m_Connections[i]);
                            break;
                        case ClientEvents.PlayCard:
                            PlayCardHandler(stream, m_Connections[i]);
                            break;
                        default:
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }

        if (m_Connections.IsEmpty)
        {
            connectionEmpty = true;
            StartCoroutine(DestroyServerAfterTime());
        }
        else
        {
            connectionEmpty = false;
        }
    }

    private void ConnectedHandler(DataStreamReader stream, NetworkConnection networkConnection)
    {
        int playerAccountId = (int)stream.ReadUInt();
        string playerName = stream.ReadFixedString32().ToString();
        Player player = new Player();
        player.connectionId = networkConnection.InternalId;
        player.accountId = playerAccountId;
        player.name = playerName == string.Empty ? $"Guest {networkConnection.InternalId}" : playerName;
        player.playedCard = CardTypes.Count;
        player.wins = 0;
        players.Add(player);

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
            writer.WriteUInt((uint)ServerEvents.SendPlayers);
            writer.WriteUInt((uint)players[i].connectionId);
            writer.WriteUInt((uint)players.Count);
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                writer.WriteUInt((uint)players[playerIndex].connectionId);
                writer.WriteFixedString32($"{players[playerIndex].name}");
            }
            m_Driver.EndSend(writer);
        }
    }

    private void StartGameHandler(DataStreamReader stream, NetworkConnection networkConnection)
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
            writer.WriteUInt((uint)ServerEvents.StartGame);
            m_Driver.EndSend(writer);
        }
    }

    private void PlayerQuitGameHandler(DataStreamReader stream, NetworkConnection networkConnection)
    {
        int playerLeftId = 0;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (m_Connections[i] == networkConnection)
            {
                playerLeftId = i;
            }
        }

        for (int i = m_Connections.Length - 1; i >= 0; i--)
        {
            //Host left
            if (m_Connections[0] == networkConnection)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
                writer.WriteUInt((uint)ServerEvents.PlayerQuitGame);
                m_Driver.EndSend(writer);
                players.Remove(players[i]);
                continue;
            }

            //Oppnonent left
            if (m_Connections[i] == networkConnection)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
                writer.WriteUInt((uint)ServerEvents.PlayerQuitGame);
                m_Driver.EndSend(writer);
                players.Remove(players[i]);
            }


            //Update other players
            if (m_Connections[i] != networkConnection)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
                writer.WriteUInt((uint)ServerEvents.OpponentLeft);
                writer.WriteUInt((uint)playerLeftId);
                m_Driver.EndSend(writer);
            }
        }

        if (m_Connections[0] == networkConnection)
        {
            StartCoroutine(DestroySelf());
        }
        else
        {
            m_Connections.RemoveAt(1);
        }
    }


    private IEnumerator DestroySelf()
    {
        yield return null;
        Destroy(this);
    }

    private void PlayCardHandler(DataStreamReader stream, NetworkConnection networkConnection)
    {
        CardTypes cardType = (CardTypes)stream.ReadUInt();
        players.Find(x => x.connectionId == networkConnection.InternalId).playedCard = cardType;
        Debug.Log(cardType);
        players.ForEach(x =>
        {
            Debug.Log(x.connectionId);
        });

        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (m_Connections[i] != networkConnection)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
                writer.WriteUInt((uint)ServerEvents.OpponentReady);
                m_Driver.EndSend(writer);
            }
        }

        if (players.TrueForAll(x => x.playedCard != CardTypes.Count))
        {
            PlayerWinConditions();

            for (int i = 0; i < m_Connections.Length; i++)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
                writer.WriteUInt((uint)ServerEvents.StartBattle);
                writer.WriteInt(playerWinId);
                m_Driver.EndSend(writer);

                if (players[i].connectionId == playerWinId)
                {
                    players[i].wins += 1;
                }
            }

            StartCoroutine(TimerForNextRound());
        }
    }

    private void PlayerWinConditions()
    {
        Debug.Log(players[0].playedCard + " | " + players[1].playedCard);

        if (players[0].playedCard == players[1].playedCard)
        {
            //its a draw
            playerWinId = -1;
            return;
        }

        bool doesPlayerOneWin = (players[0].playedCard, players[1].playedCard) switch
        {
            (CardTypes.Paper, CardTypes.Rock) => true,
            (CardTypes.Rock, CardTypes.Scissors) => true,
            (CardTypes.Scissors, CardTypes.Paper) => true,
            _ => false,
        };
        Debug.Log(doesPlayerOneWin);
        if (doesPlayerOneWin)
        {
            //player 1 wins
            playerWinId = 0;
        }
        else
        {
            //player 2 wins
            playerWinId = 1;
        }
    }

    private void StartNewRound()
    {
        int totalWins = players.Sum(x => x.wins);
        Debug.Log(totalWins);
        if (totalWins >= 3)
        {
            GameEnded();
            return;
        }

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
            writer.WriteUInt((uint)ServerEvents.StartNewRound);
            m_Driver.EndSend(writer);

            players[i].playedCard = CardTypes.Count;
        }
    }

    private void GameEnded()
    {
        int playerWinID = players[0].wins > players[1].wins ? players[0].connectionId : players[1].connectionId;
        StartCoroutine(AddGameRequest(playerWinID));

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
            writer.WriteUInt((uint)ServerEvents.GameEnded);
            writer.WriteInt(playerWinID);
            m_Driver.EndSend(writer);
            StartCoroutine(TimerToRestartLobby());
        }
    }

    private void RestartLobby()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].playedCard = CardTypes.Count;
            players[i].wins = 0;
        }

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out writer);
            writer.WriteUInt((uint)ServerEvents.RestartLobby);
            m_Driver.EndSend(writer);
        }
    }

    private IEnumerator TimerForNextRound()
    {
        yield return new WaitForSeconds(3f);
        StartNewRound();
    }

    private IEnumerator TimerToRestartLobby()
    {
        yield return new WaitForSeconds(3f);
        RestartLobby();
    }

    private IEnumerator AddGameRequest(int playerWinID)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerOne", players[0].accountId);
        form.AddField("playerTwo", players[1].accountId);
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log("cd" + players[i].connectionId);
            Debug.Log(players[i].name);
            Debug.Log("ad" + players[i].accountId);
            Debug.Log(players[i].wins);
        }
        Debug.Log("WinID: " + playerWinID);
        Debug.Log($"Winning account id is {players.Find(x => x.connectionId == playerWinID).accountId}");
        form.AddField("winningPlayer", players.Find(x => x.connectionId == playerWinID).accountId);

        UnityWebRequest httpRequest = UnityWebRequest.Post(ADDGAME_URL, form);
        httpRequest.timeout = 15;
        yield return httpRequest.SendWebRequest();
        if (httpRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Game succesfully added");
        }
        else
        {
            Debug.Log("Game failed to add");
        }
    }

    private IEnumerator DestroyServerAfterTime()
    {
        yield return new WaitForSeconds(2);
        if (connectionEmpty) { Destroy(this); }
    }
}

public enum ClientEvents
{
    PlayerConnected = 0,
    StartGame = 1,
    QuitGame = 2,
    PlayCard = 3
}

public enum ServerEvents
{
    SendPlayers = 0,
    OpponentLeft = 1,
    StartGame = 2,
    PlayerQuitGame = 3,
    OpponentReady = 4,
    StartBattle = 5,
    StartNewRound = 6,
    GameEnded = 7,
    RestartLobby = 8
}