using DG.Tweening;
using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{
    bool attacking = false;
    bool blocking = false;

    Vector3 swordRestPos = new Vector3(0.7f, 0f, 0.4f);
    Vector3 shieldRestPos = new Vector3(-0.7f, 0f, 0.2f);

    Sequence attackSequence;
    Sequence defenseSequence;

    Rigidbody playerRigidBody;
    public float turnSpeed = 200;
    public float moveSpeed = 0.0001f;

    public Transform sword;
    public Transform shield;


    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Key for velocity
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            OnForward();
        } else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            OnBack();
        }


        // Key for Direction
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            OnLeft();
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            OnRight();
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            OnBlock();
        }

        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            OnUnBlock();
        }



        if (Input.GetKey(KeyCode.Space))
        {
            OnAttack();
        }

        // KeyUp


    }

    void OnForward()
    {
        Vector3 dirToGo = transform.forward;
        playerRigidBody.AddForce(dirToGo * moveSpeed * (blocking ? 0.5f : 1f), ForceMode.VelocityChange);

        float forwardLimit = blocking ? 2f : 5f;

        if (playerRigidBody.velocity.magnitude > forwardLimit)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * forwardLimit;
        }
    }
    void OnBack()
    {
        Vector3 dirToGo = -transform.forward;
        playerRigidBody.AddForce(dirToGo * moveSpeed * (blocking ? 0.5f : 1f), ForceMode.VelocityChange);

        float reverseLimit = blocking ? 1f : 2f;

        if (playerRigidBody.velocity.magnitude > reverseLimit)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * reverseLimit;
        }
    }
    void OnLeft()
    {
        Vector3 dirToRotate = -transform.up;
        transform.Rotate(dirToRotate, Time.fixedDeltaTime * turnSpeed * (blocking ? 0.5f : 1f));
    }
    void OnRight()
    {
        Vector3 dirToRotate = transform.up;
        transform.Rotate(dirToRotate, Time.fixedDeltaTime * turnSpeed * (blocking ? 0.5f : 1f));
    }


    void OnAttack()
    {
        if(!attacking && !blocking)
        {
            attacking = true;
            AttackAction();
        }
    }

    void AttackAction()
    {
        attackSequence = DOTween.Sequence();
        attackSequence.Insert(0f, sword.DOLocalMove(new Vector3(0f, 0f, 1f), 0.5f).SetEase(Ease.InCubic));
        attackSequence.Insert(0.5f, sword.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        attackSequence.Insert(1f, sword.DOLocalRotate(new Vector3(-90f, 0f, 0f), 0.15f).SetEase(Ease.InOutBack));
        attackSequence.Insert(1.1f, sword.DOLocalMove(swordRestPos, 0.25f).SetEase(Ease.OutBack));

        attackSequence.OnComplete(AttackComplete);
        attackSequence.Play();
        // TO DO
        //MoveSword(() => { attacking = false });
    }

    void AttackComplete()
    {
        attacking = false;
    }


    void OnBlock()
    {
        if(!attacking && !blocking)
        {
            blocking = true;
            BlockAction();
        }
    }

    void BlockAction()
    {
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, -0.25f, 1f), 0.25f).SetEase(Ease.InCubic));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.25f).SetEase(Ease.InOutBack));
        //defenseSequence.OnComplete(BlockComplete);
        // TO DO
        // MoveShield To Position
    }

    void OnUnBlock()
    {
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalMove(shieldRestPos, 0.25f).SetEase(Ease.OutBack));
        defenseSequence.OnComplete(BlockComplete);
    }

    void BlockComplete()
    {
        blocking = false;
    }
}
