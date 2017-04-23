using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurface : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
            other.attachedRigidbody.gameObject.SendMessage("OnWaterEnter");
    }

    private void OnTriggerLeave(Collider other)
    {
        if (other.attachedRigidbody != null)
            other.attachedRigidbody.gameObject.SendMessage("OnWaterLeave");
    }
}
