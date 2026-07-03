using UnityEngine;

public class SlimeTierLinks : MonoBehaviour
{
    [SerializeField] private GameObject tierUpPrefab;
    [SerializeField] private GameObject tierDownPrefab;
    [SerializeField] private GameObject pickupPiecePrefab;
    [SerializeField] private GameObject pickupRemainderPrefab;
    [SerializeField] private Vector2 pickupRemainderOffset = new Vector2(0.15f, 0f);
    [SerializeField] private bool canAutoCombine;

    public GameObject TierUpPrefab => tierUpPrefab;
    public GameObject TierDownPrefab => tierDownPrefab;
    public GameObject PickupPiecePrefab => pickupPiecePrefab;
    public GameObject PickupRemainderPrefab => pickupRemainderPrefab;
    public Vector2 PickupRemainderOffset => pickupRemainderOffset;
    public bool CanAutoCombine => canAutoCombine;
}
