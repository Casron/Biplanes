using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaneComponent : MonoBehaviour 
{
	protected Plane plane;
	protected bool alive = true;
	protected int hp;
	protected float weight;
	public string locName;

	public void SetLastHit(string s)
	{
		if (plane != null)
		{
			plane.SetLastHit(s);
		}
	}
	void Awake()
	{
		SetStartingHP();
	}
	protected virtual void SetStartingHP()
	{
		hp = 100;
		weight = 0.25f;
	}
	protected virtual void Disconnect()
	{
		plane.RemovePlaneComponent(gameObject);
	}
	protected virtual void Die()
	{
		if (alive)
		{
			alive = false;
			Disconnect();
			transform.SetParent(null);
			Rigidbody rb = gameObject.AddComponent<Rigidbody>();
			rb.mass = weight;
		}
	}
	public virtual void Connect(Plane p, string s)
	{
		plane = p;
		locName = s;
	}
	protected virtual void TakeDamage(int value)
	{
		if (hp > 0)
		{
			hp -= value;
			if (hp <= 0)
			{
				Die();
			}
		}
	}
	public void DoDamage(int value)
	{
		if (plane.IsLocalPlayer())
		{
			bool wasAlive = alive;
			TakeDamage(value);
			if (wasAlive)
			{
				GameObject.FindGameObjectWithTag("Engine").GetComponent<Engine>().SendDamage(locName, value);
			}
		}
		else
		{
			TakeDamage(0);
		}
	}
	public void DoDamageServer(int value)
	{
		TakeDamage(value);
	}
	public float GetWeight()
	{
		return weight;
	}
	public bool IsSamePlane(Plane p)
	{
		if (plane == p)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
