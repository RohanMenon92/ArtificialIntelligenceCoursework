using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerStats
{
    float health { get; set; }
    float speed { get; set; }
    float turnSpeed { get; set; }

    bool isReloading { get; set; }
    bool isBlocking { get; set; }

    // On Input to Attack
    void OnAttack();
    //On Input to Block
    void OnBlock();

    // On Player being hit directly
    void OnHit();
    // On Successful Attempt to hit enemy
    void OnSuccessHit();
    // On Succesfully Blocking a hit
    void OnSuccessfulBlock();
    // On Shielded Hit to Enemy
    void OnShieldedHit();

    // On Player dying
    void OnDeath();

}
