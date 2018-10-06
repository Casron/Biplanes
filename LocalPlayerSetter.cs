using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerSetter : MonoBehaviour 
{
	public GameObject mainCamera;

	void Awake()
	{
		GameObject mCam = GameObject.FindGameObjectWithTag("MainCamera"); 
		if (mCam != null)
		{
			Destroy(mCam);
		}
		gameObject.GetComponent<Plane>().SetPlayer();
		GameObject g = (GameObject)Instantiate(mainCamera, transform.position, transform.rotation);
		g.transform.parent = transform;
		g.transform.localPosition = new Vector3(0.0f,2.8f,-11.0f);
		g.transform.localEulerAngles = new Vector3(0.0f,0.0f,0.0f);
	}
	void Start()
	{
		GameObject.FindGameObjectWithTag("Engine").GetComponent<Engine>().LocalPlayerInit(gameObject);
	}
}
