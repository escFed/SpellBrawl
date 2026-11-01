using UnityEngine;

public class JumpState : CharacterState
{
    private enum VisualPhase { None, Takeoff, AirTakeoff, Rise, Apex, Fall, Land }

    private float airTimer;
    private float landingTimer;
    private bool skipForceOnEnter;
    private bool phasedAnimation;
    private VisualPhase visualPhase;

    private const float GroundTakeoffTime = 0.11f;
    private const float AirTakeoffTime = 0.20f;
    private const float LandingTime = 0.067f;
    private const float VerticalPoseThreshold = 1.25f;

    public JumpState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public void PrepareReentry() => skipForceOnEnter = true;

    public bool TryPerformBufferedJump()
    {
        bool isGroundJump = character.CanGroundJump;
        bool isWallJump = !isGroundJump && character.CanWallJump;
        if (!character.TryPerformJump())
            return false;

        airTimer = 0f;
        landingTimer = 0f;
        if (phasedAnimation)
            PlayTakeoff(isGroundJump, isWallJump);
        return true;
    }

    public override void Enter()
    {
        base.Enter();
        airTimer = 0f;
        landingTimer = 0f;
        visualPhase = VisualPhase.None;
        phasedAnimation = character.Animation.HasState("AirJump") &&
            character.Animation.HasState("JumpRise") &&
            character.Animation.HasState("JumpApex") &&
            character.Animation.HasState("JumpFall") &&
            character.Animation.HasState("JumpLand");

        if (skipForceOnEnter)
        {
            skipForceOnEnter = false;
            if (phasedAnimation)
                UpdateAirPose();
            else
                character.Animation.TryPlay("Jump");
        }
        else
        {
            bool isGroundJump = character.CanGroundJump;
            bool isWallJump = !isGroundJump && character.CanWallJump;
            if (character.TryPerformJump() && phasedAnimation)
                PlayTakeoff(isGroundJump, isWallJump);
            else
                character.Animation.TryPlay("Jump");
        }
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        airTimer += Time.deltaTime;
        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        character.HandleAirborneMovementInput();

        if (character.JumpPressed && character.CanJump)
        {
            TryPerformBufferedJump();
            return;
        }

        if (airTimer > 0.1f && character.IsGrounded)
        {
            if (!phasedAnimation)
            {
                ExitToGroundMovement();
                return;
            }

            if (visualPhase != VisualPhase.Land)
            {
                visualPhase = VisualPhase.Land;
                landingTimer = 0f;
                character.Animation.TryPlay("JumpLand");
            }
            else
            {
                landingTimer += Time.deltaTime;
                if (landingTimer >= LandingTime)
                    ExitToGroundMovement();
            }
            return;
        }

        if (phasedAnimation &&
            (visualPhase != VisualPhase.Takeoff || airTimer >= GroundTakeoffTime) &&
            (visualPhase != VisualPhase.AirTakeoff || airTimer >= AirTakeoffTime))
            UpdateAirPose();
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
        character.Movement.ClampFallSpeed();
    }

    private void PlayTakeoff(bool isGroundJump, bool isWallJump)
    {
        if (isGroundJump)
        {
            visualPhase = VisualPhase.Takeoff;
            character.Animation.TryPlay("Jump");
        }
        else if (isWallJump)
        {
            visualPhase = VisualPhase.Rise;
            character.Animation.TryPlay("JumpRise");
        }
        else
        {
            visualPhase = VisualPhase.AirTakeoff;
            character.Animation.TryPlay("AirJump");
        }
    }

    private void UpdateAirPose()
    {
        float speed = character.Movement.OrdinaryVelocity.y;
        VisualPhase next = speed > VerticalPoseThreshold ? VisualPhase.Rise :
            speed < -VerticalPoseThreshold ? VisualPhase.Fall : VisualPhase.Apex;
        if (next == visualPhase)
            return;

        visualPhase = next;
        character.Animation.TryPlay(next == VisualPhase.Rise ? "JumpRise" :
            next == VisualPhase.Fall ? "JumpFall" : "JumpApex");
    }

    private void ExitToGroundMovement()
    {
        stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ?
            character.States.Move : character.States.Idle);
    }
}
