using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;

//The server component of a multiplayer bi-plane game
public class Server : NetElement 
{
	TcpClient[] clients;
	TcpListener server;
	const int connectionSize = 19;
	public Server() : base()
	{
		nameList[0] = GameObject.Find("Engine").GetComponent<Engine>().localUser;
		IPAddress localIP = null;
		var host = Dns.GetHostEntry(Dns.GetHostName());
		
		//Finds the local ip address of the current machine
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				localIP = ip;
				break;
			}
		}
		
		clients = new TcpClient[connectionSize];
		server = new TcpListener(localIP, 25566);
		server.Start();
	}


	public override void Listen()
	{
		if (server != null)
		{
			CheckNewConnections();
			for(int i = 0; i < connectionSize; i++)
			{
				if (clients[i] != null)
				{
					try
					{
						NetworkStream stream = clients[i].GetStream();
						
						//Performs a non-blocking read of the data stream from each client
						while (stream.DataAvailable)
						{
							byte[] flag = new byte[1];
							stream.Read(flag,0,1);
							if (flag[0] == (byte)NetFlag.USERNAME)
							{
								NewUser(stream, i);
							}
							else if (flag[0] == (byte)NetFlag.FIRSTCONNECT)
							{
								SendAllPlanes(stream, i);
							}
							else if(flag[0] == (byte)NetFlag.NEWPLANE)
							{
								MakeNewPlane(stream, i+1);
								SendNewPlane(i+1,i);
							}
							else if (flag[0] == (byte)NetFlag.PLANEDATA)
							{
								UpdatePlane(stream, i);
							}
							else if (flag[0] == (byte)NetFlag.SHOOT)
							{
								planes[i + 1].GetComponent<Plane>().FireGuns();
								FireShot(i);
							}
							else if (flag[0] == (byte)NetFlag.PARTDAMAGE)
							{
								string part = GetUserName(stream);
								byte[] val = new byte[4];
								stream.Read(val,0,4);
								int Val = BitConverter.ToInt32(val,0);
								int Index = i + 1;
								if (planes[Index] != null)
								{
									planes[Index].GetComponent<Plane>().ServerDamage(part, Val);
								}
								SendDamageToAll(part, val, i);
							}
							else
							{
								CloseClient(i);
								break;
							}
						}
						SendPlaneUpdate(i, stream);
					}
					catch (System.Exception)
					{
						CloseClient(i);
					}
				}
			}
		}
	}

	//Updates all clients when a plane has taken damage
	void SendDamageToAll(string s, byte[] val, int not)
	{
		byte[] str = TextToUTF8(s, (byte)NetFlag.PARTDAMAGE);
		for(int i = 0; i < connectionSize; i++)
		{
			if (i != not)
			{
				if (clients[i] != null)
				{
					try
					{
						NetworkStream stream = clients[i].GetStream();
						byte[] index = new byte[]{(byte)(not + 1)};
						stream.Write(str, 0, str.Length);
						stream.Write(val, 0, val.Length);
						stream.Write(index,0,1);
					}
					catch(System.Exception)
					{
						CloseClient(i);
					}
				}
			}
		}
	}
	
	//Updates the position of a plane for each client
	void SendPlaneUpdate(int Index, NetworkStream stream)
	{
		for(int i = 0; i < 20; i++)
		{
			if (planes[i] != null)
			{
				if (i != Index + 1)
				{
					byte[] header = new byte[]{(byte)NetFlag.PLANEDATA, (byte)i};
					byte[] pos = EncapsulateVector3(planes[i].transform.position);
					byte[] rot = EncapsulateVector3(planes[i].transform.eulerAngles);
					stream.Write(header,0,2);
					stream.Write(pos,0,pos.Length);
					stream.Write(rot,0,rot.Length);
				}
			}
		}
	}
	
	//Updates the position of a plane on the local machine
	void UpdatePlane(NetworkStream stream, int index)
	{
		Vector3 pos = ReadVector3(stream);
		Vector3 rot = ReadVector3(stream);

		if (planes[index + 1] != null)
		{
			planes[index + 1].transform.position = Vector3.Lerp(planes[index + 1].transform.position, pos 0.04f);
			planes[index + 1].transform.eulerAngles = rot;
		}
	}
	
	void CheckNewConnections()
	{
		//Performs a non-blocking read for incoming connections
		if (server.Pending())
		{
			try
			{
				TcpClient client = server.AcceptTcpClient();
				for(int i = 0; i < connectionSize; i++)
				{
					if (clients[i] == null)
					{
						clients[i] = client;
						clients[i].GetStream().ReadTimeout = 6000;
						clients[i].GetStream().WriteTimeout = 6000;
						break;
					}
					else if (i == connectionSize - 1 && clients[i] != null)
					{
						client.Close();
						client = null;
					}
				}
			}
			catch (System.Exception)
			{
				return;
			}
		}
	}
	
	//Alerts all other clients that a new player has joined the game
	void NewUser(NetworkStream stream, int i)
	{
		string newUserName = GetUserName(stream);
		nameList[i + 1] = newUserName;
		GameObject.Find("StatusText").GetComponent<Text>().text = newUserName + " has joined the game";	
		for(int q = 0; q < connectionSize; q++)
		{
			if (clients[q] != null)
			{
				if (q != i)
				{
					NetworkStream otherStream = clients[q].GetStream();
					byte[] newUserUpdate = TextToUTF8(newUserName,(byte)NetFlag.SERVERNEWUSERNAME);
					byte[] send = new byte[newUserUpdate.Length + 1];
					for(int d = 0; d < newUserUpdate.Length; d++)
					{
						send[d] = newUserUpdate[d];
					}
					send[send.Length - 1] = (byte)i;
					otherStream.Write(send,0,send.Length);
				}
				else
				{
					for(int d = 0; d < nameList.Length; d++)
					{
						byte[] newUserUpdate = TextToUTF8(nameList[d],(byte)NetFlag.SERVERFULLNAMEUPDATE);
						byte[] send = new byte[newUserUpdate.Length + 1];
						for(int z = 0; z < newUserUpdate.Length; z++)
						{
							send[z] = newUserUpdate[z];

						}
						send[send.Length - 1] = (byte)d;
						stream.Write(send,0,send.Length);
					}
				}
			}
		}	
	}
	
	//Closes the connection
	void CloseClient(int i)
	{

		if (clients[i] != null)
		{
			clients[i].Close();
		}
		clients[i] = null;
		HandleDisconnect(i+1);
	}
	
	//Alerts all other clients about the disconnect
	void HandleDisconnect(int i)
	{
		CloseAtIndex(i);
		for(int q = 0; q < connectionSize; q++)
		{
			if (q != i)
			{
				if (clients[q] != null)
				{
					try
					{
						byte[] send = new byte[]{(byte)NetFlag.DC,(byte)i};
						clients[q].GetStream().Write(send,0,send.Length);
					}
					catch (System.Exception)
					{
						CloseClient(q);
					}
				}
			}
		}
	}
	
	//Informs all other clients that a plane has taken damage
	public override void SendDamage(string s, int val)
	{
		for(int i = 0; i < connectionSize; i++)
		{
			if (clients[i] != null)
			{
				try
				{
					NetworkStream stream = clients[i].GetStream();
					byte[] textPart = TextToUTF8(s,(byte)NetFlag.PARTDAMAGE);
					byte[] index = new byte[]{0};
					byte[] Val = BitConverter.GetBytes(val);
					stream.Write(textPart,0,textPart.Length);
					stream.Write(Val,0,Val.Length);
					stream.Write(index,0,1);
				}
				catch(System.Exception)
				{
					CloseClient(i);
				}
			}
		}
	
	
	//Sets the plane for the local machine
	public override void NewLocalPlane(GameObject g)
	{
		base.NewLocalPlane(g);
		planes[0] = g;
		SendNewPlane(0, -1);
	}
	
	//Informs all other clients about the creation of a new plane
	public void SendNewPlane(int Index, int not)
	{
		byte index = (byte)Index;
		byte[] pos = EncapsulateVector3(planes[Index].transform.position);
		byte[] send = new byte[]{(byte)NetFlag.NEWPLANE, index};
		for(int i = 0; i < connectionSize; i++)
		{
			if (i != not)
			{
				if (clients[i] != null)
				{
					try
					{
						NetworkStream stream = clients[i].GetStream();
						stream.Write(send,0,2);
						stream.Write(pos,0,pos.Length);
					}
					catch (System.Exception)
					{
						CloseClient(i);
					}
				}
			}
		}
	}
	
	//Creates a new plane gameobject
	protected void MakeNewPlane(NetworkStream stream, int Index)
	{
		Vector3 createPos = ReadVector3(stream);
		planes[Index] = GameObject.Find("EnemySpawner").GetComponent<Respawner>().EnemySpawn(createPos);
		planes[Index].GetComponent<Plane>().SetPlayerName(nameList[Index]);
	}
	
	//When a player first connects. Inform them about all existing planes
	void SendAllPlanes(NetworkStream stream, int notIndex)
	{
		for(int i = 0; i < 20; i++)
		{
			if (i != notIndex + 1)
			{
				if (planes[i] != null)
				{
					byte[] pos = EncapsulateVector3(planes[i].transform.position);
					byte[] send = new byte[]{(byte)NetFlag.NEWPLANE, (byte)i};
					stream.Write(send,0,2);		
					stream.Write(pos,0,pos.Length);		
				}
			}
		}
	}
	

	public override void FirstConnect()
	{
	}
	
	//Informs each client that the local machine has fired a shot
	public override void FireShot()
	{
		for(int i = 0; i < connectionSize; i++)
		{
			if (clients[i] != null)
			{
				try
				{
					byte[] send = new byte[]{(byte)NetFlag.SHOOT, 0};
					NetworkStream stream = clients[i].GetStream();
					stream.Write(send,0,2);
				}
				catch (System.Exception)
				{
					CloseClient(i);
				}
			}
		}
	}
	
	//Informs all other clients that a plane has fired a shot
	public void FireShot(int index)
	{
		for(int i = 0; i < connectionSize; i++)
		{
			if (i != index)
			{
				if (clients[i] != null)
				{
					try
					{
						byte[] send = new byte[]{(byte)NetFlag.SHOOT, (byte)(index + 1)};
						NetworkStream stream = clients[i].GetStream();
						stream.Write(send,0,2);
					}
					catch (System.Exception)
					{
						CloseClient(i);
					}
				}
			}
		}
	}
}
