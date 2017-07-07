using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicInitiator : MonoBehaviour {

    public GameObject musicPlayer;
    private static MusicInitiator instanceRef;

    void Awake()
    {
        if (instanceRef == null)
        {
            instanceRef = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }


    }


	void Start ()
    {

        Instantiate(musicPlayer, transform);
		
	}

    
}
