using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour 
{

	public GameObject playerPrefab;
	public void LocalRespawn()
	{
		Invoke("MakeNewPlayer",5.0f);
	}
	void MakeNewPlayer()
	{
		float x = 0.0f;
		float z = 0.0f;
		float y = 100.0f;
		bool nope = true;
		int passes = 0;
		while(nope && passes < 5)
		{
			passes++;
			x = Random.Range(-600.0f,600.0f);
			z = Random.Range(-600.0f,600.0f);
			Vector3 reference = new Vector3(x,300.0f,z);
			transform.position = reference;
			Ray r = new Ray(transform.position,transform.up * -300.0f);
			RaycastHit[] hits = Physics.RaycastAll(r,300.0f);
			for(int i = 0 ; i < hits.Length; i++)
			{
				if (hits[i].collider.gameObject.tag == "Ground")
				{
					if (hits[i].point.y < 150.0f)
					{
						y = hits[i].point.y;
						nope = false;
						break;
					}
				}
			}
		}
		Vector3 spawn = new Vector3(x,y + 60.0f,z);
		Instantiate(playerPrefab, spawn, Quaternion.identity);
	}
	public GameObject EnemySpawn(Vector3 sPos)
	{
		return (GameObject)Instantiate(playerPrefab, sPos,Quaternion.identity);
	}
}
