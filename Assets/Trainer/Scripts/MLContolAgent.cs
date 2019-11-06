using DG.Tweening;
using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MLContolAgent : Agent, IPlayerStats
{
    GameObject target = null;
    public string tagToTarget = "FSMPlayer";
    public float health { get; set; }
    public float speed { get; set; }

    public bool isReloading { get; set; }
    public bool isBlocking { get; set; }

    float yLevel = -0.5f;
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

    float accuracyThreshold = 30f;
    float targetDistanceThreshold = 6f;
    //RayPerception m_RayPer;

    //float[] m_RayAngles = { -40f, -20f, 0f, 20f, 40f, 60f, 80f, 100f, 120f, 140f, 160f, 180f, 200f, 220f };
    //string[] detectableObjects = { "Bullet", "FSMPlayer", "Player", "wall" };
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
        //m_RayPer = GetComponent<RayPerception>();

        health = 50;
        speed = moveSpeed;
        turnSpeed = rotateSpeed;
        playerRigidBody = GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
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

        foreach (BulletScript bullet in GameObject.FindObjectsOfType<BulletScript>())
        {
            Destroy(bullet.gameObject);
        }
    }

    public override void CollectObservations()
    {
        //float rayDistance = 20f;

        //Add self Observations
        //AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 0f, 0f));
        //AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, detectableObjects, 1f, 0f));
        AddVectorObs(health);
        AddVectorObs(isReloading);
        AddVectorObs(isBlocking);
        AddVectorObs(new Vector3(transform.localPosition.x, yLevel, transform.localPosition.z));
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
            AddVectorObs(new Vector3(target.transform.localPosition.x, yLevel, target.transform.localPosition.z));
            float lookAtDeviation = transform.localRotation.eulerAngles.y - (Quaternion.LookRotation(target.transform.position - new Vector3(transform.localPosition.x, yLevel, transform.localPosition.z)).eulerAngles.y);
            AddVectorObs(lookAtDeviation);
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
            AddVectorObs(0f);
            AddVectorObs(Vector3.zero);
            AddVectorObs(0f);
            AddVectorObs(0f);
            AddVectorObs(0f);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float upDownSignal = (int)Mathf.FloorToInt(vectorAction[0]);
        float leftRightSignal = (int)Mathf.FloorToInt(vectorAction[1]);
        float blockAction = (int)Mathf.FloorToInt(vectorAction[2]);
        float attackAction = (int)Mathf.FloorToInt(vectorAction[3]);

        // Set up bool values so that fixed update can decide whenn to do it per frame
        if (upDownSignal == 1)
        {
            OnDecisionForward(true);
            OnDecisionBack(false);
        }
        else if (upDownSignal == 2)
        {
            OnDecisionBack(true);
            OnDecisionForward(false);
        } else
        {
            OnDecisionBack(false);
            OnDecisionForward(false);
        }

        if (leftRightSignal == 1)
        {
            OnDecisionLeft(true);
            OnDecisionRight(false);
        }
        else if (leftRightSignal == 2)
        {
            OnDecisionLeft(false);
            OnDecisionRight(true);
        } else {
            OnDecisionLeft(false);
            OnDecisionRight(false);
        }

        if (blockAction == 1)
        {
            OnDecisionBlock(true);
            OnDecisionUnBlock(false);
        }
        else
        {
            OnDecisionBlock(false);
            OnDecisionUnBlock(true);
        }

        if (attackAction == 1)
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

    void FixedUpdate()
    {
        // Calculate deviation
        float lookAtDeviation = 0f;
        if (target != null)
        {
            lookAtDeviation = transform.localRotation.eulerAngles.y - (Quaternion.LookRotation(target.transform.localPosition - new Vector3(transform.localPosition.x, yLevel, transform.localPosition.z)).eulerAngles.y);
        }

        // Check decision bools to have smooth input
        if (onDecisionForward)
        {
            OnForward();
        }
        else if (onDecisionBack)
        {
            OnBack();
        } else
        {
            //AddReward(-0.0001f);
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
            if(target != null)
            {
                if(Mathf.Abs(lookAtDeviation) < 2f)
                {
                    AddReward(5f);
                } else if (Mathf.Abs(lookAtDeviation) < 10f)
                {
                    // If looking towards player and firing, add a reward
                    AddReward(2f);
                }
                else
                {
                    // Add negative reward for firing
                    AddReward(-0.1f);
                }
            } else
            {
                // Add negative reward for firing if not looking at player
                AddReward(-0.5f);
            }
            OnAttack();
        }

        
        // Add Reward if Pointing to target
        if(target != null)
        {
            if (Mathf.Abs(lookAtDeviation) < accuracyThreshold)
            {
                if (Mathf.Abs(lookAtDeviation) < 5f)
                {
                    // Add HighReward for looking within 10 degrees
                    AddReward(0.004f);
                }
                else
                {
                    // Add Reward less than 1
                    AddReward(0.001f * Mathf.Abs(lookAtDeviation) / accuracyThreshold);
                }
            } else
            {
                //Deduct reward if not looking
                AddReward(-0.01f);
            }

            //Add Reward based on close to target or not
            float distanceToTarget = Vector3.Distance(target.transform.localPosition, transform.localPosition);
            if (distanceToTarget > targetDistanceThreshold)
            {
                AddReward(-0.01f * distanceToTarget);
            }
            else
            {
                AddReward(0.1f);
            }
        } else
        {
            AddReward(-0.001f);
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
            AddReward(3f);
            isReloading = true;
            AttackAction();
        } else
        {
            AddReward(-0.001f);
        }
    }

    // When it's hit by another player, called by script object
    public void OnHit(IPlayerStats hittingPlayer)
    {
        AddReward(-100f);
        this.health -= 10;
        this.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        if (health <= 0f)
        {
            hittingPlayer.OnKill();
            OnDeath();
        }
    }

    // When player blocks attack
    public void OnBlock()
    {
        if (!isReloading && !isBlocking)
        {
            isBlocking = true;
            BlockAction();
        } else
        {
            AddReward(-0.001f);
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
        GameObject instantiatedBullet = GameObject.Instantiate(bulletPrefab, firePos.position, transform.localRotation);
        instantiatedBullet.GetComponent<BulletScript>().firedFrom = this;
    }

    void AttackComplete()
    {
        isReloading = false;
    }

    void BlockAction()
    {
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, 0f, 1f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(60f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 0.5f), 0.25f).SetEase(Ease.OutBack));
    }

    void OnUnBlock()
    {
        if(!isBlocking)
        {
            AddReward(-0.001f);
            return;
        }
        if(defenseSequence.IsPlaying())
        {
            defenseSequence.Kill(true);
        }
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalMove(shieldRestPos, 0.25f).SetEase(Ease.OutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 1f), 0.25f).SetEase(Ease.OutBack));

        defenseSequence.OnComplete(BlockComplete);
    }

    public void OnKill()
    {
        AddReward(800f);
        Done();
    }

    public void BlockComplete()
    {
        isBlocking = false;
    }

    public void OnSuccessHit()
    {
        AddReward(300f);
        Debug.Log(this.gameObject.name + " successfully hit");
    }

    public void OnDeath()
    {
        AddReward(-800f);
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
        AddReward(25f);
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
        AddReward(500f);
        Done();
    }

    public void OnCollisionStay(Collision collision)
    {
        // Prevent it from touching walls for too long
        if (collision.gameObject.tag == "wall")
        {
            AddReward(-0.1f);
        }
    }

    public void KillDefenseSequence()
    {
        defenseSequence.Kill(true);
    }

    public void KillAttackSequence()
    {
        attackSequence.Kill(true);
    }

    public void Reset()
    {
        
    }
}
