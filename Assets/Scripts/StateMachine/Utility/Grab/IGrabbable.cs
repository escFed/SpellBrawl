using UnityEngine;

public interface IGrabbable
{
    bool CanBeGrabbed { get; }
    Transform GrabTransform { get; }

    void OnGrabbed(Transform holdPoint);
    void UpdateGrabbedPosition(Transform holdPoint);
    void TakePummelDamage(int amount);

    void OnThrown(CombatHit hit);
    void OnReleased();
}
