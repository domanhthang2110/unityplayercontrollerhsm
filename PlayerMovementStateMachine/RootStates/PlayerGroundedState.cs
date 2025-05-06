using UnityEngine;

    public class PlayerGroundedState : PlayerBaseState
    {
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) //passed arguments to base state constructor
        {
            IsRootState = true;
            
        }

        //defines the onEnter function of a state machine
        public override void EnterState()
        {
            //Debug.Log("current player super state: GROUNDED");
            InitializeSubState();
            Ctx.TargetSpeed = 0;
        }

        //defines the onDo function of a state machine
        public override void UpdateState()
        {
            HandleGroundedMovement();
            ApplyGroundedGravity();
            CheckSwitchStates();     
        }

        public override void FixedUpdateStates()
        {
        }

        //defines the onExit function of a state machine
        public override void ExitState()
        {
        }

        //Condition to check when to switch to a new state 
        public override void CheckSwitchStates()
        {   
            //Dang dung > nhan ctrl de ngoi
            if (Ctx.IsCrouchPressed && CurrentSubState.GetType() != typeof(PlayerCrouchState))
            {
                SwitchSubState(Factory.Crouch());           
            }
            //Dang ngoi > nhan nhay de dung day
            if (Ctx.IsJumpPressed && CurrentSubState.GetType() == typeof(PlayerCrouchState))
            {
                Ctx.IsCrouchPressed = !Ctx.IsCrouchPressed;
                Debug.LogWarning("Ctx.IsCrouchPressed: " + Ctx.IsCrouchPressed);
                SwitchState(Factory.Grounded());
                Ctx.IsJumpPressed = false;
            }
            if (Ctx.IsJumpPressed && CurrentSubState.GetType() != typeof(PlayerCrouchState))
            {
                // Set the jump grace timer when initiating a jump
                Ctx.JumpGracePeriodTimer = Ctx.JumpGracePeriodDuration; 
                SwitchState(Factory.Air());
                Ctx.IsJumpPressed = false; // Consume jump input
            }
            else if (!Ctx.IsGrounded) // Separate check for just leaving the ground without jumping
            {
                // Don't set the grace timer if just falling off an edge
                SwitchState(Factory.Air());
            }
        }

        //defines which state to intialise when switching substates    
        public override void InitializeSubState()
        {
            if (Ctx.IsMovementPressed)
            {
                SetSubState(Factory.Walk());
            }
            else if (Ctx.IsRunPressed && Ctx.IsMovementPressed)
            {
                SetSubState(Factory.Run());
            }
            else
            {
                SetSubState(Factory.Idle());
            }
        }

        private void ApplyGroundedGravity()
        {
            if (Ctx.VerticalVelocity < -4f)  // Prevent infinite falling
            {
                Ctx.VerticalVelocity = -4f;
            }
            // Apply gravity but less intense than in air
            Ctx.VerticalVelocity += PlayerStateMachine.Gravity * Time.deltaTime;
        }

private void HandleGroundedMovement()
{
    // Determine target speed based on input magnitude, not actual velocity
    float targetSpeed = Ctx.MoveInput.magnitude > 0 ? Ctx.TargetSpeed : 0f;
    
    // Instead of using current controller velocity, directly update speed based on input
    if (Ctx.MoveInput.magnitude > 0)
    {
        // If there's input, accelerate toward target speed
        Ctx.Speed = Mathf.Lerp(Ctx.Speed, targetSpeed, Time.deltaTime * Ctx.SpeedChangeRate);
    }
    else
    {
        // If no input, decelerate to zero
        Ctx.Speed = Mathf.Lerp(Ctx.Speed, 0f, Time.deltaTime * Ctx.SpeedChangeRate);
    }
    
    // Round for precision
    Ctx.Speed = Mathf.Round(Ctx.Speed * 1000f) / 1000f;

    // Animation blending
    Ctx.AnimationBlend = Mathf.Lerp(Ctx.AnimationBlend, targetSpeed, Time.deltaTime * Ctx.SpeedChangeRate);

    // Movement & Rotation
    Vector3 inputDirection = new Vector3(Ctx.MoveInput.x, 0, Ctx.MoveInput.y).normalized;

    if (inputDirection != Vector3.zero)
    {
        float rotationVelocity = Ctx.RotationVelocity;
        Ctx.TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Ctx.MainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(Ctx.transform.eulerAngles.y, Ctx.TargetRotation, ref rotationVelocity, Ctx.RotationSmoothTime);
        Ctx.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    Vector3 targetDirection = Quaternion.Euler(0, Ctx.TargetRotation, 0) * Vector3.forward;

    // Calculate movement vector
    Vector3 movement = targetDirection.normalized * (Ctx.Speed * Time.deltaTime) +
                      new Vector3(0.0f, Ctx.VerticalVelocity, 0.0f) * Time.deltaTime;

    // Try to move and check for collisions
    CollisionFlags collisionFlags = Ctx.Controller.Move(movement);

    // If we hit a wall, reduce speed
    if ((collisionFlags & CollisionFlags.Sides) != 0)
    {
        Ctx.Speed *= 0.965f; // Reduce speed by 5%
    }

    // Animator
    if (Ctx.HasAnimator)
    {
        Ctx.Animator.SetFloat(Ctx.AnimIDSpeed, Ctx.AnimationBlend);
        Ctx.Animator.SetFloat(Ctx.AnimIDMotionSpeed, Ctx.MoveInput.magnitude);
    }
}

    }
