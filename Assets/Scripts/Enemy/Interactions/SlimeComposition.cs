using System;
using System.Collections.Generic;
using UnityEngine;

public class SlimeComposition : MonoBehaviour
{
    [SerializeField] private SlimeType[] slots = { SlimeType.Basic };

    public SlimeType[] Slots => slots;

    public SlimeAttackStats BuildAttackStats() => SlimeAttackStats.FromSlots(slots);

    public static SlimeType[] MergeSlots(SlimeComposition first, SlimeComposition second) =>
        ConcatSlots(first?.slots, second?.slots);

    public static SlimeType[] SplitForPickupPiece(SlimeComposition source) =>
        new[] { GetSlot(source, 0) };

    public static SlimeType[] SplitForPickupRemainder(SlimeComposition source) =>
        GetRemainingSlots(source, startIndex: 1);

    public static SlimeType[] SplitForHit(SlimeComposition source)
    {
        if (source?.slots == null || source.slots.Length <= 1)
            return new[] { GetSlot(source, 0) };

        return GetRemainingSlots(source, startIndex: 0, removeLast: true);
    }

    public void CopyFrom(SlimeComposition source)
    {
        if (source == null)
        {
            slots = new[] { SlimeType.Basic };
            return;
        }

        CopyFromSlots(source.slots);
    }

    public void CopyFromSlots(SlimeType[] sourceSlots)
    {
        slots = sourceSlots != null && sourceSlots.Length > 0
            ? (SlimeType[])sourceSlots.Clone()
            : new[] { SlimeType.Basic };
    }

    public void SetSingleType(SlimeType type)
    {
        slots = new[] { type };
    }

    private static SlimeType GetSlot(SlimeComposition source, int index)
    {
        if (source?.slots != null && source.slots.Length > index)
            return source.slots[index];

        return SlimeType.Basic;
    }

    private static SlimeType[] GetRemainingSlots(SlimeComposition source, int startIndex, bool removeLast = false)
    {
        if (source?.slots == null || source.slots.Length == 0)
            return Array.Empty<SlimeType>();

        int endExclusive = removeLast ? source.slots.Length - 1 : source.slots.Length;
        if (startIndex >= endExclusive)
            return Array.Empty<SlimeType>();

        var remaining = new SlimeType[endExclusive - startIndex];
        Array.Copy(source.slots, startIndex, remaining, 0, remaining.Length);
        return remaining;
    }

    private static SlimeType[] ConcatSlots(SlimeType[] first, SlimeType[] second)
    {
        var merged = new List<SlimeType>();

        if (first != null)
            merged.AddRange(first);

        if (second != null)
            merged.AddRange(second);

        return merged.Count > 0 ? merged.ToArray() : new[] { SlimeType.Basic };
    }
}
