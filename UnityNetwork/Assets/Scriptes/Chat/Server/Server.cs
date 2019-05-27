using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Security.Cryptography;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine;

public class Server: MonoBehaviour
{
    public string host, database, account, password;
    public bool pooling = true;

    private string connectString;
    private MySqlConnection con = null;
    private MySqlCommand cmd = null;
    private MySqlDataReader rdr = null;

    private MD5 _md5Hash;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 5432;
    private TcpListener server;
    private bool serverStarted;
    private void Start()
    {
        ConnectToSQLServer();
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

        for(int i = 0; i < disconnectList.Count - 1; i++)
        {
            BroadCast(disconnectList[i].clientName + " has disconnected.", clients);
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
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
        StartListening();

        //Show all user who is coneected.
        BroadCast("%Login",new List<ServerClient>() { clients[clients.Count - 1] } );
    }
    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            BroadCast(c.clientName + " has connected.", clients);
            return;
        }
        else if(data.Contains("&Login"))
        {
            /*
             * 將 帳號密碼 從 data 中 拆解出來
             data 格式為 "&Login|Account=xxxxxx|Password=ooooo"
            */

            string account = data.Split('|')[1].Split('=')[1];
            string password = data.Split('|')[2].Split('=')[1];

            // 與資料庫比對 是否有此使用者
            MySQLUserLogin(account,password,c);
            return;
        }
        else if (data.Contains("&Register"))
        {
            /*
             * 將 帳號密碼 從 data 中 拆解出來
             data 格式為 "&Register|Account=xxxxxx|Password=ooooo"
            */

            string account = data.Split('|')[1].Split('=')[1];
            string password = data.Split('|')[2].Split('=')[1];

            // 註冊此使用者
            MySQLUserRegister(account, password, c);
            return;
        }
        BroadCast(c.clientName + " : " +data, clients);
    }
    private void BroadCast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error :" + e.Message + "to Client" + c.clientName);
            }
        }
    }

    // MySQL 資料庫
    private void ConnectToSQLServer()
    {
        connectString = "Server=" + host + ";Database=" + database + ";user=" + account + ";password=" + password + ";Pooling=";
        if (pooling)
        {
            connectString += "true;";
        }
        else
        {
            connectString += "false;";
        }

        try
        {
            con = new MySqlConnection(connectString);
            con.Open();
            Debug.Log("MySQL State: " + con.State);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void MySQLUserLogin(string account, string password,ServerClient c)
    {
        string cmd_string = "SELECT * FROM `user` where `account`='" + account + "' AND `password`='" + password + "';";
        cmd = new MySqlCommand(cmd_string, con);
        rdr = cmd.ExecuteReader();
        try
        {
            while (rdr.Read())
            {
                if (rdr.HasRows)
                {
                    BroadCast("&Login|1",new List<ServerClient> {c});
                }
                else
                {
                    BroadCast("&Login|0", new List<ServerClient> { c });
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Select Query Fail!");
        }
        finally
        {
            rdr.Close();
        }
    }

    private void MySQLUserRegister(string account, string password, ServerClient c)
    {
        string cmd_string = "INSERT INTO `user`(`account`,`password`) VALUES('" + account + "','" + password + "');";
        cmd = new MySqlCommand(cmd_string, con);
        rdr = cmd.ExecuteReader();
        rdr.Close();
        cmd_string = "SELECT * FROM `user` where `account`='" + account + "' AND `password`='" + password + "';";
        cmd = new MySqlCommand(cmd_string, con);
        rdr = cmd.ExecuteReader();
        try
        {
            while (rdr.Read())
            {
                if (rdr.HasRows)
                {
                    BroadCast("&Login|1", new List<ServerClient> { c });
                }
                else
                {
                    BroadCast("&Login|0", new List<ServerClient> { c });
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Select Query Fail!");
        }
        finally
        {
            rdr.Close();
        }
    }

    private void OnApplicationQuit()
    {
        if (con != null)
        {
            if (con.State.ToString() != "Closed")
            {
                con.Close();
                Debug.Log("MySQL State: " + con.State);
            }
            con.Dispose();
        }

    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

}
