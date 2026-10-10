using UnityEngine;

public sealed class CharacterActionRouter
{
    private readonly CharacterCoordinator character;
    private readonly CharacterStateMachine machine;
    private readonly CharacterDeck deck;
    private readonly CharacterParry parry;

    private CharacterStates States => character.States;

    public CharacterActionRouter(CharacterCoordinator character, CharacterDeck deck, CharacterParry parry)
    {
        this.character = character;
        this.deck = deck;
        this.parry = parry;
        machine = character.stateMachine;
    }

    public bool TryHandlePriorityInputs()
    {
        IInputProvider input = character.ActiveInput;
        if (input == null)
            return false;

        // The first accepted action wins this frame.
        if (TryHandleShield(input))
            return true;

        if (!machine.Is(States.HeavyAttack))
            character.Combat.FaceDirection(character.MoveInput.x);

        if (TryHandleDash(input))
            return true;
        if (TryHandleEvade(input))
            return true;
        if (TryHandleHeavyAttack(input))
            return true;
        if (TryHandleParry(input))
            return true;

        return TryHandleCards(input);
    }

    private bool TryHandleShield(IInputProvider input)
    {
        if (input.HasBufferedShield)
            input.ConsumeShield();

        if (!input.IsShieldHeld || !character.Shield.CanActivate || !character.IsGrounded ||
            !(IsGroundLocomotion() || machine.Is(States.Parry)))
            return false;

        input.ConsumeParry();
        machine.ChangeState(States.Shield);
        return true;
    }

    private bool TryHandleDash(IInputProvider input)
    {
        if (!input.HasBufferedDash)
            return false;

        bool cancellingHeavyCharge = machine.Is(States.HeavyCharge);
        bool canStart = IsGroundLocomotion() || cancellingHeavyCharge || machine.Is(States.Jump);
        if (!canStart || !character.IsGrounded ||
            !character.Dash.TryStartDash(character.MoveInput.x))
            return false;

        input.ConsumeDash();
        if (!cancellingHeavyCharge && input.HasBufferedAttack)
        {
            input.ConsumeAttack();
            machine.ChangeState(States.DashAttack);
        }
        else
        {
            machine.ChangeState(States.Dash);
        }

        return true;
    }

    private bool TryHandleEvade(IInputProvider input)
    {
        if (!input.HasBufferedEvade || machine.Is(States.Dodge))
            return false;

        bool canDodge = character.Dodge.CanDodge && !character.IsGrounded && machine.Is(States.Jump);
        if (!canDodge)
            return false;

        input.ConsumeEvade();
        machine.ChangeState(States.Dodge);
        return true;
    }

    private bool TryHandleHeavyAttack(IInputProvider input)
    {
        if (!input.HasBufferedHeavyAttack || !character.IsGrounded || !IsGroundLocomotion())
            return false;

        HeavyAttackType attackType = character.Combat.ResolveHeavyAttackType();
        HeavyAttackStats stats = character.Combat.GetHeavyAttackStats(attackType);
        input.ConsumeHeavyAttack();
        if (stats == null)
        {
            Debug.LogError($"[CharacterActionRouter] Missing {attackType} Heavy Attack stats on '{character.name}'.", character);
            return false;
        }

        States.HeavyCharge.Prepare(attackType, stats);
        machine.ChangeState(States.HeavyCharge);
        return true;
    }

    private bool TryHandleParry(IInputProvider input)
    {
        if (!input.HasBufferedParry || !parry.TryParry())
            return false;

        input.ConsumeParry();
        return true;
    }

    private bool TryHandleCards(IInputProvider input)
    {
        if (!character.cardsEnabled ||
            !(machine.Is(States.Idle) || machine.Is(States.Move) || machine.Is(States.Jump)))
            return false;

        return TryUseBufferedCard(input);
    }

    private bool TryUseBufferedCard(IInputProvider input)
    {
        int slot;
        if (input.HasBufferedHand1) slot = 0;
        else if (input.HasBufferedHand2) slot = 1;
        else if (input.HasBufferedHand3) slot = 2;
        else if (input.HasBufferedHand4) slot = 3;
        else return false;

        CardActions result = deck.TryUseCardFromHand(slot);
        switch (slot)
        {
            case 0: input.ConsumeHand1(); break;
            case 1: input.ConsumeHand2(); break;
            case 2: input.ConsumeHand3(); break;
            case 3: input.ConsumeHand4(); break;
        }

        return result == CardActions.Success;
    }

    private bool IsGroundLocomotion() =>
        machine.Is(States.Idle) || machine.Is(States.Move) || machine.Is(States.Crouch);
}
