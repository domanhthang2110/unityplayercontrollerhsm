/**
 The PlayerJumpState class represents the jumping state of the player in a finite state machine.
 This state is activated when the player initiates a jump.
@note: also see PlayerBaseState.cs
@author: Sonny Selten
@date 19 april 2023
**/

using UnityEngine;

    public class PlayerFallState : PlayerBaseState
    {
        /// Constructor for PlayerJumpState. Initializes base state with provided context and factory.
        /// <param name="currentContext">The PlayerStateMachine instance that holds the state machine's context.</param>
        /// <param name="playerStateFactory">The PlayerStateFactory instance used to create new states.</param>
        public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) //passed arguments to base state constructor
        {
        }

        //defines the onEnter function of a state machine
        public override void EnterState()
        {
            Debug.LogWarning("current playerstate: FALLING");
            Ctx.SetAnimatorFreeFall(true);
        }

        //defines the onDo function of a state machine
        public override void UpdateState()
        {
            //ApplyFallGravity();
            CheckSwitchStates();
        }

        public override void FixedUpdateStates()
        {
        }

        //defines the onExit function of a state machine
        public override void ExitState()
        {
            Ctx.SetAnimatorFreeFall(false);
            //Debug.LogWarning("Exiting Fall Stateeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
        }

        //Condition to check when to switch to a new state 
        public override void CheckSwitchStates()
        {
        }

        //defines which state to intialise when switching substates    
        public override void InitializeSubState()
        {
        }

        //-------------------------------------------------------------------//
    }
