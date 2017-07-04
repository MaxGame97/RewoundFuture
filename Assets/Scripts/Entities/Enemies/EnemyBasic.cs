using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasic : MonoBehaviour, IDamagable {

    IDamagable m_damageInstance;

    public Stats m_stats;
    
    // Use this for initialization
    void Start () {
        m_damageInstance = GetComponent<IDamagable>();
	}

    void IDamagable.TakeDamage(int amount)
    {
        m_stats.health -= amount;

        m_stats.health = Mathf.Clamp(m_stats.health, 0, m_stats.maxHealth);

        print(amount + " " + m_stats.maxHealth);

        if (m_stats.health <= 0)
        {
            m_damageInstance.Kill();
        }
    }

    void IDamagable.Kill()
    {
        Destroy(gameObject);
    }

}

