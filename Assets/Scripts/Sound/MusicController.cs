using UnityEngine;

public class MusicController : MonoBehaviour {


    public EventPlayer currentMusic;
	

    void Start()
    {
        currentMusic = GameObject.Find("Music_Level1_DecrepidShrine").GetComponent<EventPlayer>();
    }

    public void Music_InLevelTransition(float TransitionValue)
    {

        currentMusic.ChangeParameter("Transition1", TransitionValue);

    }


}
