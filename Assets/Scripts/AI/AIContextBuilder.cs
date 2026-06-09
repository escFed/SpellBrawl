//using UnityEngine;

//public static class AIContextBuilder
//{
//    public static AIContext Build(Transform selfTransform, PlayerController selfController, EnergyManager selfEnergy, CharacterHealth selfHealth, CharacterHealth targetHealth, AITarget targetTracker, AINavigation navigation, AICardSelector cardSelector, float attackRange, float cardRange)
//    {
//        Vector3 targetPosition = targetTracker.PerceivedTargetPosition;
//        float distanceX = Mathf.Abs(targetPosition.x - selfTransform.position.x);
//        float distanceY = targetPosition.y - selfTransform.position.y;
//        bool nearEdge = navigation.IsNearEdge(selfController, selfTransform);

//        return new AIContext
//        {
//            selfPosition = selfTransform.position,
//            targetPosition = targetPosition,
//            distanceX = distanceX,
//            distanceY = distanceY,
//            selfDamage = selfHealth != null ? selfHealth.currentDamage : 0f,
//            targetDamage = targetHealth != null ? targetHealth.currentDamage : 0f,
//            energy = selfEnergy != null ? selfEnergy.currentEnergy : 0f,
//            targetInAttackRange = distanceX <= attackRange && Mathf.Abs(distanceY) < 1f,
//            targetInCardRange = distanceX <= cardRange,
//            targetAbove = distanceY > navigation.verticalJumpThreshold,
//            targetBelow = distanceY < -navigation.verticalJumpThreshold,
//            emptyHand = cardSelector.HasEmptyHand(),
//            inDanger = selfHealth != null && selfHealth.currentDamage >= 85f,
//            nearEdge = nearEdge,
//            shouldRecover = navigation.ShouldRecover(selfTransform, selfHealth, targetPosition, nearEdge)
//        };
//    }
//}

