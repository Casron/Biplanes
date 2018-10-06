using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fussilage : PlaneComponent 
{
	public Transform ForcePos;
	public GameObject emitter;
	void FixedUpdate()
	{
		if (alive)
		{
			if (plane != null)
			{
				Vector3 push = plane.transform.up * (Mathf.Abs(plane.GetGravity())*0.2f);
				push = new Vector3(0.0f, push.y, 0.0f);
				plane.GetRigidbody().AddForce(push, ForceMode.Force);
			}
		}
	}
	protected override void SetStartingHP()
	{
		hp = 500;
		weight = 2.4f;
	}
	protected override void TakeDamage(int value)
	{
		bool tick = false;
		if (hp > 250)
		{
		 tick = true;
		}
		base.TakeDamage(value);
		if (emitter != null)
		{
			if (tick && hp <= 250)
			{
				emitter.GetComponent<ParticleSystem>().Play();
			}
		}
	}
}
