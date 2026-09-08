using System.Collections.Generic;
using UnityEngine;

// Records measured positions, including gravity/collisions. It is not a trajectory prediction.
public sealed class TrainingLaunchTrace
{
    private const int MaximumSamples = 512;
    private List<Vector3> points = new List<Vector3>(MaximumSamples);
    private List<Vector3> previousPoints = new List<Vector3>(MaximumSamples);
    public IReadOnlyList<Vector3> Points => points;
    public IReadOnlyList<Vector3> PreviousPoints => previousPoints;
    public Vector2 InitialVelocity { get; private set; }
    public float HitStun { get; private set; }
    public float Elapsed { get; private set; }
    public int DamageBefore { get; private set; }
    public int DamageAfter { get; private set; }
    public bool IsRecording { get; private set; }

    public void Begin(Vector3 position, Vector2 velocity, float stun, int before, int after)
    {
        if (points.Count > 0)
        {
            List<Vector3> swap = previousPoints;
            previousPoints = points;
            points = swap;
        }
        points.Clear();
        points.Add(position);
        InitialVelocity = velocity;
        HitStun = stun;
        DamageBefore = before;
        DamageAfter = after;
        Elapsed = 0f;
        IsRecording = true;
    }

    public void Record(Vector3 position, float deltaTime, bool moving)
    {
        if (!IsRecording || deltaTime <= 0f)
            return;
        Elapsed += deltaTime;
        if (points.Count < MaximumSamples)
            points.Add(position);
        if (!moving || points.Count >= MaximumSamples)
            IsRecording = false;
    }

    public void Stop() => IsRecording = false;
    public void Clear()
    {
        Stop();
        points.Clear();
        previousPoints.Clear();
        InitialVelocity = Vector2.zero;
        HitStun = Elapsed = 0f;
        DamageBefore = DamageAfter = 0;
    }
}
