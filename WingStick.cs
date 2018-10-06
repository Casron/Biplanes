using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingStick : PlaneComponent
{

	protected override void SetStartingHP()
	{
		hp = 15;
		weight = 0.4f;
	}
}
