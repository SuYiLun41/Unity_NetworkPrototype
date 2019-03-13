using System;
using System.Collections.Generic;
using Bindings;

namespace CSharpClient
{
    class ClientHandleNetworkData
    {
        private delegate void Packet_(byte[] data);
        private static Dictionary<int, Packet_> Packets;

        public static void InitializeNetworkPackages()
        {
            Console.WriteLine("Initialize Network Packages.");
            Packets = new Dictionary<int, Packet_>
            {
                { (int)ServerPackets.SConnectionOK , HandleConnectionOK}
            };
        }

        public static void HandleNetworkInformation(byte[] data)
        {
            int packetnum;
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadInteger();
            buffer.Dispose();
            if (Packets.TryGetValue(packetnum, out Packet_ Packet))
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
            Console.WriteLine(msg);
            ClientTCP.ThankYouServer();
        }
    }
}
