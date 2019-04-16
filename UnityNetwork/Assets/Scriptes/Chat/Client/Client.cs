using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public void OnConnectedToServer()
    {
        // if already Connected, ignore this function
        if (socketReady)
        {
            return;
        }

        // Default host / port
        string host = "127.0.0.1";
        int port = 5432;

        // Overwrite Default value
        string h;
        int p;
        h = GameObject.Find("Input_IP").GetComponent<InputField>().text;
        int.TryParse(GameObject.Find("Input_Port").GetComponent<InputField>().text,out p);
        if (h != "")
        {
            host = h;
        }
        if(p != 0)
        {
            port = p;
        }

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
