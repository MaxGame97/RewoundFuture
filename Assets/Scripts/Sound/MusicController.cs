using UnityEngine;

public class MusicController : MonoBehaviour {


    public EventPlayer MusicToControl;
	public float ParamValue = 1f;



	// Update is called once per frame
	void Update ()
    {
		//if(	Input.GetKeyDown(KeyCode.M))
		//{
		//	MusicToControl.ChangeParameter("Transition", ParamValue);
		//}


	}

	void OnTriggerEnter2D(Collider2D col)
	{
	
		Debug.Log ("triggered");
		MusicToControl.ChangeParameter("Transition", ParamValue);

	}


	
}
