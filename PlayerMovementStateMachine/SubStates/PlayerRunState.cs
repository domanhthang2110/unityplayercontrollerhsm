/**
The PlayerRunState class represents the running state of the player in a finite state machine.
This state is activated when the player is moving.
@note: also see PlayerBaseState.cs
@author: Sonny Selten
@date 19 april 2023
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PlayerRunState : PlayerBaseState
    {

        /// Constructor for PlayerRunState. Initializes base state with provided context and factory.
        /// <param name="currentContext">The PlayerStateMachine instance that holds the state machine's context.</param>
        /// <param name="playerStateFactory">The PlayerStateFactory instance used to create new states.</param>
        public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) //passed arguments to base state constructor
        {

        }
        //defines the onEnter function of a state machine
        public override void EnterState()
        {
            //Debug.Log("current playerstate: RUN");
            //Ctx.TargetSpeed = Ctx.RunSpeed;
            Ctx.TargetSpeed = 6f;  // RUN SPEED
            Ctx.AccelerationRate = 10f;
            Ctx.DecelerationRate = 8f;
        }

        //defines the onDo function of a state machine
        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        //defines the onExit function of a state machine
        public override void ExitState()
        {
        }

        //Condition to check when to switch to a new state 
        public override void CheckSwitchStates()
        {
            if (!Ctx.IsRunPressed && !Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else if (!Ctx.IsRunPressed && Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Walk());
            }
        }

        //defines which state to intialise when switching substates    
        public override void InitializeSubState()
        {
        }

        ////member function

    }

