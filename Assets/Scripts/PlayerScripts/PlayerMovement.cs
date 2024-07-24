using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;

    private Vector2 movement;

    private Rigidbody2D rb; 

   private void Awake() 
   {
        rb  = GetComponent< Rigidbody2D> ();

   }

    // Update is called once per frame
    private void Update()
    {
        movement.Set(PlayerInputManger.Movement.x, PlayerInputManger.Movement.y);

        rb.velocity = movement * movementSpeed;
    }

}
