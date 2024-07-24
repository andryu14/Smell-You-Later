using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInputManger : MonoBehaviour
{
    public static Vector2 Movement;

    private PlayerInput playersInput;

    private InputAction move; 


    private void Awake()
    {
        playersInput = GetComponent<PlayerInput>();

        move = playersInput.actions["Move"]; 
    }

   
    private void Update()
    {
        Movement = move.ReadValue<Vector2>(); 
    }
}
