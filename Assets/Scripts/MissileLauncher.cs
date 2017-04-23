using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] private bool isActive = false;
    [SerializeField] private Transform turret;
    [SerializeField] private Missile[] missiles;
    public Transform mountLocation;
    public float rotateSpeed = 90f;

    private Lemming controller;
    private bool hasTarget = false;

    private void Reset()
    {
        turret = transform.Find("turret");
    }

    void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void Dismount(Lemming dismounter)
    {
        if (dismounter == controller)
        {
            SetActive(false);
            controller = null;
        }
    }

    private void SetActive(bool b)
    {
        if (isActive != b)
        {
            isActive = b;
            if (b)
            {
                StartCoroutine(ActiveLoop());
                StartCoroutine(TargetSeek());
            }
            else
            {
                StopAllCoroutines();
            }
        }
    }

    IEnumerator ActiveLoop()
    {
        while(true)
        {
            if (!hasTarget)
            {
                Vector3 newAngle = new Vector3(-60 * Random.value, 360 * Random.value, 0);
                float diff = Mathf.DeltaAngle(newAngle.y, turret.transform.eulerAngles.y);
                turret.DORotate(newAngle, Mathf.Abs(diff) / rotateSpeed);
            }
            yield return new WaitForSeconds(3 + Random.value);
        }
    }

    IEnumerator TargetSeek()
    {
        while (true)
        {
            UFO closest = null;
            float targetDist = 200;
            Vector3 lookDirection = Vector3.zero;
            foreach(UFO ufo in UFO.activeUFOs)
            {
                Vector3 lookDir = ufo.transform.position - transform.position;
                if (lookDir.sqrMagnitude < targetDist * targetDist)
                {
                    closest = ufo;
                    targetDist = lookDir.magnitude;
                    lookDirection = lookDir;
                }
            }

            if (closest != null)
            {
                Vector3 newAngle = Quaternion.LookRotation(lookDirection.normalized, Vector3.up).eulerAngles;
                if (newAngle.x > 270 && newAngle.x < 360) // target is above
                {
                    hasTarget = true;
                    turret.DOKill();
                    float diff = Mathf.DeltaAngle(newAngle.y, turret.transform.eulerAngles.y);
                    float duration = Mathf.Abs(diff) / rotateSpeed;
                    turret.DORotate(newAngle, duration);
                    yield return new WaitForSeconds(duration + 0.5f);
                    if (closest != null)
                        FireMissile(closest);
                    yield return new WaitForSeconds(2);
                    hasTarget = false;
                }
            }
            
            yield return new WaitForSeconds(3 + Random.value);
        }
    }

    private void FireMissile(UFO closest)
    {
        foreach (Missile m in missiles)
        {
            if (!m.IsFired())
            {
                m.Fire(closest.transform);
                break;
            }
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (controller == null && collider.attachedRigidbody != null)
        {
            Lemming lemming = collider.attachedRigidbody.GetComponent<Lemming>();
            if (lemming != null && lemming.CanMount())
            {
                controller = lemming;
                controller.Mount(this);
                SetActive(true);
            }
        }
    }
}
