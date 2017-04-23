using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Transform target;
    public float maxSpeed = 30;
    public float acceleration = 20;
    public float turnSpeed = 45;

    [SerializeField] private Collider collider;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject explosionPrefab;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool isFired = false;
    private float speed = 0;
    private bool canTurn = false;

	void Start ()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalParent = transform.parent;
        trail.enabled = false;
        collider.enabled = false;
    }
	
	void Update ()
    {
		
	}

    public void Fire(Transform target)
    {
        this.target = target;
        trail.Clear();
        trail.enabled = true;
        isFired = true;
        canTurn = false;
        StartCoroutine(SeekTarget());
        StartCoroutine(StartupSequence());
    }

    public bool IsFired()
    {
        return isFired;
    }

    public void ResetMissile()
    {
        speed = 0;
        trail.enabled = false;
        collider.enabled = false;
        isFired = false;
        canTurn = false;
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        StopAllCoroutines();
    }

    IEnumerator SeekTarget()
    {
        while(true)
        {
            if (target == null)
                ResetMissile();
            else
            {
                var from = transform.rotation;
                var to = transform.rotation;
                if (canTurn)
                    to = Quaternion.LookRotation((target.transform.position - transform.position).normalized);

                var q = Quaternion.RotateTowards(from, to, turnSpeed * Time.deltaTime);
                transform.SetPositionAndRotation(transform.position + (transform.forward * speed * Time.deltaTime), q);

                if (Quaternion.Angle(from, to) < 20)
                    speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);
                else if (speed < maxSpeed / 2)
                    speed = Mathf.MoveTowards(speed, maxSpeed / 2, acceleration * Time.deltaTime);
            }
            yield return null;
        }
    }

    IEnumerator StartupSequence()
    {
        yield return new WaitForSeconds(1);
        transform.SetParent(null);
        collider.enabled = true;
        canTurn = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isFired)
            return;

        Collider other = collision.collider;
        Instantiate(explosionPrefab, transform.position, transform.rotation);

        IDamagable d = other.GetComponent<IDamagable>();
        if (d == null && other.attachedRigidbody != null)
            d = other.attachedRigidbody.GetComponent<IDamagable>();
        
        if (d != null)
        {
            d.Damage(50);
        }

        ResetMissile();
    }
}
