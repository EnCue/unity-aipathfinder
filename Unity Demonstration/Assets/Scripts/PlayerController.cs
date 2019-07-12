using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public static float moveSpeed = 3f;

	public int rotation;
    public Vector3 Orientation;
    private Vector2 initialPos;
    private Vector2 Destination;

    private Animator anim;
    public bool canMove;
    public bool playerMoving;

    private Rigidbody2D rbody;
    Vector2 velocity;

    private float timeDownX = 0.0f; // time which horizontal was pressed
    private float timeDownY = 0.0f; // time which vertical was pressed

    //Singleton pattern
    static PlayerController Instance; 

    void Awake()
    {
    	//Singleton Pattern
	
        //Checks if the instaniated object is a copy
        /* if (Instance != null)
        {
            //Commit suicide if is it a copy
            Destroy(this.gameObject);
            return; 
        }
        //Instance is set to be the singelton if not a copy
        Instance = this; 
		GameObject.DontDestroyOnLoad (this.gameObject);
		rotation = 90;*/
    }
	
    void Start()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
		//Get Input
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");
		if (canMove) 
		{
            //if key is pressed on Horizontal save time it was pressed
            if (inputX != 0)
            {
                if (timeDownX == 0.0f) //if there is no stored time for it
				timeDownX = Time.time;
            }
            else
            {
                timeDownX = 0.0f; // reset time if no button is being pressed
            }

            //if key is pressed on vertical save time it was pressed
            if (inputY != 0)
            {
                if (timeDownY == 0.0f) //if there is no stored time for it
                timeDownY = Time.time;
            }
            else
            {
                timeDownY = 0.0f; // reset time if no button is being pressed
            }
           
            //check which button was hit last to determin direction
            velocity = Vector2.zero;
			if (timeDownX > timeDownY)
            {
                velocity = Vector2.right * inputX;
            } else if (timeDownX < timeDownY) {
				velocity = Vector2.up * inputY;
			}

			if (inputX != 0 && inputY != 0) 
			{
				if (timeDownX == timeDownY) {
					inputX = 0;
				} else if (timeDownX > timeDownY) {
					inputY = 0;
				} else if (timeDownY > timeDownX) {
					inputX = 0;
				}
			}
			
			if (inputX > 0) {
				Orientation = Vector3.right;
				rotation = 0;
			} else if (inputX < 0) {
				Orientation = -Vector3.right;
				rotation = 180;
			}
			if (inputY > 0) {
				Orientation = Vector3.up;
				rotation = 90;
			} else if (inputY < 0) {
				Orientation = -Vector3.up;
				rotation = 270;
			}

            // Animations
            playerMoving = (Mathf.Abs(inputX) + Mathf.Abs(inputY)) > 0;
            anim.SetBool("PlayerMoving", playerMoving);

            if (playerMoving)
            {
				anim.SetFloat ("MoveX", inputX);
				anim.SetFloat ("MoveY", inputY);
            }

			rbody.MovePosition(rbody.position + (velocity * moveSpeed) * Time.fixedDeltaTime);
		}
    }
}
