using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine.UI;

//The client for a multiplayer biplane game
public class Client : NetElement
{
	TcpClient client;
	public Client(string s) : base()
	{
		client = new TcpClient(s, 25566);
		client.GetStream().ReadTimeout = 6000;
		client.GetStream().WriteTimeout = 6000;
		SendUserName();
	}
	
	//Listens for messages from the server
	public override void Listen()
	{
		NetworkStream stream = client.GetStream();
		
		//Performs a non-blocking read for TCP messages from the server
		while (stream.DataAvailable)
		{
			try
			{
				byte[] flag = new byte[1];
				stream.Read(flag,0,1);
				if (flag[0] == (byte)NetFlag.SERVERNEWUSERNAME)
				{
					GetNewUserName(stream);
				}
				else if (flag[0] == (byte)NetFlag.SERVERFULLNAMEUPDATE)
				{
					GetUserNameList(stream);
				}
				else if (flag[0] == (byte)NetFlag.DC)
				{
					ReadDisconnect(stream);
				}
				else if (flag[0] == (byte)NetFlag.NEWPLANE)
				{
					MakeNewPlane(stream);
				}
				else if (flag[0] == (byte)NetFlag.PLANEDATA)
				{
					UpdatePlane(stream);
				}
				else if (flag[0] == (byte)NetFlag.SHOOT)
				{
					byte[] index = new byte[1];
					stream.Read(index, 0, 1);
					int Index = (int)index[0];
					planes[Index].GetComponent<Plane>().FireGuns();
				}
				else if (flag[0] == (byte)NetFlag.PARTDAMAGE)
				{
					string part = GetUserName(stream);
					byte[] val = new byte[4];
					stream.Read(val,0,4);
					int Val = BitConverter.ToInt32(val,0);
					byte[] index = new byte[1];
					stream.Read(index,0,1);
					int Index = (int)index[0];
					if (planes[Index] != null)
					{
						planes[Index].GetComponent<Plane>().ServerDamage(part, Val);
					}
				}
			}
			catch (System.Exception)
			{
				Application.Quit();
			}
		}
		try
		{
			byte[] send = EncapsulatePlaneData(localPlane);
			stream.Write(send,0,send.Length);
		}
		catch (System.Exception)
		{
			Application.Quit();
		}
	}
	
	//Informs the server of the clients local username
	public void SendUserName()
	{
		NetworkStream stream = client.GetStream();
		byte[] nom = TextToUTF8(GameObject.Find("Engine").GetComponent<Engine>().localUser, (byte)NetFlag.USERNAME);
		stream.Write(nom, 0, nom.Length);
	}
	
	//Deletes a player who has disconnected from memory
	void ReadDisconnect(NetworkStream stream)
	{
		byte[] num = new byte[1];
		stream.Read(num,0,1);
		CloseAtIndex((int)num[0]);		
	}
	
	//Gets the name of a new player
	void GetNewUserName(NetworkStream stream)
	{
		string s = GetUserName(stream);
		byte[] slot = new byte[1];
		stream.Read(slot,0,1);
		int index = (int)slot[0];
		nameList[index] = s;
		GameObject.Find("StatusText").GetComponent<Text>().text = s + " has joined the game";
		if (planes[index] != null)
		{
			planes[index].GetComponent<Plane>().SetPlayerName(s);
		}		
	}
	
	//Gets the entire list of player names from the server
	void GetUserNameList(NetworkStream stream)
	{
		string s = GetUserName(stream);
		byte[] slot = new byte[1];
		stream.Read(slot,0,1);
		nameList[(int)slot[0]] = s;
		if (slot[0] == 0)
		{
			GameObject.Find("StatusText").GetComponent<Text>().text = "You have successfully joined the game";
		}	
	}
	
	//Updates the server if the local plane has taken damage
	public override void SendDamage(string s, int val)
	{
		try
		{
			NetworkStream stream = client.GetStream();
			byte[] textPart = TextToUTF8(s,(byte)NetFlag.PARTDAMAGE);
			byte[] Val = BitConverter.GetBytes(val);
			stream.Write(textPart,0,textPart.Length);
			stream.Write(Val,0,Val.Length);
		}
		catch(System.Exception)
		{
			Application.Quit();
		}
	}
	
	//Respawns the local plane and informs the server as to its position
	public override void NewLocalPlane(GameObject g)
	{
		base.NewLocalPlane(g);
		byte[] x = BitConverter.GetBytes(g.transform.position.x);
		byte[] y = BitConverter.GetBytes(g.transform.position.y);
		byte[] z = BitConverter.GetBytes(g.transform.position.z);
		byte[] send = new byte[]{(byte)NetFlag.NEWPLANE};
		try
		{
			if (client != null)
			{
				NetworkStream stream = client.GetStream();
				stream.Write(send,0,1);
				stream.Write(x,0,4);
				stream.Write(y,0,4);
				stream.Write(z,0,4);
			}
		}
		catch(System.Exception)
		{
			Application.Quit();
		}
	}
	
	//Creates a plane for a freshly joined player
	protected void MakeNewPlane(NetworkStream stream)
	{
		byte[] index = new byte[1];
		stream.Read(index,0,1);
		int Index = (int)index[0];
		Vector3 createPos = ReadVector3(stream);
		planes[Index] = GameObject.Find("EnemySpawner").GetComponent<Respawner>().EnemySpawn(createPos);
		if (nameList[Index] != null)
		{
			planes[Index].GetComponent<Plane>().SetPlayerName(nameList[Index]);
		}
	}
	
	//Performs a handshake protocol with the server
	public override void FirstConnect()
	{
		try
		{
			client.GetStream().Write(new byte[]{(byte)NetFlag.FIRSTCONNECT},0,1);
		}
		catch(System.Exception)
		{
			Application.Quit();
		}
	}
	
	//Updates the position of a non-local player controlled plane
	void UpdatePlane(NetworkStream stream)
	{
		byte[] index = new byte[1];
		stream.Read(index,0,1);
		int Index = (int)index[0];
		Vector3 pos = ReadVector3(stream);
		Vector3 rot = ReadVector3(stream);
		if (planes[Index] != null)
		{
			planes[Index].transform.position = pos;
			planes[Index].transform.eulerAngles = rot;
		}
	}
	
	//Informs the server that the client is attempting to fire
	public override void FireShot()
	{
		try
		{
			if (client != null)
			{
				byte[] send = new byte[]{(byte)NetFlag.SHOOT};
				NetworkStream stream = client.GetStream();
				stream.Write(send,0,1);
			}
		}
		catch (System.Exception)
		{
			Application.Quit();
		}
	}
}
