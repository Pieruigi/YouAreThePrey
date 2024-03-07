using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace YATP
{
    public enum PlayerState { Normal, Hidden, Dead, Busy }

    public class PlayerController : MonoBehaviour
    {
        public UnityAction<float, float> OnStaminaChanged;


        public static PlayerController Instance { get; private set; }

        [SerializeField]
        float sprintSpeed;

        [SerializeField]
        float runSpeed;

        [SerializeField]
        float walkSpeed;

        [SerializeField]
        float crouchSpeed;

        [SerializeField]
        float acceleration;

        [SerializeField]
        float deceleration;

        [SerializeField]
        float maxPitch;

        [SerializeField]
        float minPitch;

        [SerializeField]
        float mouseSensitivity;

        [SerializeField]
        Camera playerCamera;
        public Camera Camera
        {
            get { return playerCamera; }
        }

        [SerializeField]
        float crouchHeight;

        [SerializeField]
        float maxStamina;
        
        [SerializeField]
        float staminaDepleteSpeed;

        [SerializeField]
        float standStaminaRecoilSpeed;

        [SerializeField]
        float walkStaminaRecoilSpeed;

        [SerializeField]
        float crouchStaminaRecoilSpeed;

        [SerializeField]
        float runStaminaRecoilSpeed;

        [SerializeField]
        float staminaRecoilDelay;

        float currentStamina;
        

        float staminaRecoilTime = 0;


        CharacterController characterController;
        Vector3 velocity = Vector3.zero;
        Vector2 moveInput = Vector2.zero;
        Vector2 aimInput = Vector2.zero;
        
        float pitch = 0;
        float yaw = 0;
        bool walkInput = false;
        bool sprintInput = false;
        bool sprintInputLast = false;
        bool crouchInput = false;
        
        float characterDefaultHeight;
        float cameraDefaultHeight;
        float cameraCrouchHeight;

        //bool dead = false;
        //public bool Dead
        //{
        //    get { return dead; }
        //}
        //bool paused = false;
        //public bool Paused
        //{
        //    get { return paused; }
        //}
        PlayerState state;
        public PlayerState State
        {
            get { return state; }
        }
       
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                characterController = GetComponent<CharacterController>();
                characterDefaultHeight = characterController.height;
                cameraDefaultHeight = playerCamera.transform.parent.localPosition.y;
                float crouchMul = crouchHeight / characterDefaultHeight;
                cameraCrouchHeight = cameraDefaultHeight * crouchMul;
                //currentStamina = maxStamina;
                SetStamina(maxStamina);
            }
            else
            {
                Destroy(gameObject);
            }
            
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
           UpdateState();
            
        }

        void UpdateState()
        {
            switch(state)
            {
                case PlayerState.Normal:
                    UpdateNormalState();
                    break;
            }
        }

        void UpdateNormalState()
        {
            CheckInput();

            Rotate();

            Crouch();

            Move();

            UpdateStamina();
        }

        void UpdateStamina()
        {
            if(sprintInput)
            {
                SetStamina(currentStamina - Time.deltaTime * staminaDepleteSpeed);
            }
            else // Not sprinting
            {
                if (sprintInputLast) // Was sprinting the previous frame
                {
                    staminaRecoilTime = staminaRecoilDelay;
                }
                else
                {
                    if(currentStamina < maxStamina)
                    {
                        if (staminaRecoilTime > 0)
                        {
                            staminaRecoilTime -= Time.deltaTime;
                        }
                        else
                        {
                            float speed = GetStaminaRecoilSpeed();
                            SetStamina(currentStamina + Time.deltaTime * speed);
                        }
                    }
                    
                }
            }
        }

        float GetStaminaRecoilSpeed()
        {
            if (moveInput.magnitude == 0)
            {
                return standStaminaRecoilSpeed;
            }
            else
            {
                if (walkInput)
                    return walkStaminaRecoilSpeed;
                if (crouchInput)
                    return crouchStaminaRecoilSpeed;
                if (sprintInput)
                    return 0;
                return runStaminaRecoilSpeed;
            }

        }

        private void CheckInput()
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            aimInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            crouchInput = Input.GetAxis("Fire1") > 0;
            walkInput = !crouchInput && Input.GetAxis("Fire3") > 0;
            sprintInputLast = sprintInput;
            sprintInput = currentStamina > 0 && !crouchInput && !walkInput && Input.GetAxis("Jump") > 0;
            
        }

        void Rotate()
        {
            yaw += aimInput.x * mouseSensitivity;
            pitch += -aimInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Crouch()
        {
            if (crouchInput)
            {
                float cSpeed = 2;
                if (characterController.height > crouchHeight)
                {
                    characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, Time.deltaTime * cSpeed);
                    Vector3 collCenter = characterController.center;
                    collCenter.y = characterController.height * .5f;
                    characterController.center = collCenter;
                }

                Vector3 pos = playerCamera.transform.parent.localPosition;
                if (pos.y > cameraCrouchHeight)
                {
                    pos.y = Mathf.MoveTowards(pos.y, cameraCrouchHeight, Time.deltaTime * cSpeed);
                    playerCamera.transform.parent.localPosition = pos;
                }



            }
            else
            {
                float cSpeed = 2;
                if (characterController.height < characterDefaultHeight)
                {
                    characterController.height = Mathf.MoveTowards(characterController.height, characterDefaultHeight, Time.deltaTime * cSpeed);
                    Vector3 collCenter = characterController.center;
                    collCenter.y = characterController.height * .5f;
                    characterController.center = collCenter;
                }

                Vector3 pos = playerCamera.transform.parent.localPosition;
                if (pos.y < cameraDefaultHeight)
                {
                    pos.y = Mathf.MoveTowards(pos.y, cameraDefaultHeight, Time.deltaTime * cSpeed);
                    playerCamera.transform.parent.localPosition = pos;
                }



            }
        }

        void Move()
        {
         
            Vector3 hVel = velocity;
            hVel.y = 0;
            float vVel = velocity.y;
            float speed = hVel.magnitude;

            Vector3 targetDirection = Vector3.zero;
            if (moveInput.magnitude > 0) 
            {
                float hMaxSpeed = GetMoveSpeed();

                speed += Time.deltaTime * acceleration;
                if(speed >hMaxSpeed)
                    speed = hMaxSpeed;

                if(!sprintInput)
                    targetDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
                else
                    targetDirection = transform.forward;
            }
            else
            {
                // Keep the last direction while decelerating
                if(speed > 0)
                {
                    targetDirection = velocity;
                    targetDirection.y = 0;
                    targetDirection.Normalize();

                    speed -= Time.deltaTime * deceleration;
                    if (speed < 0)
                        speed = 0;
                }
                
            }

            hVel = targetDirection * speed;

         
            // 
            // Apply gravity
            //
            if (!IsGrounded())
            {
                RaycastHit hit;
                float disp = .2f;
                if(Physics.Raycast(transform.position+Vector3.up*characterController.skinWidth, Vector3.down, disp, LayerMask.GetMask(new string[] { "Floor" })))
                {
                    vVel = 0;
                    characterController.Move(Vector3.down * disp);
                }
                else
                {
                    vVel += Physics.gravity.y * Time.deltaTime;
                }
              
                                  
            }
            else
            {
                vVel = 0;
            }
            

            // 
            // Move character
            //
            velocity = hVel + Vector3.up * vVel;

            characterController.Move(velocity * Time.deltaTime);
        }

        float GetMoveSpeed()
        {
            if(moveInput.magnitude == 0)
                return 0;

            if (walkInput)
                return walkSpeed;

            if (crouchInput)
                return crouchSpeed;

            if(sprintInput) 
                return sprintSpeed;

            return runSpeed;
        }

        bool IsGrounded()
        {
            return Physics.OverlapSphere(transform.position + Vector3.up * (characterController.radius-characterController.skinWidth), characterController.radius, LayerMask.GetMask(new string[] { "Floor" })).Length > 0;
        }

        void SetStamina(float value)
        {
            currentStamina = value;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        void SetState(PlayerState newState)
        {
            if(state == newState) return;

            PlayerState oldState = state;
            state = newState;
        }

        public void Die()
        {
            Debug.Log("You are dying...");
            SetState(PlayerState.Dead);
            
        }
    }

    

}
