using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public IPlayerStats firedFrom;
    ParticleSystem particleSystem1;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().velocity = transform.forward * 50f;
        particleSystem1 = this.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        IPlayerStats iPlayerCollision = collision.gameObject.GetComponent<IPlayerStats>();
        if (collision.gameObject.tag == "wall")
        {
            BulletDestroy();
        }
        else if (collision.gameObject.tag == "Shield") {
            IPlayerStats iShieldCollision = collision.transform.parent.parent.GetComponent<IPlayerStats>();
            GetComponent<Rigidbody>().velocity = -GetComponent<Rigidbody>().velocity;
            if (iShieldCollision != null && iShieldCollision != firedFrom)
            {
                iShieldCollision.OnSuccessfulBlock();
                firedFrom.OnShieldedHit();

                BulletDestroy(true);
            }
        }
        else if (iPlayerCollision != null)
        {
            iPlayerCollision.OnHit();
            firedFrom.OnSuccessHit();
            BulletDestroy();
        }
    }

    private void BulletDestroy(bool wasShielded = false)
    {
        if(particleSystem1 == null)
        {
            particleSystem1 = this.GetComponent<ParticleSystem>();
        }

        if(!wasShielded)
        {
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            particleSystem1.Stop();
            particleSystem1.Emit(1000);
        }
        this.GetComponent<Collider>().enabled = false;
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        StartCoroutine(WaitAndDestroy());
        //Destroy(gameObject);
    }

    IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
