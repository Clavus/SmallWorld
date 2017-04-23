using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VRTK;

public class Lemming : VRTK_InteractableObject, IDamagable
{
    [Header("Lemming")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Collider collider;
    [SerializeField] Behaviour startBehaviour;
    [SerializeField] GameObject[] lemmingBodyTypes;

    [Range(0, 100)]
    public float maxHealth = 100;
    [Range(0, 100)]
    public float health = 100;
    
    public WanderSettings wanderSettings;
    public GrabSettings grabSettings;
    public FallSettings fallSettings;
    public WaterSettings waterSettings;

    private readonly int PARAM_SPEED = Animator.StringToHash("Speed_f");
    private readonly int PARAM_GROUNDED = Animator.StringToHash("Grounded");
    private readonly int PARAM_ANIMATION = Animator.StringToHash("Animation_int");
    private readonly int PARAM_DEATH = Animator.StringToHash("Death_b");
    private readonly int PARAM_DEATHTYPE = Animator.StringToHash("DeathType_int");
    private readonly int PARAM_CROUCH = Animator.StringToHash("Crouch_b");
    private readonly int PARAM_JUMP = Animator.StringToHash("Jump_b");
    private readonly int PARAM_HEAD_H = Animator.StringToHash("Head_Horizontal_f");
    private readonly int PARAM_HEAD_V = Animator.StringToHash("Head_Vertical_f");
    private readonly int PARAM_BODY_H = Animator.StringToHash("Body_Horizontal_f");
    private readonly int PARAM_BODY_V = Animator.StringToHash("Body_Vertical_f");

    private Behaviour currentBehaviour;
    private Vector3 lastPosition;
    private IEnumerator activeRoutine = null;
    private const float animatorUpdateInterval = 0.1f;
    private float bodyDrag = 0;
    private float waterLevel = 0;
    private int geometryLayer;
    private int waterLayer;
    private MissileLauncher mountedTurret;
    private bool hideHealthbarAfterTouch = true;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public enum Behaviour
    {
        Wander, Grabbed, Falling, Turret, Dead, Drowning
    }
    
	void Start ()
    {
        geometryLayer = LayerMask.NameToLayer("Geometry");
        waterLayer = LayerMask.NameToLayer("Water");
        SetBehaviour(startBehaviour);
        StartCoroutine(AnimatorUpdate());

        healthbar.SetHealth(1);
        healthbar.Hide();

        bodyDrag = body.drag;

        // pick random lemming body
        int keep = Random.Range(0, lemmingBodyTypes.Length);
        for(int i = 0; i < lemmingBodyTypes.Length; i++)
        {
            if (i != keep)
                Destroy(lemmingBodyTypes[i]);
            else
                lemmingBodyTypes[i].SetActive(true);
        }

    }

    void FixedUpdate()
    {
        if (currentBehaviour == Behaviour.Drowning)
        {
            float diff = waterLevel - transform.position.y;
            if (diff > 0)
                body.AddForce(Vector3.up * diff * 8, ForceMode.Acceleration);
        }
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        base.StartTouching(currentTouchingObject);

        if (currentBehaviour != Behaviour.Dead)
            healthbar.Show();
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);

        if (currentBehaviour != Behaviour.Dead && hideHealthbarAfterTouch)
            healthbar.Hide();
    }

    public override void Grabbed(GameObject grabbingObject)
    {
        base.Grabbed(grabbingObject);
        SetBehaviour(Behaviour.Grabbed);
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);

        foreach (MonoBehaviour m in grabSettings.enableOnDrop)
            m.enabled = true;

