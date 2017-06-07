using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDamagable
{
    void TakeDamage(int amount);
    void Kill();
}

[System.Serializable]
public class Stats{

    public int maxHealth;
    public int health;

}


