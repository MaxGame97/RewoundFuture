using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorFadeOut : MonoBehaviour {

    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [SerializeField] private Texture fadeTexture;   // The texture used to fade out the screen



    // ----------------------
    // --- Private values ---
    // ----------------------

    float fadeSpeed;                                // The speed of the fade out effect (in seconds, larger takes longer time)
    float fadeAlpha;                                // The current alpha for the screen fade effect



    // -------------------------
    // --- Public properties ---
    // -------------------------
    
    public float FadeSpeed { set { fadeSpeed = value; } }
    
        
        
    // Use this for initialization
    void Start () {
        // Set the fade alpha to 1
        fadeAlpha = 1f;

        // Start the fade out coroutine
        StartCoroutine(FadeOut());
    }

    private void OnGUI()
    {
        GUI.color = new Color(0f, 0f, 0f, fadeAlpha);

        if(fadeTexture != null)
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeTexture);
    }

    // Decreases the fade alpha and when it reaches 0, destroys this object
    IEnumerator FadeOut()
    {
        // While the fade alpha is smaller than 0
        while (fadeAlpha > 0f)
        {
            // Increase the fade alpha based on the fade speed
            fadeAlpha -= (1 / fadeSpeed) * Time.deltaTime;

            // If the fade alpha is larger than 0
            if (fadeAlpha < 0f)
                // Set it to 0
                fadeAlpha = 0f;

            yield return null;
        }

        // Destroy this gameObject
        Destroy(gameObject);
    }
}
