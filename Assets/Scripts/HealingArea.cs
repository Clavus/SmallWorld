using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingArea : MonoBehaviour
{
    public float healthPerSecond = 10;

    private List<Lemming> patients = new List<Lemming>();

    private void Start()
    {
        StartCoroutine(HealthUpdate());
    }

    IEnumerator HealthUpdate()
    {
        while(true)
        {
            foreach (Lemming patient in patients)
                patient.Heal(healthPerSecond);

            yield return new WaitForSeconds(1);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.attachedRigidbody != null)
        {
            Lemming lemming = collider.attachedRigidbody.GetComponent<Lemming>();
            if (lemming != null && !patients.Contains(lemming))
            {
                patients.Add(lemming);
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.attachedRigidbody != null)
        {
            Lemming lemming = collider.attachedRigidbody.GetComponent<Lemming>();
            if (lemming != null)
            {
                patients.Remove(lemming);
            }
        }
    }

}
