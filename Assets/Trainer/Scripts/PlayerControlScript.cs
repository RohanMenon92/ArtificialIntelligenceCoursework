using DG.Tweening;
using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour, IPlayerStats
{
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
    public Sequence attackSequence;
    public Sequence defenseSequence;

    Rigidbody playerRigidBody;

    public Transform gun;
    public Transform shield;

    // Start is called before the first frame update
    void Start()
    {
        health = 50;
        speed = moveSpeed;
        turnSpeed = rotateSpeed;
        playerRigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
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

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            OnBlock();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            OnUnBlock();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            OnAttack();
        }

        // KeyUp
    }

    public void Reset()
    {
        health = 50;
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
        }
    }

    // When it's hit by another player, called by script object
    public void OnHit()
    {
        this.health -= 10;
        this.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        if(health <= 0f)
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
        FireBullet();
        attackSequence = DOTween.Sequence();
        attackSequence.Insert(0f, gun.DOLocalMove(new Vector3(0f, 0f, -0.4f), 0.2f).SetEase(Ease.InOutBack));
        attackSequence.Insert(0.3f, gun.DOLocalMove(new Vector3(0f, 0f, 0f), 4.7f).SetEase(Ease.InOutQuad));

        attackSequence.OnComplete(AttackComplete);
        attackSequence.Play();
        // TO DO
        //MoveSword(() => { attacking = false });
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
        defenseSequence = DOTween.Sequence();
        defenseSequence.Insert(0f, shield.DOLocalMove(new Vector3(0f, 0f, 1f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, shield.DOLocalRotate(new Vector3(60f, 0f, 0f), 0.25f).SetEase(Ease.InOutBack));
        defenseSequence.Insert(0f, gun.DOScale(new Vector3(1f, 1f, 0.5f), 0.25f).SetEase(Ease.OutBack));
        // TO DO
        // MoveShield To Position
    }

    void OnUnBlock()
    {
        if (defenseSequence.IsPlaying())
        {
            defenseSequence.Kill(true);
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
        Debug.Log(this.gameObject.name + " successfully hit");
    }

    public void OnDeath()
    {
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

    public void KillDefenseSequence()
    {
        defenseSequence.Kill(true);
    }

    public void KillAttackSequence()
    {
        attackSequence.Kill(true);
    }
}
