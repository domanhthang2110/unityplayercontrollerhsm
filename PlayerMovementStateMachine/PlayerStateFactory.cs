using System.Collections.Generic;

public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    // Constructor requires the context (PlayerStateMachine)
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    // --- Methods to create instances of each state ---
    // We pass the context and this factory to each state's constructor

    public PlayerBaseState Grounded() 
    {
        return new PlayerGroundedState(_context, this); // Assumes PlayerGroundedState exists
    }

    public PlayerBaseState Air()
    {
        return new PlayerAirState(_context, this); // Assumes PlayerAirState exists
    }

    public PlayerBaseState Idle()
    {
        return new PlayerIdleState(_context, this); // Assumes PlayerIdleState exists
    }

    public PlayerBaseState Walk()
    {
        return new PlayerWalkState(_context, this); // Assumes PlayerWalkState exists
    }

    public PlayerBaseState Run()
    {
        return new PlayerRunState(_context, this); // Assumes PlayerRunState exists
    }

    public PlayerBaseState Jump()
    {
        return new PlayerJumpState(_context, this); // Assumes PlayerJumpState exists
    }

    public PlayerBaseState Fall()
    {
        return new PlayerFallState(_context, this); // Assumes PlayerFallState exists
    }
    
    public PlayerBaseState Crouch()
    {
        // Ensure PlayerCrouchState class exists if you uncomment this
        // return new PlayerCrouchState(_context, this); 
        return null; // Placeholder if Crouch is not implemented yet
    }

    // Add methods for any other states you might need
}

