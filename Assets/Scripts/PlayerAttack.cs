using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackInfo
{
    public AttackType type;
    public float duration;
    public int damage;
    public Vector2 offset;
}

public class PlayerAttack : MonoBehaviour {

    public AttackInfo[] m_attacks;
    public GameObject[] m_triggers;

    string[] m_tags = { "Enemy" };

    public void Attack(AttackType attack, bool xDir)
    {
        switch (attack)
        {
            case AttackType.light:
                Vector2 offset = xDir ? m_attacks[0].offset * -1 : m_attacks[0].offset;

                CreateDamageTrigger(0.9f, m_attacks[0].damage, offset);

                break;
            case AttackType.medium:
                break;
            case AttackType.heavy:
                break;
            default:
                break;
        }
    }

    void CreateDamageTrigger(float duration, int damage, Vector2 offset)
    {
        GameObject g = Instantiate<GameObject>(m_triggers[0], transform.position + (Vector3)offset, Quaternion.identity, transform);
        g.GetComponent<TriggerDamage>().Init(duration, m_tags, damage);
    }
}
