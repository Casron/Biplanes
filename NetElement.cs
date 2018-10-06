using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Reflection;
using UnityEngine.UI;
using System.Net.Sockets;
public enum NetFlag{USERNAME,SERVERNEWUSERNAME,SERVERFULLNAMEUPDATE, DC, NEWPLANE, FIRSTCONNECT, CONTINUEREAD, PLANEDATA, SHOOT, PARTDAMAGE};

//Parent of both server and client
//Provides methods that are usable by both
public abstract class NetElement
{
	protected string[] nameList;
	protected GameObject[] planes;
	protected GameObject localPlane;
	
	public abstract void Listen();
	public abstract void SendDamage(string component, int damage);
	public abstract void FirstConnect();
	public abstract void FireShot();
	
	
	//Initializes the list of other player's name
	public NetElement()
	{
		nameList = new string[20];
		for(int i = 0; i < 20; i++)
		{
			nameList[i] = " ";
		}
		planes = new GameObject[20];
	}
	//Assigns the character the player will control on the local machine
	public virtual void NewLocalPlane(GameObject g)
	{
		localPlane = g;
	}
	
	//Converts strings to a UTF-8 compliant byte array
	protected byte[] TextToUTF8 (string s, byte flag)
	{
		byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(s);
		Int32 length = utf8Bytes.Length;
		byte[] size = BitConverter.GetBytes(length);
		byte[] full = new byte[5 + length];
		full[0] = flag;
		for(int i = 0; i < 4; i++)
		{
			full[i+1] = size[i];
		}
		for(int i = 0; i < length; i++)
		{
			full[i+5] = utf8Bytes[i];
		}
		return full;
	}
	
	//Converts a byte-array back into a string
	protected string GetUserName(NetworkStream stream)
	{
		try
		{
			byte[] size = new byte[4];
			stream.Read(size,0,4);
			Int32 length = BitConverter.ToInt32(size,0);
			byte[] data = new byte[length];
			stream.Read(data,0,length);
			string r = System.Text.Encoding.UTF8.GetString(data);
			return r;
		}
		catch (System.Exception)
		{
			throw;
		}
	}
	
	//When a player disconnects, delete their plane from the game
	protected void CloseAtIndex(int i)
	{
		GameObject.Find("StatusText").GetComponent<Text>().text = nameList[i] + " has disconnected";	
		nameList[i] = " ";
		if (planes[i] != null)
		{
			planes[i].GetComponent<Plane>().Remove();
			planes[i] = null;
		}
	}
	
	//Provides a standardized method to send information regarding the position and rotation of a plane object
	protected byte[] EncapsulatePlaneData(GameObject planeObj)
	{
		Plane plane = planeObj.GetComponent<Plane>();
		byte[] flag = new byte[]{(byte)NetFlag.PLANEDATA};
		Vector3 vec = planeObj.transform.position;
		Vector3 rot = planeObj.transform.eulerAngles;
		byte[] x = BitConverter.GetBytes(vec.x);
		byte[] y = BitConverter.GetBytes(vec.y);
		byte[] z = BitConverter.GetBytes(vec.z);

		byte[] rotX = BitConverter.GetBytes(rot.x);
		byte[] rotY = BitConverter.GetBytes(rot.y);
		byte[] rotZ = BitConverter.GetBytes(rot.z);

		byte[] send = new byte[25];
		send[0] = flag[0];
		for(int i = 0; i < 4; i++)
		{
			send[i+1] = x[i];
			send[i+5] = y[i];
			send[i+9] = z[i];
			send[i+13] = rotX[i];
			send[i+17] = rotY[i];
			send[i+21] = rotZ[i];
		}
		return send;
	}
	
	//Converts 12 bytes into floats representing x,y,z coordinates
	protected Vector3 ReadVector3(NetworkStream stream)
	{
		byte[] x = new byte[4];
		byte[] y = new byte[4];
		byte[] z = new byte[4];

		stream.Read(x,0,4);
		stream.Read(y,0,4);
		stream.Read(z,0,4);

		float X = BitConverter.ToSingle(x,0);
		float Y = BitConverter.ToSingle(y,0);
		float Z = BitConverter.ToSingle(z,0);
		
		return new Vector3(X,Y,Z);
	}
	
	//Converts x,y,z floats into a byte-array
	protected byte[] EncapsulateVector3(Vector3 pos)
	{
		byte[] x = BitConverter.GetBytes(pos.x);
		byte[] y = BitConverter.GetBytes(pos.y);
		byte[] z = BitConverter.GetBytes(pos.z);
		byte[] ret = new byte[12];
		for(int i = 0; i < 4; i++)
		{
			ret[i] = x[i];
			ret[i+4] = y[i];
			ret[i+8] = z[i];
		}
		return ret;
	}
}
