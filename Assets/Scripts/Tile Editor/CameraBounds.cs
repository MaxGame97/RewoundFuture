using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraBounds : MonoBehaviour
{

    // ----------------------
    // --- Private values ---
    // ----------------------

    [SerializeField] private Vector2 horizontalBounds = new Vector2();
    [SerializeField] private Vector2 verticalBounds = new Vector2();



    // -------------------------
    // --- Public properties ---
    // -------------------------

    public Vector2 HorizontalBounds { get { return horizontalBounds; } set { horizontalBounds = value; } }
    public Vector2 VerticalBounds { get { return verticalBounds; } set { verticalBounds = value; } }



    // Update is called once per frame
    private void Update()
    {
        // If the left horizontal bounds value is larger than the right, swap them
        if (horizontalBounds.x > horizontalBounds.y)
        {
            float temp = horizontalBounds.x;
            horizontalBounds.x = horizontalBounds.y;
            horizontalBounds.y = temp;
        }

        // If the bottom vertical bounds value is larger than the top, swap them
        if (verticalBounds.x > verticalBounds.y)
        {
            float temp = verticalBounds.x;
            verticalBounds.x = verticalBounds.y;
            verticalBounds.y = temp;
        }
    }
    
    // OnDrawGizmosSelected is called when the object is selected and gizmos are enabled
    void OnDrawGizmosSelected()
    {
        // --------------------------------------------------
        // --- Draw a representation of the camera bounds ---
        // --------------------------------------------------

        // Set the color to a very transparent yellow
        Gizmos.color = Color.green;

        // Drawn the four lines representing the camera bounds
        Gizmos.DrawLine(new Vector3(horizontalBounds.x, verticalBounds.x), new Vector3(horizontalBounds.x, verticalBounds.y));
        Gizmos.DrawLine(new Vector3(horizontalBounds.y, verticalBounds.x), new Vector3(horizontalBounds.y, verticalBounds.y));
        Gizmos.DrawLine(new Vector3(horizontalBounds.x, verticalBounds.x), new Vector3(horizontalBounds.y, verticalBounds.x));
        Gizmos.DrawLine(new Vector3(horizontalBounds.x, verticalBounds.y), new Vector3(horizontalBounds.y, verticalBounds.y));
    }

    // Generates a rough camera bounds based on the child objects
    public void GenerateRoughCameraBounds()
    {
        // Get all colliders from the child object of this object
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();

        // Reset the camera bounds values
        horizontalBounds = Vector2.zero;
        verticalBounds = Vector2.zero;

        // If there are more than 0 child colliders
        if(childColliders.Length != 0)
        {
            // Get the extents from the first collider
            Vector2 boundsExtents = childColliders[0].bounds.extents;

            // Set the camera bounds values to contain the first collider
            horizontalBounds.x = childColliders[0].transform.position.x - boundsExtents.x;
            horizontalBounds.y = childColliders[0].transform.position.x + boundsExtents.x;
            verticalBounds.x = childColliders[0].transform.position.y - boundsExtents.y;
            verticalBounds.y = childColliders[0].transform.position.y + boundsExtents.y;

            // Check all but the first collider
            for (int i = 1; i < childColliders.Length; i++)
            {
                // Get the extents from the current collider
                boundsExtents = childColliders[i].bounds.extents;

                // Get the values containing this collider
                float horizontalX = childColliders[i].transform.position.x - boundsExtents.x;
                float horizontalY = childColliders[i].transform.position.x + boundsExtents.x;
                float verticalX = childColliders[i].transform.position.y - boundsExtents.y;
                float verticalY = childColliders[i].transform.position.y + boundsExtents.y;

                // If these values are larger than the previous values, update them to the new ones
                horizontalBounds.x = horizontalBounds.x < horizontalX ? horizontalBounds.x : horizontalX;
                horizontalBounds.y = horizontalBounds.y > horizontalY ? horizontalBounds.y : horizontalY;
                verticalBounds.x = verticalBounds.x < verticalX ? verticalBounds.x : verticalX;
                verticalBounds.y = verticalBounds.y > verticalY ? verticalBounds.y : verticalY;
            }
        }
    }
}
