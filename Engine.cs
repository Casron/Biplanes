using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Engine : MonoBehaviour 
{

	NetElement socket;
	public InputField nameText;
	public Text connectText;
	public string localUser;

	// Use this for initialization
	void Start () 
	{
		DontDestroyOnLoad(gameObject);
	}

	public void StartServer()
	{
		try
		{
			localUser = nameText.text;
			socket = new Server();
			InvokeRepeating("Listen",0.04f, 0.04f);
			SceneManager.LoadScene("GameScene");
		}
		catch (System.Exception)
		{
			return;
		}
	}

	public void ClientConnect()
	{
		try
		{
			localUser = nameText.text;
			socket = new Client(connectText.text);
			InvokeRepeating("Listen",0.04f, 0.04f);
			SceneManager.LoadScene("GameScene");
		}
		catch(System.Exception)
		{
			throw;
		}
	}
	public void FirstConnect()
	{
		socket.FirstConnect();
	}
	void Listen()
	{
		socket.Listen();
	}

	public void ReceiveDamage()
	{

	}
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CloseApplication();
		}
	}
	void CloseApplication()
	{
		Application.Quit();
	}
	public void SendDamage(string component, int damage)
	{
		socket.SendDamage(component, damage);
	}
	public void LocalPlayerInit(GameObject g)
	{
		socket.NewLocalPlane(g);
	}
	public void LocalShoot()
	{
		socket.FireShot();
	}
}
