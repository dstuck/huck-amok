using UnityEngine;

[RequireComponent(typeof(SlimeComposition))]
public class SlimeCompositionBootstrap : MonoBehaviour
{
    private void Start()
    {
        if (GetComponent<SlimeProjectile>() != null)
            return;

        if (TryGetComponent<SlimeVisuals>(out var visuals) && TryGetComponent<SlimeComposition>(out var composition))
            visuals.Apply(composition);
    }
}
