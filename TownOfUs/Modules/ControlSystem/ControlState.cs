using UnityEngine;

namespace TownOfUs.Modules.ControlSystem;

/// <summary>
/// Per-client state for Role-player control. This is intentionally client-local:
/// - The controller sends the desired direction for a controlled player via RPC.
/// - The controlled player's owner applies that direction inside a movement patch.
/// </summary>
public class ControlState
{
    // After initial control begins, different clients may briefly disagree on transform state.
    // During this grace window we avoid applying any victim movement input to prevent desync.
    public const float InitialControlSyncGraceSeconds = 1.0f;

    private readonly Dictionary<byte, byte> ControlledBy = new();
    private readonly Dictionary<byte, Vector2> ControlledDirection = new();
    private readonly Dictionary<byte, Vector2> ControlledPosition = new();
    private readonly Dictionary<byte, Vector2> ControlledVelocity = new();
    private readonly Dictionary<byte, float> ControlledSince = new();

    public void SetControl(byte controlledId, byte controllerId)
    {
        ControlledBy[controlledId] = controllerId;
        ControlledDirection[controlledId] = Vector2.zero;
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
        ControlledSince[controlledId] = Time.time;
    }

    public void ClearControl(byte controlledId)
    {
        ControlledBy.Remove(controlledId);
        ControlledDirection.Remove(controlledId);
        ControlledPosition.Remove(controlledId);
        ControlledVelocity.Remove(controlledId);
        ControlledSince.Remove(controlledId);
    }

    public bool IsControlled(byte controlledId, out byte controllerId)
    {
        return ControlledBy.TryGetValue(controlledId, out controllerId);
    }

    public void SetDirection(byte controlledId, Vector2 direction)
    {
        ControlledDirection[controlledId] = direction;
    }

    public Vector2 GetDirection(byte controlledId)
    {
        return ControlledDirection.TryGetValue(controlledId, out var dir) ? dir : Vector2.zero;
    }

    public void SetMovementState(byte controlledId, Vector2 position, Vector2 velocity)
    {
        ControlledPosition[controlledId] = position;
        ControlledVelocity[controlledId] = velocity;
    }

    public Vector2 GetPosition(byte controlledId)
    {
        return ControlledPosition.TryGetValue(controlledId, out var pos) ? pos : Vector2.zero;
    }

    public Vector2 GetVelocity(byte controlledId)
    {
        return ControlledVelocity.TryGetValue(controlledId, out var vel) ? vel : Vector2.zero;
    }

    public float GetControlElapsedSeconds(byte controlledId)
    {
        return ControlledSince.TryGetValue(controlledId, out var since) ? Mathf.Max(0f, Time.time - since) : float.PositiveInfinity;
    }

    public bool IsInInitialGrace(byte controlledId)
    {
        return GetControlElapsedSeconds(controlledId) < InitialControlSyncGraceSeconds;
    }

    public void ClearMovementState(byte controlledId)
    {
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
    }

    public void ClearAll()
    {
        ControlledBy.Clear();
        ControlledDirection.Clear();
        ControlledPosition.Clear();
        ControlledVelocity.Clear();
        ControlledSince.Clear();
    }
}