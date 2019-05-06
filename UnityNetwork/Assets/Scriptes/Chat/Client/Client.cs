using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public GameObject chatContainer;
    public GameObject messagePrefabs;

    public string clientName;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;


    bool isLogin;

    private GameObject nowPanel;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject connectPanel;

    void Awake()
    {
        isLogin = false;
        OnChangePanel(loginPanel);
    }


    public void ConnectedToServer()
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
        int.TryParse(GameObject.Find("Input_Port").GetComponent<InputField>().text, out p);
        if (h != "")
        {
            host = h;
        }
        if (p != 0)
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

        if (socketReady)
        {
            GameObject.Find("Input_IP").GetComponent<InputField>().interactable = false;
            GameObject.Find("Input_Port").GetComponent<InputField>().interactable = false;
            GameObject.Find("NameInput").GetComponent<InputField>().interactable = false;
            GameObject.Find("ConnectButton").GetComponent<Button>().interactable = false;
            GameObject.Find("SendInput").GetComponent<InputField>().interactable = true;
            GameObject.Find("SendButton").GetComponent<Button>().interactable = true;
            GameObject.Find("DisconnectButton").GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    private void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            Send("&NAME|" + clientName);
            return;
        }

        GameObject go = Instantiate(messagePrefabs, chatContainer.transform) as GameObject;
        go.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data) {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton()
    {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;


        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
        if (!socketReady)
        {
            GameObject.Find("Input_IP").GetComponent<InputField>().interactable = true;
            GameObject.Find("Input_Port").GetComponent<InputField>().interactable = true;
            GameObject.Find("NameInput").GetComponent<InputField>().interactable = true;
            GameObject.Find("ConnectButton").GetComponent<Button>().interactable = true;
            GameObject.Find("SendInput").GetComponent<InputField>().interactable = false;
            GameObject.Find("SendButton").GetComponent<Button>().interactable = false;
            GameObject.Find("DisconnectButton").GetComponent<Button>().interactable = false;
        }
    }

    public void OnDisconnectedButton()
    {
        CloseSocket();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void SetName(string name)
    {
        clientName = name;
    }


    public void OnNameChange()
    {
        SetName(GameObject.Find("NameInput").GetComponent<InputField>().text);
    }

    public void OnSendLogin()
    {
        // Validate User

        // if success , change to connect panel
        isLogin = true;
        OnChangePanel(connectPanel);
    }

    public void OnSendLogout()
    {
        // Logout User

        // if success , change to connect panel
        isLogin = false;
        OnChangePanel(loginPanel);
    }

    public void OnSendRegister()
    {
        // Validate Whther User Is Registed


        // if success , change to connect panel
        isLogin = true;
        OnChangePanel(connectPanel);
    }

    public void ChangeToRegisterPanel()
    {
        OnChangePanel(registerPanel);
    }

    void OnChangePanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        connectPanel.SetActive(false);
        nowPanel = panel;
        nowPanel.SetActive(true);
    }
}
