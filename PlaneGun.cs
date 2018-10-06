using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneGun : PlaneComponent
{
	public LineRenderer lR;
	public AudioSource audioSource;
	public GameObject bulletHolePrefab;
	bool HasAmmo = true;
	Vector3 hitPos;
	bool shotInProgress = false;
	void Update()
	{
		if (shotInProgress)
		{
			FireLine();
		}
	}
	public bool GetAmmo()
	{
		return HasAmmo;
	}
	public void Shoot()
	{
		if (HasAmmo && plane != null)
		{
			HasAmmo = false;
			Invoke("Reload",0.25f);
			FireLine();
			shotInProgress = true;
			Invoke("ResetLine",0.125f);
			audioSource.Play();
			Ray r = new Ray(transform.position, plane.transform.forward * 1500.0f);
			RaycastHit[] hits = Physics.RaycastAll(r,1500);
			GameObject closestObj = null;
			float distance = 1600.0f;
			hitPos = plane.transform.forward * 500.0f + transform.position;
			for(int i = 0 ; i < hits.Length; i++)
			{
				PlaneComponent pC = hits[i].collider.gameObject.GetComponent<PlaneComponent>();
				if (pC != null)
				{
					if (!pC.IsSamePlane(plane))
					{
						float compare = Vector3.Distance(transform.position, hits[i].collider.gameObject.transform.position);
						if (compare < distance)
						{
							distance = compare;
							closestObj = hits[i].collider.gameObject;
							hitPos = hits[i].point;
						}
					}
				}
				else
				{
					float compare = Vector3.Distance(transform.position, hits[i].collider.gameObject.transform.position);
					if (compare < distance)
					{
						distance = compare;
						closestObj = hits[i].collider.gameObject;
						hitPos = hits[i].point;
					}	
				}
			}
			if (closestObj != null)
			{
				PlaneComponent pC = closestObj.GetComponent<PlaneComponent>();
				if (pC != null)
				{
					GameObject g = Instantiate(bulletHolePrefab,hitPos, transform.rotation);
					g.transform.LookAt(transform.position, Vector3.Cross(hitPos, transform.position));
					g.transform.SetParent(closestObj.transform);
					int damage = 150 - (int)(Mathf.Floor(125.0f * (distance/1500)));
					pC.SetLastHit(plane.GetPlayerName());
					pC.DoDamage(damage);
				}
			}
		}
	}
	void Reload()
	{
		HasAmmo = true;
	}
	void ResetLine()
	{
		Vector3[] set = new Vector3[]{transform.position, transform.position};
		lR.SetPositions(set);
		shotInProgress = false;
	}
	void FireLine()
	{
		Vector3[] set = new Vector3[]{transform.position, hitPos};
		lR.SetPositions(set);
	}
}
