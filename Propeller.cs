using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : PlaneComponent
{

	public GameObject emitter;
	const float speed = 1440.0f;
	public float thrust = 200.0f;
	public AudioSource audioSource;

	void FixedUpdate()
	{
		if (alive)
		{
			transform.Rotate(Vector3.forward, speed * Time.deltaTime);

		}
	}
	protected override void Die()
	{
		base.Die();
		audioSource.Stop();
	}
	protected override void SetStartingHP()
	{
		hp = 200;
		weight = 0.4f;
	}
	protected override void TakeDamage(int value)
	{
		bool tick = false;
		if (hp > 100)
		{
		 tick = true;
		}
		base.TakeDamage(value);
		if (emitter != null)
		{
			if (tick && hp <= 100)
			{
				emitter.GetComponent<ParticleSystem>().Play();
				thrust = thrust * .8f;
			}
		}
	}
}
