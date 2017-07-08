using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransition : MonoBehaviour {

    public MusicController musicPlayer;
    public float musicTransitionValue;
	// Use this for initialization
	void Start () {
        

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            musicPlayer = GameObject.Find("MusicPlayer(Clone)").GetComponent<MusicController>();
            musicPlayer.Music_InLevelTransition(musicTransitionValue);
        }
    }



}
