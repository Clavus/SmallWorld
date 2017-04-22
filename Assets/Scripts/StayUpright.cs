using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class StayUpright : MonoBehaviour
{
    public float anglePerSecond = 180;

	void Update ()
    {
        var angles = transform.rotation.eulerAngles;
        angles.x = Mathf.MoveTowardsAngle(angles.x, 0, Time.deltaTime * anglePerSecond);
        angles.z = Mathf.MoveTowardsAngle(angles.z, 0, Time.deltaTime * anglePerSecond);
        transform.rotation = Quaternion.Euler(angles);
	}
}
