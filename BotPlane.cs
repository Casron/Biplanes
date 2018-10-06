using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPlane : MonoBehaviour {

	void Update()
	{
		gameObject.GetComponent<Plane>().FireGuns();
	}
}
