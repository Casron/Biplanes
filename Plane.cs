using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

public class Plane : MonoBehaviour 
{
	public GameObject leftWing;
	public GameObject rightWing;
	public GameObject topWing;
	public GameObject fussilage;
	public GameObject propeller;
	public GameObject pilot;
	public GameObject leftGun;
	public GameObject rightGun;
	public GameObject stick1;
	public GameObject stick2;
	public GameObject stick3;
	public GameObject stick4;
	Rigidbody rB;
	string playerName;
	string lastHitter;
	bool localPlayer;
	bool alive = true;
	GameObject deathText;
	int sticksRemaining;
	bool firstPerson = false;
	int ID;


	const float g = -10.0f;

	void Start()
	{
		rB = gameObject.GetComponent<Rigidbody>();
		lastHitter = "Lack of Skill";
		alive = true;
		ConnectAll();
		sticksRemaining = 4;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		SetMass();
	}
	public Rigidbody GetRigidbody()
	{
		return rB;
	}
	public bool IsLocalPlayer()
	{
		return localPlayer;
	}
	public void SetPlayerName(string s)
	{
		Debug.Log(s);
		playerName = s;
	}
	public string GetPlayerName()
	{
		return playerName;
	}
	public void SetLastHit(string s)
	{
		lastHitter = s;
	}
	void FixedUpdate()
	{
		Vector3 grav = new Vector3(0.0f,g * rB.mass,0.0f);
		rB.AddForce(grav, ForceMode.Force);
		float x = Mathf.Abs(transform.position.x);
		float z = Mathf.Abs(transform.position.z);
		float y = transform.position.y;
		if (x > 1500.0f || z > 1500.0f)
		{
			Kill("Going out of bounds");
		}
		if (y > 400 || y < 0)
		{
			if (propeller != null)
			{
				propeller.GetComponent<Propeller>().DoDamage(1000);
				Kill("Engine Stall");
			}
		}
		if (Input.GetKeyDown(KeyCode.F) && localPlayer)
		{
			if (!firstPerson)
			{
				GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition = new Vector3(0.0f,0.0f,0.0f);
				firstPerson = true;
			}
			else
			{
				GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition = new Vector3(0.0f,2.8f,-11.0f);
				firstPerson = false;
			}
		}
	}
	public void RemovePlaneComponent(GameObject g)
	{
		FieldInfo[] fI = typeof(Plane).GetFields();
		for(int i = 0; i < fI.Length;i++)
		{
			GameObject compare = (GameObject)(fI[i].GetValue(this));
			if (g == compare && compare != null && g != null)
			{
				if (g == fussilage)
				{
					Kill("Fussilage destruction");
					if (pilot != null)
					{
						pilot.GetComponent<PlaneComponent>().DoDamage(1000);
					}
					if (propeller != null)
					{
						propeller.GetComponent<PlaneComponent>().DoDamage(1000);
					}
				}
				if (g == pilot)
				{
					Kill("Pilot Death");
				}
				if (g == topWing)
				{
					sticksRemaining = -2;
					if (stick1 != null)
					{
						stick1.GetComponent<PlaneComponent>().DoDamage(1000);
					}
					if (stick2 != null)
					{
						stick2.GetComponent<PlaneComponent>().DoDamage(1000);
					}
					if (stick3 != null)
					{
						stick3.GetComponent<PlaneComponent>().DoDamage(1000);
					}
					if (stick4 != null)
					{
						stick4.GetComponent<PlaneComponent>().DoDamage(1000);
					}
				}
				if (g.GetComponent<WingStick>() != null)
				{
					sticksRemaining -= 1;
					if (sticksRemaining == 1)
					{
						if (topWing != null)
						{
							topWing.GetComponent<PlaneComponent>().DoDamage(1000);
						}
					}
				}
				fI[i].SetValue(this, null);
			}
		}
		SetMass();
	}
	public GameObject GetPropeller()
	{
		return propeller;
	}
	public void FireGuns()
	{
		if (alive)
		{
			bool ammo = false;
			if (leftGun != null)
			{
				if (leftGun.GetComponent<PlaneGun>().GetAmmo())
				{
					ammo = true;
				}
				leftGun.GetComponent<PlaneGun>().Shoot();
			}
			if (rightGun != null)
			{
				if (rightGun.GetComponent<PlaneGun>().GetAmmo())
				{
					ammo = true;
				}
				rightGun.GetComponent<PlaneGun>().Shoot();
			}
			if (ammo && localPlayer)
			{
				GameObject.FindGameObjectWithTag("Engine").GetComponent<Engine>().LocalShoot();
			}
		}
	}
	public void SetPlayer()
	{
		localPlayer = true;
		deathText = GameObject.Find("DeathText");
		deathText.SetActive(false);
	}
	void OnCollisionEnter(Collision hit)
	{
		PlaneComponent pC = hit.collider.gameObject.GetComponent<PlaneComponent>(); 
		if (pC != null)
		{
			pC.DoDamage(1000);
		}
	}
	void SetMass()
	{
		rB.mass = 0.25f;
		FieldInfo[] fI = typeof(Plane).GetFields();
		for(int i = 0; i < fI.Length;i++)
		{
			GameObject g = (GameObject)(fI[i].GetValue(this));
			if (g != null)
			{
				rB.mass += g.GetComponent<PlaneComponent>().GetWeight();
			}
		}
	}
	void ConnectAll()
	{
		FieldInfo[] fI = typeof(Plane).GetFields();
		for(int i = 0; i < fI.Length;i++)
		{
			GameObject g = (GameObject)(fI[i].GetValue(this));
			if (g != null)
			{
				g.GetComponent<PlaneComponent>().Connect(this, fI[i].Name);
			}
		}		
	}
	public float GetGravity()
	{
		return g * rB.mass;
	}
	public void Kill(string causeOfDeath)
	{
		if (alive)
		{
			alive = false;
			if (localPlayer)
			{
				deathText.SetActive(true);
				deathText.GetComponent<Text>().text = "Oof, you were killed....\n\nCause of Death: " + causeOfDeath + " \n\nKiller: " + lastHitter;
				GameObject.Find("Respawner").GetComponent<Respawner>().LocalRespawn();
			}
			if (pilot != null)
			{
				pilot.GetComponent<Pilot>().DoDamage(1000);
			}
		}
	}
	public void ServerDamage(string part, int Val)
	{
		FieldInfo[] fI = typeof(Plane).GetFields();
		for(int i = 0; i < fI.Length; i++)
		{
			if (fI[i].Name.Equals(part))
			{
				if (fI[i].GetValue(this) != null)
				{
					GameObject gg = (GameObject)fI[i].GetValue(this);
					gg.GetComponent<PlaneComponent>().DoDamageServer(Val);
				}
			}
		}
	}
	public void Remove()
	{
		Destroy(gameObject);
	}
}
