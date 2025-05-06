using UnityEngine;

    public class PlayerAirState : PlayerBaseState
    {
        private Vector3 airMomentum; // Stores horizontal momentum for smoother airborne movement

        public PlayerAirState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            IsRootState = true;
            
        }

        public override void EnterState()
        {
            //Debug.Log("Current player super state: AIR");
            InitializeSubState();

            // Preserve momentum from the previous state
            airMomentum = new Vector3(Ctx.Controller.velocity.x, 0.0f, Ctx.Controller.velocity.z);

            // Ensure TargetSpeed carries over correctly
            Ctx.TargetSpeed = Mathf.Max(airMomentum.magnitude, Ctx.TargetSpeed);

            Debug.Log("Target Speed at Air Entry: " + Ctx.TargetSpeed);
        }

        public override void UpdateState()
        {
            HandleAirMovement();
            CheckSwitchStates();
            
        }

        public override void FixedUpdateStates()
        {
        }

        public override void ExitState()
        {
            //Debug.LogWarning("Exiting Air State");
        }

        public override void CheckSwitchStates()
        {
            if (Ctx.IsGrounded)
            {
                SwitchState(Factory.Grounded());
            }
        }

        public override void InitializeSubState()
        {
            if (Ctx.IsJumpPressed)
            {
                Debug.LogWarning("Jump pressed and air state");
                SetSubState(Factory.Jump());
            }
            else
            {
                SetSubState(Factory.Fall());
            }
        }

        private void HandleAirMovement()
        {
            // Apply gravity
            if (Ctx.VerticalVelocity < Ctx.TerminalVelocity)
            {
                Ctx.VerticalVelocity += PlayerStateMachine.Gravity * Time.deltaTime;
            }

            // Calculate target speed based on input magnitude (same as grounded)
            float targetSpeed = Ctx.TargetSpeed;
            float inputMagnitude = Ctx.MoveInput.magnitude;

            // Smooth acceleration/deceleration (same as grounded)
            float currentHorizontalSpeed = new Vector3(Ctx.Controller.velocity.x, 0, Ctx.Controller.velocity.z).magnitude;

            if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > 0.1f)
            {
                Ctx.Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * Ctx.SpeedChangeRate);
                Ctx.Speed = Mathf.Round(Ctx.Speed * 1000f) / 1000f;
            }
            else
            {
                Ctx.Speed = targetSpeed * inputMagnitude;
            }

            // Animation blending (same as grounded)
            Ctx.AnimationBlend = Mathf.Lerp(Ctx.AnimationBlend, targetSpeed * inputMagnitude, Time.deltaTime * Ctx.SpeedChangeRate);
            if (Ctx.AnimationBlend < 0.01f) Ctx.AnimationBlend = 0f;

            // Rotation handling (same as grounded but with air control factor)
            Vector3 inputDirection = new Vector3(Ctx.MoveInput.x, 0, Ctx.MoveInput.y).normalized;

            if (inputDirection != Vector3.zero)
            {
                float rotationVelocity = Ctx.RotationVelocity;
                Ctx.TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Ctx.MainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(
                    Ctx.transform.eulerAngles.y,
                    Ctx.TargetRotation,
                    ref rotationVelocity,
                    Ctx.RotationSmoothTime * (1 + (1 - Ctx.AirControlFactor)) // Slower rotation in air
                );
                Ctx.transform.rotation = Quaternion.Euler(0, rotation, 0);
            }

            // Movement direction (same as grounded but with air control)
            Vector3 targetDirection = Quaternion.Euler(0, Ctx.TargetRotation, 0) * Vector3.forward;

            // Apply air control factor to movement
            Vector3 horizontalMovement = targetDirection.normalized * (Ctx.Speed * Time.deltaTime);
            horizontalMovement = Vector3.Lerp(
                new Vector3(Ctx.Controller.velocity.x, 0, Ctx.Controller.velocity.z) * Time.deltaTime,
                horizontalMovement,
                Ctx.AirControlFactor
            );

            // Combine horizontal and vertical movement
            Vector3 verticalMovement = new Vector3(0.0f, Ctx.VerticalVelocity, 0.0f) * Time.deltaTime;
            Ctx.Controller.Move(horizontalMovement + verticalMovement);

            // Update Animator
            if (Ctx.HasAnimator)
            {
                Ctx.Animator.SetFloat(Ctx.AnimIDSpeed, Ctx.AnimationBlend);
                Ctx.Animator.SetFloat(Ctx.AnimIDMotionSpeed, inputMagnitude);
            }
        }
    }
