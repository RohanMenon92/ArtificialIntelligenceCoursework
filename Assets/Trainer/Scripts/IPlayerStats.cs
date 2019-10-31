using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerStats
{
    float health { get; set; }
    float speed { get; set; }
    bool isAttacking { get; set; }
    bool isBlocking { get; set; }

    // On Input to Attack
    bool OnAttack();
    //On Input to Block
    bool OnBlock();

    // On Attempt to hit player
    void OnHit(IPlayerStats atackPlayer);
    //On Blocking enemy Attack
    void OnBlockAttack();
    //On Successful hit to player
    void OnSuccessHit();
    //On Succesful attack to other player
    void OnSuccessAttack();
}