        Damage(25);
        SetBehaviour(health > 0 ? Behaviour.Falling : Behaviour.Dead);
    }

    public void Damage(float amount)
    {
        if (currentBehaviour == Behaviour.Dead)
            return;

        health = Mathf.Clamp(health - amount, 0, maxHealth);
        healthbar.SetHealth(health / maxHealth);
        healthbar.Show();
        StartCoroutine(HideHealthbarAfterSeconds(3));

        if (health <= 0)
            SetBehaviour(Behaviour.Dead);
    }

    public void Heal(float amount)
    {
        if (health >= maxHealth)
            return;

        health = Mathf.Clamp(health + amount, 0, maxHealth);

        healthbar.SetHealth(health / maxHealth);
        healthbar.Show();
        StartCoroutine(HideHealthbarAfterSeconds(2));

        if (health >= maxHealth && currentBehaviour == Behaviour.Dead)
        {
            animator.SetBool(PARAM_DEATH, false);
            SetBehaviour(Behaviour.Wander);
        }
    }

    IEnumerator HideHealthbarAfterSeconds(float secs)
    {
        hideHealthbarAfterTouch = false;
        yield return new WaitForSeconds(secs);
        healthbar.Hide();
        hideHealthbarAfterTouch = true;
    }

    public void SetBehaviour(Behaviour behaviour)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = null;
        currentBehaviour = behaviour;
        animator.SetInteger(PARAM_ANIMATION, 0);

        if (behaviour != Behaviour.Turret && mountedTurret != null)
        {
            mountedTurret.Dismount(this);
            mountedTurret = null;
        }

        body.drag = bodyDrag;

        switch (behaviour)
        {
            case Behaviour.Wander:
                agent.enabled = true;
                activeRoutine = WanderBehaviour();
                break;
            case Behaviour.Grabbed:
                agent.enabled = false;
                foreach (MonoBehaviour m in grabSettings.disableOnGrab)
                    m.enabled = false;
                break;
            case Behaviour.Falling:
                agent.enabled = false;
                animator.SetBool(PARAM_GROUNDED, false);
                foreach (MonoBehaviour m in fallSettings.disableOnFall)
                    m.enabled = false;
                break;
            case Behaviour.Turret:
                agent.SetDestination(mountedTurret.mountLocation.position);
                animator.SetInteger(PARAM_ANIMATION, 1);
                break;
            case Behaviour.Drowning:
                agent.enabled = false;
                body.drag = waterSettings.waterDrag;
                animator.SetBool(PARAM_GROUNDED, false);
                foreach (MonoBehaviour m in fallSettings.enableOnLand)
                    m.enabled = true;
                break;
            case Behaviour.Dead:
                agent.enabled = false;
                animator.SetInteger(PARAM_DEATHTYPE, Random.Range(1, 3));
                animator.SetBool(PARAM_DEATH, true);
                break;
        }

        if (activeRoutine != null)
            StartCoroutine(activeRoutine);
    }

    public bool CanMount()
    {
        return currentBehaviour == Behaviour.Wander;
    }

    public void Mount(MissileLauncher launcher)
    {
        mountedTurret = launcher;
        SetBehaviour(Behaviour.Turret);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsGrabbed())
            return;

        int layer = collision.gameObject.layer;
        if ((currentBehaviour == Behaviour.Falling || currentBehaviour == Behaviour.Dead) && (layer & geometryLayer) > 0 && Vector3.Dot(collision.contacts[0].normal, Vector3.up) > 0.5f )
        {
            foreach (MonoBehaviour m in fallSettings.enableOnLand)
                m.enabled = true;

            animator.SetBool(PARAM_GROUNDED, true);

            if (currentBehaviour == Behaviour.Falling)
                SetBehaviour(Behaviour.Wander);
        }
        
    }

    void OnWaterEnter()
    {
        if (IsGrabbed())
            return;

        if (currentBehaviour == Behaviour.Dead)
        {
            collider.enabled = false; // dead person fell into the water, is now lost
            StartCoroutine(RemoveMeAfterDelay(5));
        }
        else
        {
            foreach (MonoBehaviour m in waterSettings.enableOnWater)
                m.enabled = true;

            foreach (MonoBehaviour m in waterSettings.disableOnWater)
                m.enabled = false;

            waterLevel = transform.position.y;
            SetBehaviour(Behaviour.Drowning);
        }
    }

    void OnWaterLeave()
    {
        
    }

    IEnumerator RemoveMeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    IEnumerator AnimatorUpdate()
    {
        lastPosition = transform.position;

        while(true)
        {
            animator.SetFloat(PARAM_SPEED, Vector3.Distance(lastPosition, transform.position) / animatorUpdateInterval);
            lastPosition = transform.position;
            yield return new WaitForSeconds(animatorUpdateInterval);
        }
    }

    IEnumerator WanderBehaviour()
    {
        while(true)
        {
            float range = wanderSettings.wanderRange.x + Random.value * (wanderSettings.wanderRange.y - wanderSettings.wanderRange.x);
            float dir = Mathf.PI * 2 * Random.value;
            agent.SetDestination(transform.position + new Vector3(Mathf.Cos(dir) * range, 0, Mathf.Sin(dir) * range));
            yield return new WaitForSeconds(wanderSettings.interval.x + Random.value * (wanderSettings.interval.y - wanderSettings.interval.x));
        }
    }

}

[System.Serializable]
public class WanderSettings
{
    public Vector2 interval = new Vector2(3, 5);
    public Vector2 wanderRange = new Vector2(2, 10);
}

[System.Serializable]
public class GrabSettings
{
    public MonoBehaviour[] disableOnGrab;
    public MonoBehaviour[] enableOnDrop;
}

[System.Serializable]
public class FallSettings
{
    public MonoBehaviour[] disableOnFall;
    public MonoBehaviour[] enableOnLand;
}

[System.Serializable]
public class WaterSettings
{
    public float waterDrag = 5f;
    public MonoBehaviour[] disableOnWater;
    public MonoBehaviour[] enableOnWater;
}