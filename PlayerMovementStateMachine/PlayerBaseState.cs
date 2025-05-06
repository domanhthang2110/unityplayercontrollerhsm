/**
@brief Abstract base class for all player states.
This class defines the common functionality and properties that all player states should have.
It is designed to be extended by concrete subclasses that implement the specific behavior for each player state.
@note Concrete subclasses that inherit from PlayerBaseState must implement the
abstract methods EnterState(), UpdateState(), ExitState(), CheckSwitchStates(), and InitializeSubstate().
@author: Sonny Selten
@date: 13 april 2023
**/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public abstract class PlayerBaseState
    {
        ////state variables
        //@brief Private boolean variable that indicates whether the state is a root state.
        private bool _isRootState = false; // default value

        //@brief The context of the player state machine.
        //This is the state machine that the player state is currently running in.
        private PlayerStateMachine _ctx; //context, short because otherwise it will conflix with nametype. 

        //@brief The factory that creates player states.
        private PlayerStateFactory _factory;


        //@brief The current substate of the player state machine.
        //This is the current substate that the player state is running in, if any.
        private PlayerBaseState _currentSubState;


        //@brief The current superstate of the player state machine.
        //This is the current superstate that the player state is running in, if any.
        private PlayerBaseState _currentSuperState;


        ////getters and setters
        protected bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }

        protected PlayerStateMachine Ctx { get { return _ctx; } set { _ctx = value; } }
        protected PlayerStateFactory Factory { get { return _factory; } set { _factory = value; } }
        public PlayerBaseState CurrentSubState { get { return _currentSubState; } set { _currentSubState = value; } }
        protected PlayerBaseState CurrentSuperState { get { return _currentSuperState; } set { _currentSuperState = value; } }

        ////functions

        ///@brief Constructor for PlayerBaseState.
        ///<param name="currentContext"> currentContext The context of the player state machine.
        ///<param name="playerStateFactory"> playerStateFactory The factory that creates player states.

        public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        {
            _ctx = currentContext;
            _factory = playerStateFactory;
        }


        //@brief Abstract method that defines the behavior for entering the player state.
        public abstract void EnterState();

        //@brief Abstract method that defines the behavior for executing/updating the player state.
        public abstract void UpdateState();

        //@brief Abstract method that defines the behavior for exiting the player state.
        public abstract void ExitState();

        //@brief Abstract method that defines the behavior for switching the player state.
        public abstract void CheckSwitchStates();

        //@brief Abstract method that defines the behavior for running the player sub-state.   
        public abstract void InitializeSubState();


        ///@brief entry function of state machine that checks if it is a root state and if so, delegates the entry call down the hierarchy
        public void EnterStates()
        {
            EnterState();
            if (_currentSubState != null)
            {
                _currentSubState.EnterStates();
            }
        }

        ///@brief do function of state machine that checks if it is a root state and if so, delegates the entry call down the hierarchy
        public void UpdateStates()
        {
            if (this.IsRootState)
            {
                Debug.LogWarning("Updating state: " + this.GetType().Name + " and current substate: " + _currentSubState?.GetType().Name);
            }
            UpdateState();
            if (_currentSubState != null)
            {
                _currentSubState.UpdateStates();
            }
        }

        ///@brief exit function of state machine that checks if it is a root state and if so, delegates the entry call down the hierarchy
        public void ExitStates()
        {
            ExitState();
            if(IsRootState)
            {
                //Debug.LogWarning("Exiting state: " + this.GetType().Name + " and current substate: " + _currentSubState?.GetType().Name);
            }
            if (_currentSubState != null)
            {
                _currentSubState.ExitStates(); 
            }
        }

        ///@brief state machine function to switch to the state passed on the parameter
        /// <param name="newState">defines the new state to switch to.</param>
        protected void SwitchState(PlayerBaseState newState)
        {
            //exit function of current state
            ExitStates();
            if (newState.IsRootState && _currentSuperState?.GetType() != newState.GetType())
            {
                _currentSuperState?.ExitStates();
            }

            //entry function of new state
            newState.EnterStates();
            
            if (newState.IsRootState)
            {
                //switch current state of context
                _ctx.CurrentState = newState;
            }
            else if (_currentSuperState != null)
            {
                //set the current super states sub state to the new state
                _currentSuperState.SetSubState(newState);
            }
        }

        ///@brief Switches the current substate while properly exiting the old substate and entering the new one
        protected void SwitchSubState(PlayerBaseState newSubState)
        {
            // If there's an existing substate, exit it first
            if (_currentSubState != null)
            {
                _currentSubState.ExitStates();
            }

            // Set the new substate
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);

            // Enter the new substate
            _currentSubState.EnterStates();
        }


        ///@brief function to set the parent/superstate of a child/concrete state
        /// <param name="newSuperState">defines the new parent state to switch to.</param>
        private void SetSuperState(PlayerBaseState newSuperState)
        {
            _currentSuperState = newSuperState;
        }

        ///@brief function to set the child/concrete state of a parent/superstate
        /// <param name="newSubState">defines the new state child to switch to.</param>
        protected void SetSubState(PlayerBaseState newSubState)
        {
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);
        }


        public virtual void FixedUpdateStates()
        {
        }
    }