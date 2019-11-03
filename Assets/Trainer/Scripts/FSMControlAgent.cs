﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMControlAgent : MonoBehaviour, IPlayerStats
{
    public float health { get; set; }
    public float speed { get; set; }

    float yLevel = 0.4f;
    float minimumHeading = 3f;
    public bool isReloading { get; set; }
    public bool isBlocking { get; set; }

    public float turnSpeed { get; set; }

    public float rotateSpeed = 200;
    public float moveSpeed = 0.1f;

    public GameObject bulletPrefab;
    public Transform firePos;

    public Transform gun;
    public Transform shield;

    public SpriteRenderer stateDisplay;
    public Sprite patrolSprite;
    public Sprite investigateSprite;
    public Sprite confrontSprite;
    public Sprite dodgeSprite;
    public Sprite deadSprite;

    Vector3 shieldRestPos = new Vector3(0f, 0.6f, 0.75f);

    Sequence attackSequence;
    Sequence defenseSequence;

    Rigidbody playerRigidBody;

    float healthThreshold = 50;

    List<Vector3> waypoints = new List<Vector3>();
    int waypointIterator = 0;

    private State currentState = State.Patrol;

    // To check Entry and Exit of state
    private State lastState = State.Patrol;

    GameObject target;
    Vector3 targetHeading;

    float targetingDistance = 12f;

    #region FSMStatesRegion
    private enum State {
        Patrol = 0,
        Investigate = 1,
        Confront = 2,
        Dodge = 3,
        Dead = 4
        //Flee = 0
    };

    void Start()
    {
        health = 100;
        speed = moveSpeed;
        turnSpeed = rotateSpeed;

        target = GameObject.FindWithTag("Player");
        playerRigidBody = GetComponent<Rigidbody>();

        waypoints = new List<Vector3>();
        waypoints.Add(new Vector3(transform.position.x, yLevel, transform.position.z + 5f));
        waypoints.Add(new Vector3(transform.position.x + 5f, yLevel, transform.position.z - 5f));
        waypoints.Add(new Vector3(transform.position.x - 5f, yLevel, transform.position.z + 5f));

        waypointIterator = 0;
        targetHeading = waypoints[waypointIterator];
    }

    void SwitchState(State newState)
    {
        bool switchAllowed = false;

        // Check if switch between states is allowed
        switch (currentState)
        {
            case State.Patrol:
                {
                    switchAllowed = newState == State.Investigate || newState == State.Dead;
                }
                break;
            case State.Investigate:
                {
                    switchAllowed = newState == State.Confront || newState == State.Dodge || newState == State.Patrol || newState == State.Dead;
                }
                break;
            case State.Confront:
                {
                    switchAllowed = newState == State.Dodge || newState == State.Investigate || newState == State.Dead;
                }
                break;
            case State.Dodge:
                {
                    switchAllowed = newState == State.Confront || newState == State.Investigate || newState == State.Dead;
                }
                break;
            case State.Dead:
                {
                    switchAllowed = false;
                }
                break;
        }

        if(switchAllowed)
        {
            currentState = newState;
        }
    }

    // Check current and last state and switch when required
    void CheckState(State currState)
    {
        if(currentState != lastState)
        {
            OnExitState(lastState);
            OnEnterState(currentState);
            lastState = currentState;
        }
        OnProcessState(currentState);
    }

    // Check entry to stateEnter
    void OnEnterState(State stateEnter)
    {
        switch (stateEnter)
        {
            case State.Patrol:
                {
                    stateDisplay.sprite = patrolSprite;
                    target = null;
                    waypoints = new List<Vector3>();
                    waypoints.Add(new Vector3(transform.position.x, yLevel, transform.position.z + 5f));
                    waypoints.Add(new Vector3(transform.position.x - 5f, yLevel, transform.position.z + 5f));
                    waypoints.Add(new Vector3(transform.position.x + 5f, yLevel, transform.position.z + 5f));
                }
                break;
            case State.Investigate:
                {
                    stateDisplay.sprite = investigateSprite;
                }
                break;
            case State.Confront:
                {
                    stateDisplay.sprite = confrontSprite;
                    if (isBlocking)
                    {
                        OnUnBlock();
                    }
                }
                break;
            case State.Dodge:
                {
                    stateDisplay.sprite = dodgeSprite;
                    playerRigidBody.velocity = Vector3.zero;
                }
                break;
            case State.Dead:
                {
                    stateDisplay.sprite = deadSprite;

                }
                break;
        }
    }

    // Check Exit from last state
    void OnExitState(State stateExit)
    {
        switch(stateExit)
        {
            case State.Patrol:
                {

                }
                break;
            case State.Investigate:
                {

                }
                break;
            case State.Confront:
                {
                }
                break;
            case State.Dodge:
                {
                }
                break;
            case State.Dead:
                {

                }
                break;
        }
    }

    // Loop for currState
    void OnProcessState(State currState)
    {
        switch (currState)
        {
            case State.Patrol:
                {
                    minimumHeading = 3f;
                    // Loop through waypoints
                    if ((transform.position - targetHeading).magnitude < minimumHeading)
                    {
                        waypointIterator++;
                        if (waypointIterator == waypoints.Count)
                        {
                            waypointIterator = 0;
                        }
                        targetHeading = waypoints[waypointIterator];
                    }
                }
                break;
            case State.Investigate:
                {
                    minimumHeading = 8f;
                    if (target != null)
                    {
                        targetHeading = target.transform.position;
                    }

                    if (!isBlocking && health <= healthThreshold && !isReloading && Vector3.Distance(transform.position, target.transform.position) < targetingDistance/2)
                    {
                        OnBlock();
                    }

                }
                break;
            case State.Confront:
                {
                    minimumHeading = 10f;

                    targetHeading = target.transform.position + (target.GetComponent<Rigidbody>().velocity * 2);
                }
                break;
            case State.Dodge:
                {
                    minimumHeading = 8f;
                    targetHeading = target.transform.position + target.GetComponent<Rigidbody>().velocity;

                    if (!isBlocking && health <= healthThreshold && !isReloading)
                    {
                        OnBlock();
                    }
                }
                break;
            case State.Dead:
                {

                }
                break;
        }
        // Rotate to target
        transform.rotation = Quaternion.Slerp(transform.rotation,
             Quaternion.LookRotation(targetHeading - new Vector3(transform.position.x, yLevel, transform.position.z)), turnSpeed * Time.deltaTime);

        // Decide to Attack
        if(Mathf.Abs(transform.rotation.eulerAngles.y - (Quaternion.LookRotation(targetHeading - new Vector3(transform.position.x, yLevel, transform.position.z)).eulerAngles.y)) < 0.1f) {
            if(currentState == State.Confront && !isBlocking)
            {
                OnAttack();
            }
        }


        // Move towards or away from target
        Vector3 dirToGo = new Vector3();
        if (currState == State.Dodge)
        {
            dirToGo = -transform.forward;
        }
        else
        {
            dirToGo = transform.forward;
        }
        float distanceToTarget = (transform.position - targetHeading).magnitude;
        dirToGo = dirToGo * moveSpeed * (distanceToTarget < minimumHeading ? distanceToTarget/(minimumHeading + 1f) : 1f);
        playerRigidBody.AddForce(new Vector3(dirToGo.x, 0f, dirToGo.z) * (isBlocking ? 0.5f : 1f), ForceMode.VelocityChange);

        // put speed limits
        float forwardLimit = isBlocking ? 1f : 5f;
        if(currState != State.Dodge)
        {
            if (playerRigidBody.velocity.magnitude > forwardLimit)
            {
                playerRigidBody.velocity = playerRigidBody.velocity.normalized * forwardLimit;
            }
        } else
        {
            float reverseLimit = isBlocking ? 1f : 2f;

            if (playerRigidBody.velocity.magnitude > reverseLimit)
            {
                playerRigidBody.velocity = playerRigidBody.velocity.normalized * reverseLimit;
            }
        }
    }

    #endregion

    public void OnAttack()
    {
        if (!isReloading && !isBlocking)
        {
            isReloading = true;
            AttackAction();
        }
    }
    void AttackAction()
    {
        FireBullet();
        attackSequence = DOTween.Sequence();
        attackSequence.Insert(0f, gun.DOLocalMove(new Vector3(0f, 0f, -0.4f), 0.2f).SetEase(Ease.InOutBack));
        attackSequence.Insert(0.3f, gun.DOLocalMove(new Vector3(0f, 0f, 0f), 4.7f).SetEase(Ease.InOutQuad));

        attackSequence.OnComplete(AttackComplete);
        attackSequence.Play();
        SwitchState(State.Dodge);
    }

    void AttackComplete()
    {
        isReloading = false;
        if(health <= healthThreshold)
        {
            SwitchState(State.Dodge);
        } else
        {
            SwitchState(State.Investigate);
        }
    }

    void BlockAction()
    {
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, 0f, 1f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(60f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 0.5f), 0.25f).SetEase(Ease.OutBack));
        //if(target != null)
        //{
        //    if(isReloading)
        //    {
        //        SwitchState(State.Dodge);
        //    } else
        //    {
        //        SwitchState(State.Investigate);
        //    }
        //} else
        //{
        //    SwitchState(State.Patrol);
        //}
    }

    void OnUnBlock()
    {
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalMove(shieldRestPos, 0.3f).SetEase(Ease.OutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.OutBack));
        defenseSequence.OnComplete(BlockComplete);
    }

    void BlockComplete()
    {
        isBlocking = false;
    }

    void FireBullet()
    {
        GameObject instantiatedBullet = GameObject.Instantiate(bulletPrefab, firePos.position, transform.rotation);
        instantiatedBullet.GetComponent<BulletScript>().firedFrom = this;
    }

    // When it's hit by another player, called by script object
    public void OnHit()
    {
        this.health -= 10;
        this.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        if (currentState != State.Confront && health < healthThreshold)
        {
            OnBlock();
        }

        if (health <= 0)
        {
            SwitchState(State.Dead);
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

    void Update()
    {

    }

    void FixedUpdate()
    {
        CheckState(currentState);
    }

    public void OnAttackAreaEnter()
    {
        SwitchState(State.Confront);
    }

    public void OnAttackAreaExit()
    {
        if(currentState != State.Dodge && Vector3.Distance(transform.position, target.transform.position) > targetingDistance)
        {
            SwitchState(State.Investigate);
        }
    }

    public void OnInvestigateAreaEnter()
    {
        SwitchState(State.Investigate);
    }

    public void OnInvestigateAreaExit()
    {
        SwitchState(State.Patrol);
    }

    public void SetTarget(GameObject targetDetected)
    {
        target = targetDetected;
    }

    public void OnAttackAreaStay()
    {
        // If dodging, dont confront
        if(currentState == State.Dodge)
        {
            return;
        }
        // Else if investigating, confront
        if(currentState == State.Investigate)
        {
            SwitchState(State.Confront);
        }
    }

    public void OnSuccessHit()
    {
        Debug.Log(this.gameObject.name + " successfully hit");
    }

    public void OnDeath()
    {
        if(FindObjectOfType<MLContolAgent>() != null)
        {
            FindObjectOfType<MLContolAgent>().OnEnemyDeath();
        }
        Debug.Log(this.gameObject.name + " isDead");
    }

    public void OnSuccessfulBlock()
    {
        Debug.Log(this.gameObject.name + " successfully blocked a shot");
    }
    public void OnShieldedHit()
    {
        Debug.Log(this.gameObject.name + " shot hit was blocked by a shield");
    }
}