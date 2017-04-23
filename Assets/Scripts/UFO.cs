using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour, IDamagable
{
    public float hoverHeight = 40f;
    public float health = 100;

    [SerializeField] private GameObject explosionPrefab;

    private Rigidbody body;
    private Vector3 cachedAngVelocity = Vector3.zero;

    private Vector3 engineForce = Vector3.zero;
    private Building target = null;

    private static Building[] targets = null;
    public static List<UFO> activeUFOs = new List<UFO>();

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        cachedAngVelocity.y = Mathf.PI / 4f;

        if (targets == null)
            targets = FindObjectsOfType<Building>();
    }

    private void OnEnable()
    {
        activeUFOs.Add(this);
    }

    void OnDisable()
    {
        activeUFOs.Remove(this);
    }

    void Start()
    {
        PickTarget();
        StartCoroutine(FlyControl());
        StartCoroutine(TargetSeek());
    }

    void FixedUpdate()
    {
        body.angularVelocity = cachedAngVelocity;
        body.AddForce(engineForce, ForceMode.Acceleration);
    }

    void PickTarget()
    {
        target = targets[Random.Range(0, targets.Length)];
    }

    IEnumerator FlyControl()
    {
        while (true)
        {
            if (body.velocity.y < -1)
                engineForce.y = body.velocity.y * -3;
            else
                engineForce.y = 0;

            if (body.position.y < hoverHeight)
                engineForce.y = Mathf.Max(engineForce.y, 20);

            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator TargetSeek()
    {
        while (true)
        {
            if (target != null)
            {
                var dir = (target.transform.position - transform.position);
                dir.y = 0;
                dir.Normalize();

                engineForce.x = dir.x * 3;
                engineForce.z = dir.z * 3;
            }
            else
            {
                engineForce.x = Mathf.Max(0, body.velocity.x - 10) * -1;
                engineForce.z = Mathf.Max(0, body.velocity.z - 10) * -1;
            }

            yield return new WaitForSeconds(3 + 2 * Random.value);
        }
    }

    public void Damage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            engineForce = Vector3.zero;
            StopAllCoroutines();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (health <= 0)
        {
            int geometry = LayerMask.NameToLayer("Geometry");
            int layer = collision.collider.gameObject.layer;
            if ((layer & geometry) > 0)
                StartCoroutine(Explode(2));
        }
    }

    void OnWaterEnter()
    {
        StartCoroutine(Explode(0));
    }

    IEnumerator Explode(float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}