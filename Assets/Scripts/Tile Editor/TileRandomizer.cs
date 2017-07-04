using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class TileRandomizer : MonoBehaviour {

    public Sprite[] m_variants = new Sprite[0];
	// Use this for initialization
	void Start() 
    {
        int rand = Random.Range(0, m_variants.Length);

        GetComponent<SpriteRenderer>().sprite = m_variants[rand];
	}
}
