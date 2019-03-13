using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class ClientTCP : MonoBehaviour
{
    public string IP_ADRESS;
    public int PORT;

    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
   // private byte[] _asyncbuffer = new byte[1024];

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting to server...");
        _clientSocket.BeginConnect(IP_ADRESS,PORT,new AsyncCallback(ConnectCallback),_clientSocket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        _clientSocket.EndConnect(ar);
        while (true)
        {
            OnReceive();
        }
    }

    private void OnReceive()
    {
        byte[] _sizeinfo = new byte[4];
        byte[] _receivedbuffer = new byte[1024];

        int totalread = 0, currentread = 0;

        try
        {
            currentread = totalread = _clientSocket.Receive(_sizeinfo);
            if (totalread <= 0)
            {
                Console.WriteLine("You are not connected to the server.");
            }
            else
            {
                while (totalread < _sizeinfo.Length && currentread > 0)
                {
                    currentread = _clientSocket.Receive(_sizeinfo, totalread, _sizeinfo.Length - totalread, SocketFlags.None);
                    totalread = currentread;
                }
                int messagesize = 0;
                messagesize |= _sizeinfo[0];
                messagesize |= (_sizeinfo[1] << 8);
                messagesize |= (_sizeinfo[2] << 16);
                messagesize |= (_sizeinfo[3] << 24);

                byte[] data = new byte[messagesize];

                totalread = 0;
                currentread = totalread = _clientSocket.Receive(data, totalread, data.Length - totalread, SocketFlags.None);
                while (totalread < messagesize && currentread > 0)
                {
                    currentread = _clientSocket.Receive(data, totalread, data.Length - totalread, SocketFlags.None);
                }

                ClientHandleNetworkData.HandleNetworkInformation(data);
            }
        }
        catch
        {
            Console.WriteLine("You are not connected to the server.");
        }
    }

    public void SendData(byte[] data)
    {
        _clientSocket.Send(data);
    }

    public void ThankYouServer()
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInteger((int)ClientPackets.CThankYou);
        buffer.WriteString("Connect to the server success!");
        SendData(buffer.ToArray());
        buffer.Dispose();
    }
}
