﻿/**
/// PlayerStateMachine is the context class of the player controller, also known as the player controller.
/// This class inherits from NetworkBehaviour and passes on all NetworkBehaviour functionality to the concrete states.
@author: Sonny Selten
@date 19 april 2023
**/

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

//this class is the context class of the player controller. 
//also known als the player controller
//this class will inherit from network behavior and pass on all networkbehavior functionality to the concrete states

    public class PlayerStateMachine : MonoBehaviour
    {
        #region Configuration Fields (Set in Inspector)

        [Header("Movement Speeds")]
        [SerializeField] private float _walkSpeed = 2.0f;
        [SerializeField] private float _runSpeed = 6.0f;
        [SerializeField] private float _crouchSpeed = 1.0f; // Added for consistency

        [Header("Turning")]
        [SerializeField] [Range(0.0f, 0.3f)] private float _rotationSmoothTime = 0.1f;
        [SerializeField] private float _turnSpeed = 120f; // Example, might not be needed if only using SmoothDampAngle

        [Header("Acceleration")]
        [SerializeField] private float _speedChangeRate = 10.0f;
        // Removed AccelerationRate, DecelerationRate, TurnAcceleration, TurnInertiaFactor - simplify to SpeedChangeRate for now

        [Header("Jumping & Gravity")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _jumpGracePeriodDuration = 0.15f;
        [SerializeField] private float _gravityMultiplier = 1.0f; // Allows adjusting gravity intensity
        [SerializeField] private float _airControlFactor = 0.3f; // How much influence input has in air
        [SerializeField] private float _terminalVelocity = 53.0f;

        [Header("Ground Check")]
        [SerializeField] private float _groundedCheckOffset = 0.1f; // Offset for sphere/ray cast origin
        [SerializeField] private float _groundedCheckRadius = 0.3f;  // Radius for sphere cast
        [SerializeField] private LayerMask _groundLayerMask;        // Layers considered ground

        [Header("Camera")]
        [SerializeField] private GameObject _cinemachineCameraTarget;
        [SerializeField] private float _topClamp = 70.0f;
        [SerializeField] private float _bottomClamp = -30.0f;
        [SerializeField] private float _cameraAngleOverride = 0.0f;
        [SerializeField] private bool _lockCameraPosition = false;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursor = true;

        #endregion

        #region Public Properties (Accessible by States as Ctx.Property)

        // Core Components
        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public Transform CameraTransform { get; private set; }
        public bool HasAnimator { get; private set; }

        // Movement Config Accessors
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float RotationSmoothTime => _rotationSmoothTime;
        public float SpeedChangeRate => _speedChangeRate;
        public float JumpHeight => _jumpHeight;
        public float Gravity => Physics.gravity.y * _gravityMultiplier; // Use engine gravity * multiplier
        public float TerminalVelocity => _terminalVelocity;
        public float AirControlFactor => _airControlFactor;
        public float JumpGracePeriodDuration => _jumpGracePeriodDuration;

        // Current State Information
        public PlayerBaseState CurrentState { get; set; } // Managed by state transitions
        public Vector3 CurrentVelocity { get; set; } // Holds the velocity to be applied by Controller.Move
        public float TargetRotation { get; set; } // Target Y rotation angle
        public float RotationVelocity { get; set; } // Used by SmoothDampAngle ref
        public bool IsGrounded { get; private set; }
        public float JumpGracePeriodTimer { get; set; }

        // Input State (Read by states)
        public Vector2 MoveInput { get; private set; }
        public bool IsMovementPressed { get; private set; }
        public bool IsRunPressed { get; private set; }
        public bool IsJumpPressed { get; set; } // Allow states to consume (set to false)
        public bool IsCrouchPressed { get; set; } // Assuming toggle, managed by state/input


        // Animation Parameter IDs (Cached for performance)
        public int AnimIDSpeed { get; private set; }
        public int AnimIDGrounded { get; private set; }
        public int AnimIDJump { get; private set; }
        public int AnimIDFreeFall { get; private set; }
        public int AnimIDMotionSpeed { get; private set; }
        public int AnimIDCrouch { get; private set; }

        #endregion

        #region Private Fields

        // State Management
        private PlayerStateFactory _states;

        // Input Raw Values (from Input System callbacks)
        private Vector2 _rawLookInput;

        // Camera Rotation
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private const float _cameraThreshold = 0.01f;

        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            // Cache Components
            Controller = GetComponent<CharacterController>();
            PlayerTransform = transform;
            HasAnimator = TryGetComponent(out var animator);
            Animator = animator; // Assign animator even if null initially, states should check HasAnimator

            // Find Main Camera (Robustness: Check tag exists)
            if (Camera.main != null)
            {
                CameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Camera not found. Ensure a camera is tagged 'MainCamera'.", this);
            }

            // Cache Animator Parameter IDs
            AssignAnimationIDs();

            // Setup State Machine
            _states = new PlayerStateFactory(this);
            CurrentState = _states.Grounded(); // Start in Grounded state
            CurrentState.EnterState();

            // Initialize Camera Rotation
            if (_cinemachineCameraTarget != null)
            {
                _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
            }

            // Set Cursor Lock State
            SetCursorState(_lockCursor);
        }

        private void Update()
        {
            // Decrement Timers
            if (JumpGracePeriodTimer > 0)
            {
                JumpGracePeriodTimer -= Time.deltaTime;
            }

            // --- Delegate Core Logic to the Current State ---
            CurrentState.UpdateState();
            // -----------------------------------------------

            // Perform Ground Check (Using chosen method)
            GroundedCheck(); // Use SphereCast for now, can be swapped later

        }

        private void LateUpdate()
        {
            // Handle Camera Rotation
            CameraRotation();
        }

        #endregion

        #region Initialization

        private void AssignAnimationIDs()
        {
            AnimIDSpeed = Animator.StringToHash("Speed");
            AnimIDGrounded = Animator.StringToHash("Grounded");
            AnimIDJump = Animator.StringToHash("Jump");
            AnimIDFreeFall = Animator.StringToHash("FreeFall"); // Verify parameter name in Animator
            AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            AnimIDCrouch = Animator.StringToHash("Crouch");       // Verify parameter name in Animator
        }

        #endregion

        #region Ground Check

        // Using SphereCast method for now as per fields defined earlier
        private void GroundedCheck()
        {
            // Set sphere position, with offset
            Vector3 spherePosition = new Vector3(PlayerTransform.position.x, PlayerTransform.position.y - _groundedCheckOffset, PlayerTransform.position.z);
            bool physicallyGrounded = Physics.CheckSphere(spherePosition, _groundedCheckRadius, _groundLayerMask, QueryTriggerInteraction.Ignore);

            // Only set IsGrounded to true if physically grounded AND grace period is over
            IsGrounded = JumpGracePeriodTimer <= 0f && physicallyGrounded;

            // Update animator
            SetAnimatorGrounded(IsGrounded); // Use helper method
        }

        // Optional: Gizmo for visualizing the ground check sphere
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (IsGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // Draw sphere at position and matching radius
            Gizmos.DrawSphere(
                new Vector3(PlayerTransform.position.x, PlayerTransform.position.y - _groundedCheckOffset, PlayerTransform.position.z),
                _groundedCheckRadius);
        }

        #endregion

        #region Input System Callbacks

        // These methods just store the raw input. States will interpret these values.
        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
            IsMovementPressed = MoveInput.magnitude > 0.01f;
        }

        public void OnLook(InputValue value)
        {
            _rawLookInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            // Set the flag if pressed and grace period allows. States must consume it (set back to false).
            if (value.isPressed && JumpGracePeriodTimer <= 0f)
            {
                IsJumpPressed = true;
            }
            // Optional: Immediately set false on release? Or let states handle consumption?
            // else if (!value.isPressed) { IsJumpPressed = false; } // Might interfere with buffering
        }

        public void OnSprint(InputValue value)
        {
            IsRunPressed = value.isPressed;
        }

        public void OnCrouch(InputValue value)
        {
            // Example for a toggle crouch - State needs to handle the actual toggle logic
            if (value.isPressed)
            {
                 IsCrouchPressed = !IsCrouchPressed; // Simple toggle flag, state verifies if valid
            }
        }

        #endregion

        #region Camera

        private void CameraRotation()
        {
            if (_cinemachineCameraTarget == null) return; // Don't run if no target

            // If there is input and camera position is not fixed
            if (_rawLookInput.sqrMagnitude >= _cameraThreshold && !_lockCameraPosition)
            {
                // Adjust multiplier based on device if necessary (e.g., mouse vs controller sensitivity)
                float deltaTimeMultiplier = 1.0f; // Assuming mouse/consistent input rate

                _cinemachineTargetYaw += _rawLookInput.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _rawLookInput.y * deltaTimeMultiplier;
            }

            // Clamp rotations
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

            // Apply rotation to Cinemachine target
            _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            // Simplified clamping
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        #endregion

        #region Animator Helpers

        // Provide consistent way for states to update animator, includes HasAnimator check
        public void SetAnimatorBool(int animID, bool value)
        {
            if (HasAnimator) Animator.SetBool(animID, value);
        }

        public void SetAnimatorFloat(int animID, float value)
        {
            if (HasAnimator) Animator.SetFloat(animID, value);
        }

        public void SetAnimatorTrigger(int animID)
        {
             if (HasAnimator) Animator.SetTrigger(animID);
        }

        // Specific helpers can still exist if preferred
        public void SetAnimatorGrounded(bool grounded) => SetAnimatorBool(AnimIDGrounded, grounded);
        public void SetAnimatorJump(bool jump) => SetAnimatorBool(AnimIDJump, jump);
        public void SetAnimatorFreeFall(bool freeFall) => SetAnimatorBool(AnimIDFreeFall, freeFall);
        public void SetAnimatorCrouch(bool crouch) => SetAnimatorBool(AnimIDCrouch, crouch);

        #endregion

        #region Cursor Management

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(_lockCursor && hasFocus); // Lock when focused, unlock when not (optional)
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        #endregion
    }
