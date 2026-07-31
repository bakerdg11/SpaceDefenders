using UnityEngine;

[CreateAssetMenu(
    fileName = "NewShipData",
    menuName = "Ships/Ship Data"
)]
public class ShipData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string shipName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;

    [Header("Prefab")]
    [SerializeField] private GameObject shipPrefab;

    [Header("Statistics")]
    [SerializeField, Min(1f)] private float maximumHealth;
    [SerializeField, Min(0f)] private float movementSpeed;
    [SerializeField, Min(0f)] private float rotationSpeed;

    [Header("Grid Placement")]
    [SerializeField, Min(0f)] private float heightOffset = 0.75f;

    [Header("Weapons")]
    [SerializeField] private ShipWeaponSlot[] weaponSlots;

    public string ShipName => shipName;
    public string Description => description;
    public Sprite Icon => icon;

    public GameObject ShipPrefab => shipPrefab;

    public float MaximumHealth => maximumHealth;
    public float MovementSpeed => movementSpeed;
    public float RotationSpeed => rotationSpeed;
    public float HeightOffset => heightOffset;

    public ShipWeaponSlot[] WeaponSlots => weaponSlots;
}