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

    public InputField login_account;
    public InputField login_pwd;

    public InputField register_account;
    public InputField register_pwd;

    public InputField connectName;

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
        string h = "";
        int p = 0;
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
        //登入處理
        if (data == "%Login")
        {
            if (nowPanel == loginPanel)
            {
                Send("&Login|Account=" + login_account.text + "|Password=" + login_pwd.text);
            }
            else if (nowPanel == registerPanel)
            {
                Send("&Register|Account=" + register_account.text + "|Password=" + register_pwd.text);
            }
            return;
        }

        //登入處理(Server 回傳)
        if (data.Contains("&Login"))
        {
            //登入成功
            if (data.Split('|')[1] == "1")
            {
                SetName((nowPanel == loginPanel ? login_account.text : register_account.text));
                LoginSuccess();
                Send("&NAME|" + clientName);
            }
            //登入失敗
            else if(data.Split('|')[1] == "0")
            {
                LoginFail();
            }
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
        isLogin = false;
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
        connectName.text = name;
        clientName = name;
    }


    public void OnSendLogin()
    {
        // Validate Input Empty
        if (login_account.text == "" || login_pwd.text == "")
        {
            Debug.Log("Please Enter Your Account And Password");
            return;
        }
       
        ConnectedToServer();
    }

    void LoginSuccess()
    {
        // if success , change to connect panel
        login_account.text = "";
        login_pwd.text = "";
        register_account.text = "";
        register_pwd.text = "";

        isLogin = true;
        OnChangePanel(connectPanel);
    }

    void LoginFail()
    {
        login_account.text = "";
        login_pwd.text = "";
        register_account.text = "";
        register_pwd.text = "";
        // if fail , close Socket
        CloseSocket();
    }

    public void OnSendLogout()
    {
        // if success , change to login panel
        CloseSocket();
        OnChangePanel(loginPanel);
    }

    public void OnSendRegister()
    {
        // Validate Input Empty
        if (register_account.text == "" || register_pwd.text == "")
        {
            Debug.Log("Please Enter Your Account And Password");
            return;
        }


        // if success , change to connect panel
        ConnectedToServer();
    }

    public void ChangeToRegisterPanel()
    {
        OnChangePanel(registerPanel);
    }

    public void ChangeToLoginPanel()
    {
        OnChangePanel(loginPanel);
    }

    void OnChangePanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        connectPanel.SetActive(false);
        nowPanel = panel;
        nowPanel.SetActive(true);
        if (nowPanel  == connectPanel)
        {
            GameObject[] messages = GameObject.FindGameObjectsWithTag("Message");
            if(messages.Length > 0)
            {
                foreach(GameObject msg in messages)
                {
                    Destroy(msg);
                }
            }
        }
    }
}
