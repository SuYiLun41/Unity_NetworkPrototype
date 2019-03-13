using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandleNetworkData : MonoBehaviour
{
    private delegate void Packet_(byte[] data);
    private static Dictionary<int, Packet_> Packets;

    public static void InitializeNetworkPackages()
    {
        Debug.Log("Initialize Network Packages.");
        Packets = new Dictionary<int, Packet_>
            {
                { (int)ServerPackets.SConnectionOK , HandleConnectionOK}
            };
    }

    public void Awake()
    {
        InitializeNetworkPackages();
    }

    public static void HandleNetworkInformation(byte[] data)
    {
        int packetnum;
        PacketBuffer buffer = new PacketBuffer();
        Packet_ Packet;
        buffer.WriteBytes(data);
        packetnum = buffer.ReadInteger();
        buffer.Dispose();
        if (Packets.TryGetValue(packetnum, out Packet))
        {
            Packet.Invoke(data);
        }
    }

    //讀取ServerTCP的SendConnectionOK方法傳送之資料
    private static void HandleConnectionOK(byte[] data)
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        int packetnum = buffer.ReadInteger();
        string msg = buffer.ReadString();
        buffer.Dispose();

        //在此加入你要執行的事情
        Debug.Log(msg);
        ClientTCP ctp = new ClientTCP();
        ctp.ThankYouServer();
    }
}
