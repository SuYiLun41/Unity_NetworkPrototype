using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server: MonoBehaviour
{
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 5432;
    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        try
        {
            server = new TcpListener(IPAddress.Any,port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Server has been started on port " + port.ToString());
        }
        catch(Exception e)
        {
            Debug.Log("Socket error:" + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted)
            return;
        
        //Listening client's action
        foreach(ServerClient c  in clients)
        {
            // Is the client still connected?

            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            //Check for message from the client
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if(data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
    }
    
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if(c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpCLient, server);
    }
    private void AcceptTcpCLient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));

        //Show User Is Coneected
    }
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log(c.clientName + " has sent the fllowing message :" + data);
    }
    private void BroadCast()
    {

    }
}

public class ServerClient: MonoBehaviour
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

}
