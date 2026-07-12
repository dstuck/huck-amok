using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerMovementSlowdown : MonoBehaviour
{
    private readonly HashSet<StickySplatZone> activeZones = new HashSet<StickySplatZone>();
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void EnterZone(StickySplatZone zone)
    {
        if (zone == null)
            return;

        activeZones.Add(zone);
        RecalculateSlowdown();
    }

    public void ExitZone(StickySplatZone zone)
    {
        if (zone == null)
            return;

        activeZones.Remove(zone);
        RecalculateSlowdown();
    }

    private void RecalculateSlowdown()
    {
        float multiplier = 1f;

        foreach (var zone in activeZones)
        {
            if (zone != null)
                multiplier *= zone.SlowMultiplier;
        }

        playerController.SetMovementSlow(multiplier);
    }
}
