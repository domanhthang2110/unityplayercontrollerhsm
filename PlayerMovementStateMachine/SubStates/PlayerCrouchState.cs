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

public class PlayerCrouchState : PlayerBaseState
{

    /// Constructor for PlayerRunState. Initializes base state with provided context and factory.
    /// <param name="currentContext">The PlayerStateMachine instance that holds the state machine's context.</param>
    /// <param name="playerStateFactory">The PlayerStateFactory instance used to create new states.</param>
    public PlayerCrouchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) //passed arguments to base state constructor
    {

    }
    //defines the onEnter function of a state machine
    public override void EnterState()
    {
        Debug.Log("current playerstate: CROUCH");
        Debug.Log("Crouch: " + Ctx.IsCrouchPressed + " " + Ctx.IsCrouching);
        Ctx.SetAnimatorCrouch(true);
        Ctx.TargetSpeed = 1f;
        Ctx.AccelerationRate = 6f;
        Ctx.DecelerationRate = 6f;
        }

    //defines the onDo function of a state machine
    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    //defines the onExit function of a state machine
    public override void ExitState()
    {
        Debug.LogWarning("Exiting crouch");
        Ctx.SetAnimatorCrouch(false);
    }

    //Condition to check when to switch to a new state 
    public override void CheckSwitchStates()
    {
        Debug.LogWarning("Is crouch pressed: " + Ctx.IsCrouchPressed + " Is crouching: " + Ctx.IsCrouching);
        if (!Ctx.IsCrouchPressed || Ctx.IsJumpPressed)
        {
            Ctx.IsJumpPressed = false;
            SwitchState(Factory.Grounded());    
        }
    }

    //defines which state to intialise when switching substates    
    public override void InitializeSubState()
    {
        //TODO
    }

    ////member function

}

