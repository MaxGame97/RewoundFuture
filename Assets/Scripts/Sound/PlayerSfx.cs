using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSfx : MonoBehaviour {

	public EventPlayer sfx_Land;
	public EventPlayer sfx_Jump;
	public EventPlayer sfx_SwordSwing;


//	public bool playSwordSwing = false; 


	// Use this for initialization
	void Start () {

	

		
	}
	
	// Update is called once per frame
	void Update () {

//		if (playSwordSwing == true) {
//			SwordSwing.PlayEvent ();
//			playSwordSwing = false;
//		}
			
		
	}

	void PlaySwordSwing (){

		sfx_SwordSwing.PlayEvent ();

	}
	void PlayJump (){

		sfx_Jump.PlayEvent ();
		print ("JUMP!");

	}
	void PlayLand (){

		sfx_Land.PlayEvent ();
		print ("Land");

	}

}
