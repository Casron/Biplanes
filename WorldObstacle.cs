using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObstacle : MonoBehaviour 
{

	void OnCollisionEnter(Collision hit)
	{
		PlaneComponent pC = hit.collider.gameObject.GetComponent<PlaneComponent>(); 
		if (pC != null)
		{
			pC.DoDamage(1000);
		}
	}
}
