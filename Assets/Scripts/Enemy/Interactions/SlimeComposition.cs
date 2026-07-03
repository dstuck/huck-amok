using UnityEngine;

public class SlimeComposition : MonoBehaviour
{
    public static SlimeComposition Merge(SlimeComposition first, SlimeComposition second)
    {
        return null;
    }

    public static void SplitForPickup(
        SlimeComposition source,
        out SlimeComposition piece,
        out SlimeComposition remainder)
    {
        piece = null;
        remainder = null;
    }

    public static SlimeComposition SplitForHit(SlimeComposition source)
    {
        return null;
    }

    public void CopyFrom(SlimeComposition source)
    {
    }
}
