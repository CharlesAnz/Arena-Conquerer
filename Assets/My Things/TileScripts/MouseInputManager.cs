using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputManager : MonoBehaviour
{
    GameObject currenthover = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Hovering();
    }


    public void Hovering()
    {
        RaycastHit tileHovered;

        //creates a ray at the current mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Casts ray into the screen and detects if it has hit an object
        if (Physics.Raycast(ray, out tileHovered))
        {
            //checks if the raycast hit object contains the IMouseInput component
            if (tileHovered.transform.gameObject.GetComponent<IMouseInput>() != null)
            {
                //checks if the current object the mouse is hovering over is not the same as raycast hit object
                if(currenthover != tileHovered.transform.gameObject)
                {
                    //runs OnMouseExit on the old game object being hovered if it has the IMouseInput compononent
                    currenthover?.gameObject.GetComponent<IMouseInput>()?.OnMouseExit();

                    //makes the new object that the mouse is hovering over the currently hovered object
                    currenthover = tileHovered.transform.gameObject;

                    //runs OnMouseEnter on the new game object being hovered if it has the IMouseInput compononent
                    tileHovered.transform.gameObject.GetComponent<IMouseInput>()?.OnMouseEnter();
                }

                //runs the Hover function from the IMouseInput interface
                tileHovered.transform.gameObject.GetComponent<IMouseInput>().Hover();

            }
                //tileHovered.transform.gameObject.GetComponent<MeshRenderer>().material = hoverColor;
        }
    }
}
