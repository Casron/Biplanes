using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTop : PlaneComponent
{
	void FixedUpdate()
	{
		if (alive)
		{
			if (plane != null)
			{
				Vector3 push = transform.up * (Mathf.Abs(plane.GetGravity())*0.4f);
				push = new Vector3(0.0f, push.y, 0.0f);
				plane.GetRigidbody().AddForce(push, ForceMode.Force);
			}
		}
	}
	protected override void SetStartingHP()
	{
		hp = 250;
		weight = 0.2f;
	}
}
