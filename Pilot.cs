using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pilot : PlaneComponent 
{
	float mPivotY;
	float mPivotX;
	float queuedYaw = 0.0f;
	float queuedHor = 0.0f;
    public Animator anim;
    public GameObject emitter;
    protected override void SetStartingHP()
    {
        hp = 1;
    }
    protected override void Die()
	{
		if (alive)
		{
			alive = false;
            anim.SetBool("Dead",true);
            if (plane != null && plane.IsLocalPlayer())
            {
                plane.Kill("Pilot Death");
            }
		}
	}
    void FixedUpdate()
    {
        if (plane != null && alive)
        {
            if (plane.IsLocalPlayer())
            {
                float yaw = Input.GetAxis("Mouse Y");
                float hor = Input.GetAxis("Mouse X");
                queuedYaw += yaw;
                queuedYaw = Mathf.Clamp(queuedYaw, -30.0f, 30.0f);

                queuedHor += hor;
                queuedHor = Mathf.Clamp(queuedHor,-20.0f,20.0f);


                if (plane.leftWing != null)
                {
                    plane.leftWing.transform.localEulerAngles = new Vector3(queuedYaw * 2.0f - (queuedHor), 0.0f,0.0f);
                }
                if (plane.rightWing != null)
                {
                    plane.rightWing.transform.localEulerAngles= new Vector3(queuedYaw * 2.0f + (queuedHor), 0.0f, 0.0f);
                }

                plane.transform.localEulerAngles = new Vector3(plane.transform.eulerAngles.x, plane.transform.eulerAngles.y, queuedHor * -3.0f);
                plane.transform.Rotate(Vector3.up, queuedHor * Time.deltaTime);
                if (Input.GetMouseButton(0))
                {
                    plane.FireGuns();
                }
            }
        }
    }
    protected override void TakeDamage(int value)
    {
        if (alive)
        {
                if (plane.leftWing != null)
                {
                    plane.leftWing.transform.localEulerAngles = new Vector3(20.0f, 0.0f,0.0f);
                }
                if (plane.rightWing != null)
                {
                    plane.rightWing.transform.localEulerAngles= new Vector3(20.0f, 0.0f, 0.0f);
                }   
        }
        emitter.GetComponent<ParticleSystem>().Play();
        base.TakeDamage(value);
    }
}
