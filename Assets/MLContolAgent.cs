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

    Vector3 resetFSMPosition = new Vector3(-16f, 0.4f, 11.4f);
    Vector3 resetMLPosition = new Vector3(23.6f, 0.4f, -36f);

    RayPerception m_RayPer;

    float[] m_RayAngles = { -40f, -20f, 0f, 20f, 40f, 60f, 80f, 100f, 120f, 140f, 160f, 180f, 200f, 220f };
    string[] detectableObjects = { "Bullet", "FSMPlayer", "Player" };
    // Start is called before the first frame update
    void Start()
    {
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        m_RayPer = GetComponent<RayPerception>();

        health = 100;
        speed = moveSpeed;
        turnSpeed = rotateSpeed;
        playerRigidBody = GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        GameObject.FindObjectOfType<FSMControlAgent>().transform.position = new Vector3(resetFSMPosition.x - Random.Range(-2f, 2f), resetFSMPosition.y, resetFSMPosition.z - Random.Range(-2f, 2f));
        transform.position = new Vector3(resetMLPosition.x - Random.Range(-2f, 2f), resetMLPosition.y, resetMLPosition.z - Random.Range(-2f, 2f));

        foreach(BulletScript bullet in GameObject.FindObjectsOfType<BulletScript>())
        {
            Destroy(bullet.gameObject);
        }
    }

    public override void CollectObservations()
    {
        float rayDistance = 20f;

        AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 1f, 0f));
        AddVectorObs(health);
        AddVectorObs(isReloading);
        AddVectorObs(isBlocking);
        AddVectorObs(transform.position);
        AddVectorObs(transform.rotation);
        AddVectorObs(playerRigidBody.velocity.x);
        AddVectorObs(playerRigidBody.velocity.z);

        if (target != null)
        {
            IPlayerStats targetIPlayerStats = target.GetComponent<IPlayerStats>();

            AddVectorObs(targetIPlayerStats.health);
            //41
            AddVectorObs(targetIPlayerStats.isReloading);
            //42
            AddVectorObs(targetIPlayerStats.isBlocking);
            AddVectorObs(target.transform.position);
            AddVectorObs(target.transform.rotation);
            AddVectorObs(target.GetComponent<Rigidbody>().velocity.x);
            AddVectorObs(target.GetComponent<Rigidbody>().velocity.z);
        } else
        {
            AddVectorObs(0f);
            AddVectorObs(false);
            AddVectorObs(false);
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

        if(upDownSignal > 0f)
        {
            OnForward();
        } else if(upDownSignal < 0f)
        {
            OnBack();
        }

        if(leftRightSignal > 0f)
        {
            OnLeft();
        } else if(leftRightSignal < 0f)
        {
            OnRight();
        }

        if(blockAction > 0.5f)
        {
            if(!isBlocking)
            {
                OnBlock();
            }
        } else
        {
            if(isBlocking)
            {
                OnUnBlock();
            }
        }

        if(attackAction > 0.5f)
        {
            OnAttack();
        }
    }

        // Update is called once per frame
        //void Update()
        //{
        //    // Key for velocity
        //    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        //    {
        //        OnForward();
        //    }
        //    else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        //    {
        //        OnBack();
        //    }

        //    // Key for Direction
        //    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        //    {
        //        OnLeft();
        //    }
        //    else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        //    {
        //        OnRight();
        //    }

        //    if (Input.GetKeyDown(KeyCode.LeftControl))
        //    {
        //        OnBlock();
        //    }

        //    if (Input.GetKeyUp(KeyCode.LeftControl))
        //    {
        //        OnUnBlock();
        //    }

        //    if (Input.GetKey(KeyCode.Space))
        //    {
        //        OnAttack();
        //    }

        //    // KeyUp


        //}

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
        }
    }

    // When it's hit by another player, called by script object
    public void OnHit()
    {
        AddReward(-10f);
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
        }
    }

    void AttackAction()
    {
        // Add tiny reward to teach to correctly fire
        AddReward(0.01f);
        FireBullet();
        attackSequence = DOTween.Sequence();
        attackSequence.Insert(0f, gun.DOLocalMove(new Vector3(0f, 0f, -0.4f), 0.2f).SetEase(Ease.InOutBack));
        attackSequence.Insert(0.3f, gun.DOLocalMove(new Vector3(0f, 0f, 0f), 4.7f).SetEase(Ease.InOutQuad));

        attackSequence.OnComplete(AttackComplete);
        attackSequence.Play();
    }

    void FireBullet()
    {
        GameObject instantiatedBullet = GameObject.Instantiate(bulletPrefab, firePos.position, transform.rotation);
        instantiatedBullet.GetComponent<BulletScript>().firedFrom = this;
    }

    void AttackComplete()
    {
        isReloading = false;
    }


    void BlockAction()
    {
        // Add tiny reward to teach to correctly block
        AddReward(0.01f);
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, 0f, 1f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(60f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 0.5f), 0.25f).SetEase(Ease.OutBack));
        // TO DO
        // MoveShield To Position
    }

    void OnUnBlock()
    {
        shield.GetComponentInChildren<Collider>().enabled = false;
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalMove(shieldRestPos, 0.25f).SetEase(Ease.OutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 1f), 0.25f).SetEase(Ease.OutBack));

        defenseSequence.OnComplete(BlockComplete);
    }

    public void BlockComplete()
    {
        shield.GetComponentInChildren<Collider>().enabled = true;
        isBlocking = false;
    }

    public void OnSuccessHit()
    {
        AddReward(25f);
        Debug.Log(this.gameObject.name + " successfully hit");
    }

    public void OnDeath()
    {
        AddReward(-200f);
        Done();
        Debug.Log(this.gameObject.name + " isDead");
    }

    public void OnSuccessfulBlock()
    {
        AddReward(2f);
        Debug.Log(this.gameObject.name + " successfully blocked a shot");
    }
    public void OnShieldedHit()
    {
        AddReward(1f);
        Debug.Log(this.gameObject.name + " shot hit was blocked by a shield");
    }

    public void OnInvestigateAreaEnter(GameObject targetInArea)
    {
        target = targetInArea;
    }
    public void SetTarget(GameObject targetInArea)
    {
        AddReward(0.01f);
        if(target != targetInArea)
        {
            target = targetInArea;
        }
    }
    public void OnInvestigateAreaExit(GameObject targetInArea)
    {
        if (target != targetInArea)
        {
            target = null;
        }
    }

    public void OnEnemyDeath()
    {
        AddReward(200f);
        Done();
    }
}
