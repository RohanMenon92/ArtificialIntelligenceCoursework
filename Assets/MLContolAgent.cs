using DG.Tweening;
using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLContolAgent : Agent, IPlayerStats
{
    GameObject target = null;
    public string tagToTarget = "FSMPlayer";
    public float health { get; set; }
    public float speed { get; set; }

    public bool isReloading { get; set; }
    public bool isBlocking { get; set; }

    public float turnSpeed { get; set; }

    public float rotateSpeed = 200;
    public float moveSpeed = 0.0001f;

    public GameObject bulletPrefab;
    public Transform firePos;

    Vector3 shieldRestPos = new Vector3(0f, 0.6f, 0.75f);

    Sequence attackSequence;
    Sequence defenseSequence;

    Rigidbody playerRigidBody;

    public Transform gun;
    public Transform shield;

    Vector3 resetFSMPosition = new Vector3(-39.6f, -0.5f, 56.5f);
    Vector3 resetMLPosition = new Vector3(-8.8f, -0.5f, 41f);

    RayPerception m_RayPer;

    float[] m_RayAngles = { -40f, -20f, 0f, 20f, 40f, 60f, 80f, 100f, 120f, 140f, 160f, 180f, 200f, 220f };
    string[] detectableObjects = { "Bullet", "FSMPlayer", "Player", "wall" };
    // Start is called before the first frame update


    // Check decision bools to have smooth input
    bool onDecisionForward = false;
    bool onDecisionBack = false;
    bool onDecisionLeft = false;
    bool onDecisionRight = false;
    bool onDecisionUnBlock = false;
    bool onDecisionBlock = false;
    bool onDecisionAttack = false;

    void Start()
    {
        //StartCoroutine(CheckDamageAndDecideToFail());
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        m_RayPer = GetComponent<RayPerception>();

        health = 50;
        speed = moveSpeed;
        turnSpeed = rotateSpeed;
        playerRigidBody = GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        // Only reset sibling FSM AI
        FSMControlAgent fsmControlAgent = transform.parent.GetComponentInChildren<FSMControlAgent>();

        // Randomize Spawn locations
        fsmControlAgent.transform.localPosition = new Vector3(resetFSMPosition.x - Random.Range(-2f, 2f), resetFSMPosition.y, resetFSMPosition.z - Random.Range(-2f, 2f));
        transform.localPosition = new Vector3(resetMLPosition.x - Random.Range(-2f, 2f), resetMLPosition.y, resetMLPosition.z - Random.Range(-2f, 2f));

        // ResetHealth
        health = 100;
        fsmControlAgent.health = 100;

        // Kill animations and callbacks
        if (isBlocking)
        {
            OnUnBlock();
            defenseSequence.Kill(true);
        }

        if (isReloading)
        {
            attackSequence.Kill(true);
        }

        if (fsmControlAgent.isBlocking)
        {
            OnUnBlock();
            fsmControlAgent.defenseSequence.Kill(true);
        }

        if (fsmControlAgent.isReloading)
        {
            fsmControlAgent.attackSequence.Kill(true);
        }


        foreach (BulletScript bullet in GameObject.FindObjectsOfType<BulletScript>())
        {
            Destroy(bullet.gameObject);
        }

        //StartCoroutine(CheckDamageAndDecideToFail());
    }

    //IEnumerator CheckDamageAndDecideToFail()
    //{
    //    yield return new WaitForSeconds(10f);
    //    // End Experiment with faliure because no one got damaged
    //    FSMControlAgent fsmControlAgent = transform.parent.GetComponentInChildren<FSMControlAgent>();

    //    if (fsmControlAgent.health == 50)
    //    {
    //        Debug.Log("Failure case no one lost health at 15 seconds");
    //        AddReward(-75f);
    //        Done();
    //    }
    //}

    public override void CollectObservations()
    {
        float rayDistance = 20f;

        //Add self Observations
        AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 1f, 0f));
        AddVectorObs(health);
        AddVectorObs(isReloading);
        AddVectorObs(isBlocking);
        AddVectorObs(new Vector3(transform.localPosition.x, -0.5f, transform.localPosition.z));
        AddVectorObs(transform.localRotation.eulerAngles.y);
        AddVectorObs(playerRigidBody.velocity.x);
        AddVectorObs(playerRigidBody.velocity.z);

        //If targeting, Add targeting or add empty values
        bool isTargetNull = target != null;
        AddVectorObs(isTargetNull);

        if (isTargetNull)
        {
            IPlayerStats targetIPlayerStats = target.GetComponent<IPlayerStats>();

            AddVectorObs(targetIPlayerStats.health);
            AddVectorObs(targetIPlayerStats.isReloading);
            AddVectorObs(targetIPlayerStats.isBlocking);
            AddVectorObs(Vector3.Distance(target.transform.localPosition, transform.localPosition));
            AddVectorObs(new Vector3(target.transform.localPosition.x, -0.5f, target.transform.localPosition.z));
            AddVectorObs(target.transform.localRotation.y);
            AddVectorObs(target.GetComponent<Rigidbody>().velocity.x);
            AddVectorObs(target.GetComponent<Rigidbody>().velocity.z);
        }
        else
        {
            AddVectorObs(0f);
            AddVectorObs(false);
            AddVectorObs(false);
            AddVectorObs(0f);
            AddVectorObs(Vector3.zero);
            AddVectorObs(Quaternion.identity);
            AddVectorObs(0f);
            AddVectorObs(0f);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float upDownSignal = Mathf.FloorToInt(vectorAction[0]);
        float leftRightSignal = Mathf.FloorToInt(vectorAction[1]);
        float blockAction = Mathf.FloorToInt(vectorAction[2]);
        float attackAction = Mathf.FloorToInt(vectorAction[3]);

        // Set up bool values so that fixed update can decide whenn to do it per frame
        if (upDownSignal >= 0f)
        {
            OnDecisionForward(true);
            OnDecisionBack(false);
        }
        else if (upDownSignal < 0f)
        {
            OnDecisionBack(true);
            OnDecisionForward(false);
        }

        if (leftRightSignal > 0f)
        {
            OnDecisionLeft(true);
            OnDecisionRight(false);
        }
        else if (leftRightSignal < 0f)
        {
            OnDecisionLeft(false);
            OnDecisionRight(true);
        } else if (leftRightSignal == 0f) {
            OnDecisionLeft(false);
            OnDecisionRight(false);
        }

        if (blockAction > 0.5f)
        {
            if (!isBlocking)
            {
                OnDecisionBlock(true);
                OnDecisionUnBlock(false);
            }
        }
        else
        {
            if (isBlocking)
            {
                OnDecisionBlock(false);
                OnDecisionUnBlock(true);
            }
        }

        if (attackAction > 0.5f)
        {
            OnDecisionAttack(true);
        } else
        {
            OnDecisionAttack(false);
        }
    }

    void OnDecisionAttack(bool value)
    {
        onDecisionAttack = value;
    }
    void OnDecisionBlock(bool value)
    {
        onDecisionBlock = value;
    }
    void OnDecisionUnBlock(bool value)
    {
        onDecisionUnBlock = value;
    }
    void OnDecisionLeft(bool value)
    {
        onDecisionLeft = value;
    }
    void OnDecisionRight(bool value)
    {
        onDecisionRight = value;
    }
    void OnDecisionForward(bool value)
    {
        onDecisionForward = value;
    }
    void OnDecisionBack(bool value)
    {
        onDecisionBack = value;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check decision bools to have smooth input
        if (onDecisionForward)
        {
            AddReward(0.001f);
            OnForward();
        }
        else if (onDecisionBack)
        {
            AddReward(0.0001f);
            OnBack();
        }

        if (onDecisionLeft)
        {
            OnLeft();
        }
        else if (onDecisionRight)
        {
            OnRight();
        }

        if (onDecisionUnBlock)
        {
            OnUnBlock();
        }
        else if (onDecisionBlock)
        {
            OnBlock();
        }

        if(onDecisionAttack)
        {
            OnAttack();
        }

        // If objects glitch, end simulation
        if (transform.localPosition.y < -40f || transform.localPosition.z < -8.5f || transform.localPosition.z > 96.5f || transform.localPosition.x < -75f || transform.localPosition.x > 28f)
        {
            Debug.Log("Out of Bounds Falling off screen");
            AddReward(-100f);
            OnDeath();
        }
    }
    void OnForward()
    {
        Vector3 dirToGo = transform.forward;
        playerRigidBody.AddForce(dirToGo * moveSpeed * (isBlocking ? 0.5f : 1f), ForceMode.VelocityChange);

        float forwardLimit = isBlocking ? 1f : 5f;

        if (playerRigidBody.velocity.magnitude > forwardLimit)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * forwardLimit;
        }
    }
    void OnBack()
    {
        Vector3 dirToGo = -transform.forward;
        playerRigidBody.AddForce(dirToGo * moveSpeed * (isBlocking ? 0.5f : 1f), ForceMode.VelocityChange);

        float reverseLimit = isBlocking ? 1f : 2f;

        if (playerRigidBody.velocity.magnitude > reverseLimit)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * reverseLimit;
        }
    }
    void OnLeft()
    {
        Vector3 dirToRotate = -transform.up;
        transform.Rotate(dirToRotate, Time.fixedDeltaTime * turnSpeed * (isBlocking ? 0.5f : 1f));
    }
    void OnRight()
    {
        Vector3 dirToRotate = transform.up;
        transform.Rotate(dirToRotate, Time.fixedDeltaTime * turnSpeed * (isBlocking ? 0.5f : 1f));
    }

    public void OnAttack()
    {
        if (!isReloading && !isBlocking)
        {
            isReloading = true;
            AttackAction();
        } else
        {
            AddReward(-0.0001f);
        }
    }

    // When it's hit by another player, called by script object
    public void OnHit()
    {
        AddReward(-2f);
        this.health -= 10;
        this.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        if (health <= 0f)
        {
            OnDeath();
        }
    }

    // When other player blocks attack
    public void OnBlock()
    {
        if (!isReloading && !isBlocking)
        {
            isBlocking = true;
            BlockAction();
        } else
        {
            AddReward(-0.0001f);
        }
    }

    void AttackAction()
    {
        // Add tiny reward to teach to correctly fire
        FireBullet();
        attackSequence = DOTween.Sequence();
        attackSequence.Insert(0f, gun.DOLocalMove(new Vector3(0f, 0f, -0.4f), 0.2f).SetEase(Ease.InOutBack));
        attackSequence.Insert(0.3f, gun.DOLocalMove(new Vector3(0f, 0f, 0f), 4.7f).SetEase(Ease.InOutQuad));

        attackSequence.OnComplete(AttackComplete);
        attackSequence.Play();
    }

    void FireBullet()
    {
        AddReward(0.1f);

        GameObject instantiatedBullet = GameObject.Instantiate(bulletPrefab, firePos.position, transform.localRotation);
        instantiatedBullet.GetComponent<BulletScript>().firedFrom = this;
    }

    void AttackComplete()
    {
        isReloading = false;
    }

    void BlockAction()
    {
        // Add tiny reward to teach to correctly block
        AddReward(0.001f);
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, 0f, 1f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(60f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 0.5f), 0.25f).SetEase(Ease.OutBack));
        // TO DO
        // MoveShield To Position
    }

    void OnUnBlock()
    {
        if(!isBlocking)
        {
            AddReward(-0.0001f);
            return;
        }
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalMove(shieldRestPos, 0.25f).SetEase(Ease.OutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 1f), 0.25f).SetEase(Ease.OutBack));

        defenseSequence.OnComplete(BlockComplete);
    }

    public void BlockComplete()
    {
        isBlocking = false;
    }

    public void OnSuccessHit()
    {
        AddReward(25f);
        Debug.Log(this.gameObject.name + " successfully hit");
    }

    public void OnDeath()
    {
        AddReward(-100f);
        Done();
        Debug.Log(this.gameObject.name + " isDead");
    }

    public void OnSuccessfulBlock()
    {
        AddReward(10f);
        Debug.Log(this.gameObject.name + " successfully blocked a shot");
    }
    public void OnShieldedHit()
    {
        AddReward(5f);
        Debug.Log(this.gameObject.name + " shot hit was blocked by a shield");
    }

    public void OnInvestigateAreaEnter(GameObject targetInArea)
    {
        target = targetInArea;
    }
    public void SetTarget(GameObject targetInArea)
    {
        AddReward(0.001f);
        if (target != targetInArea)
        {
            target = targetInArea;
        }
    }
    public void OnInvestigateAreaExit(GameObject targetInArea)
    {
        if (target == targetInArea)
        {
            AddReward(-1f);
            target = null;
        }
    }

    public void OnEnemyDeath()
    {
        AddReward(200f);
        Done();
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Prevent it from touching walls for too long
        if(collision.gameObject.tag == "wall")
        {
            AddReward(-1f);
        }
    }
}
