using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWing : PlaneComponent
{
	void FixedUpdate()
	{
		if (alive)
		{
			if (plane != null)
			{

				GameObject pp = plane.GetPropeller();
				if (pp != null)
				{
					Vector3 push = transform.up * (Mathf.Abs(plane.GetGravity())*0.14f);
					push = new Vector3(0.0f, push.y, 0.0f);
					Propeller p = pp.GetComponent<Propeller>();
					Vector3 thrust = transform.forward * p.thrust/2.0f;
					thrust = new Vector3(thrust.x, thrust.y / 5.0f, thrust.z);
					push += thrust;
					plane.GetRigidbody().AddForce(push, ForceMode.Force);

					float normalizedXAngles = transform.localEulerAngles.x;
					if (normalizedXAngles >= 270.0f)
					{
						normalizedXAngles -= 360.0f;
					}
					plane.GetRigidbody().AddTorque(transform.right * normalizedXAngles * thrust.magnitude/1000.0f);
					plane.GetRigidbody().AddTorque(transform.forward * transform.localPosition.x);
					
				}
			}
		}
	}
	protected override void SetStartingHP()
	{
		hp = 125;
		weight = 0.25f;	
	}
}
