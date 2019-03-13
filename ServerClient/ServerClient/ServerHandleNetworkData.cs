using System;
using System.Collections.Generic;
using Bindings;

namespace CSharpServer
{
    class ServerHandleNetworkData
    {
        private delegate void Packet_(int index,byte[] data);
        private static Dictionary<int, Packet_> Packets;

        public static void InitializeNetworkPackages()
        {
            Console.WriteLine("Initialize Network Packages.");
            Packets = new Dictionary<int, Packet_>
            {
                { (int)ClientPackets.CThankYou , HandleThankYou}
            };
        }

        public static void HandleNetworkInformation(int index,byte[] data)
        {
            int packetnum;
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadInteger();
            buffer.Dispose();
            if (Packets.TryGetValue(packetnum, out Packet_ Packet))
            {
                Packet.Invoke(index,data);
            }
        }

        //讀取ServerTCP的SendConnectionOK方法傳送之資料
        private static void HandleThankYou(int index,byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //在此加入你要執行的事情
            Console.WriteLine(msg);
        }
    }
}
