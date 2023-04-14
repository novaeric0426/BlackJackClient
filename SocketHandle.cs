using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GamePacket;
using Google.FlatBuffers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SocketHandle : MonoBehaviour {

    TcpClient clientSocket = new TcpClient ();
    NetworkStream stream = default (NetworkStream);
    private byte[] buff = new byte[1024];
    public string playerName;

    public Queue<ByteBuffer> EventQueue = new Queue<ByteBuffer> ();

    public UnityEvent<int> changePlayerNum;
    public int clientIndex;

    public void Connect () {
        clientSocket.Connect ("127.0.0.1", 11021);
        stream = clientSocket.GetStream ();
        Task.Run (() => receiveData ());
    }

    public async Task receiveData () {
        while (true) {
            var byteCount = await stream.ReadAsync (buff, 0, buff.Length);
            var response = Encoding.UTF8.GetString (buff, 0, byteCount);
            var buf = new ByteBuffer (buff);
            EventQueue.Enqueue (buf);
        }
    }
    public void Send (byte[] buf) {
        stream.WriteAsync (buf, 0, buf.Length);
    }

    public void ProcessPacket (ByteBuffer buf) {
        var packet = Message.GetRootAsMessage (buf);
        if (packet.Func == "SetName") {
            Debug.Log ("SetName : " + packet.Data);
        } else if (packet.Func == "GetJoinPlayerNum") {
            changePlayerNum.Invoke ((int) packet.Card.Value.Rank);
        } else if (packet.Func == "GetClientIndex") {
            Debug.Log ("GetClientIndex");
            clientIndex = (int) packet.Card.Value.Rank;
        } else if (packet.Func == "SetCard") {
            if ((int) packet.Card.Value.Suit == 3) {
                GameManager.Instance.dealerScript.GetCard ((int) packet.Card.Value.Rank);

                return;
            }
            Debug.Log ("SetCard Packet delivered!");
            Debug.Log ("Rank : " + packet.Card.Value.Rank);
            Debug.Log ("Suit : " + packet.Card.Value.Suit);
            GameManager.Instance.playerList[(int) packet.Card.Value.Suit].GetCard ((int) packet.Card.Value.Rank);
            //GameManager.Instance.playerList[(int) packet.Card.Value.Rank].GetCard ((int) packet.Card.Value.Suit);

        } else if (packet.Func == "DealStart") {
            GameManager.Instance.DealStart ();
        } else if (packet.Func == "HitCard") {
            GameManager.Instance.HitClicked ((int) packet.Card.Value.Rank);
        } else if (packet.Func == "ShowCard") {
            GameManager.Instance.ShowOtherCard (packet.Card.Value.Suit, packet.Card.Value.Rank);
        } else if (packet.Func == "RoundDone") {
            GameManager.Instance.HitDealer ();
        } else if (packet.Func == "ChoiceDone") {
            GameManager.Instance.CheckIcon[(int) packet.Card.Value.Rank].enabled = true;
        } else if (packet.Func == "GetJoinedClientNickname") {
            List<string> nameList = packet.Data.Split (',').ToList ();
            for (int i = 0; i < 3; i++) {
                GameManager.Instance.clientNameList[i].text = nameList[i];
            }
        } else if (packet.Func == "AllPlayerJoined") {
            SceneManager.LoadScene ("GameScene");
        }

    }

    public byte[] MakePacket (string data_, short rank_, short suit_, string func_) {
        var builder = new FlatBufferBuilder (1024);
        var data = builder.CreateString (data_);
        var func = builder.CreateString (func_);
        Message.StartMessage (builder);
        Message.AddData (builder, data);
        Message.AddCard (builder, CardData.CreateCardData (builder, rank_, suit_));
        Message.AddFunc (builder, func);
        var pkt = Message.EndMessage (builder);
        builder.Finish (pkt.Value);
        return builder.SizedByteArray ();
    }
    // Start is called before the first frame update
    void Awake () {
        DontDestroyOnLoad (gameObject);
    }

    void Update () {
        if (EventQueue.Count > 0) {
            ProcessPacket (EventQueue.Dequeue ());
        }
    }

}