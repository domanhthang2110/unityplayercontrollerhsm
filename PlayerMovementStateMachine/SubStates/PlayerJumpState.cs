using UnityEngine;

    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
        }

        public override void EnterState()
        {
            Debug.LogWarning("current playerstate: JUMPING");
            
            Ctx.VerticalVelocity = Mathf.Sqrt(Ctx.JumpHeight * -2f * PlayerStateMachine.Gravity);
            Ctx.SetAnimatorJump(Ctx.VerticalVelocity > 0);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateStates()
        {
        }

        public override void ExitState()
        {

            Ctx.SetAnimatorJump(false);
            Ctx.IsJumpPressed = false;
            Debug.LogWarning("Exiting Jump Stateeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");

        }

        public override void CheckSwitchStates()
        {
            if (Ctx.VerticalVelocity <= 0)
            {
                SwitchState(Factory.Fall());
            }
        }

        public override void InitializeSubState()
        {
        }
    }