using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyShipData", menuName = "Ships/Enemy Ship Data")]
public class EnemyShipData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string shipName;
    [SerializeField] private GameObject shipPrefab;

    [Header("Health")]
    [SerializeField, Min(1f)] private float maximumHealth = 100f;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float movementSpeed = 3f;
    [SerializeField, Min(0f)] private float rotationSpeed = 360f;
    [SerializeField, Min(0.01f)] private float stoppingDistance = 0.1f;

    [Header("Combat")]
    [SerializeField] private WeaponData weaponData;

    [Header("Rewards")]
    [SerializeField, Min(1)] private int resourceDropValue = 10;

    public string ShipName => shipName;
    public GameObject ShipPrefab => shipPrefab;

    public float MaximumHealth => maximumHealth;

    public float MovementSpeed => movementSpeed;
    public float RotationSpeed => rotationSpeed;
    public float StoppingDistance => stoppingDistance;

    public WeaponData WeaponData => weaponData;

    public int ResourceDropValue => resourceDropValue;
}