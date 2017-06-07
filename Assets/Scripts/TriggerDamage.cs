using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDamage : MonoBehaviour {

    string[] m_triggerTags;
    float m_lifetime;
    int m_damage;

    IDamagable m_instance;

    void UpdateLifetime()
    {
        m_lifetime -= Time.deltaTime;

        if (m_lifetime < 0)
        {
            DestroyObject(gameObject);
        }
    }

    void Update()
    {
        UpdateLifetime();
    }

    public void Init(float time, string[] triggeringTags, int damage)
    {
        m_lifetime = time;
        m_damage = damage;
        m_triggerTags = triggeringTags;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        foreach(string s in m_triggerTags)
        {
            if (col.CompareTag(s))
            {
                m_instance = col.GetComponent<IDamagable>();
                m_instance.TakeDamage(m_damage);
            }
        }
    }
}
