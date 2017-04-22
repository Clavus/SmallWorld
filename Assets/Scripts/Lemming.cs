using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VRTK;

public class Lemming : VRTK_InteractableObject
{
    [Header("Lemming")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] Behaviour startBehaviour;
    [SerializeField] GameObject[] lemmingBodyTypes;

    public WanderSettings wanderSettings;
    public GrabSettings grabSettings;
    public FallSettings fallSettings;

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
    private int geometryLayer;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public enum Behaviour
    {
        Wander, Grabbed, Falling
    }
    
	void Start ()
    {
        geometryLayer = LayerMask.NameToLayer("Geometry");
        SetBehaviour(startBehaviour);
        StartCoroutine(AnimatorUpdate());

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
        
        SetBehaviour(Behaviour.Falling);
    }

    public void SetBehaviour(Behaviour behaviour)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = null;
        currentBehaviour = behaviour;

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
        }

        if (activeRoutine != null)
            StartCoroutine(activeRoutine);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentBehaviour == Behaviour.Falling && (collision.gameObject.layer & geometryLayer) > 0)
        {
            foreach (MonoBehaviour m in fallSettings.enableOnLand)
                m.enabled = true;

            animator.SetBool(PARAM_GROUNDED, true);
            SetBehaviour(Behaviour.Wander);
        }
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