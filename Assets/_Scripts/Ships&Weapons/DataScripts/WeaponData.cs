using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponData",
    menuName = "Ships/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string weaponName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private WeaponType weaponType;

    [Header("Combat")]
    [SerializeField] private float damage;
    [SerializeField] private int ammunition;
    [SerializeField] private float attackFireRate;
    [SerializeField] private float attackRange;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float sequentialFireDelay = 0.25f;

    [Header("Laser Beam")]
    [SerializeField] private float maximumEnergy = 100f;
    [SerializeField] private float energyDepletionRate;
    [SerializeField] private float energyRegenerationDelay;
    [SerializeField] private float energyRegenerationRate;

    [Header("Visuals")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject weaponModelPrefab;
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private WeaponFirePattern firePattern;

    public string WeaponName => weaponName;
    public string Description => description;
    public Sprite Icon => icon;
    public WeaponType WeaponType => weaponType;

    public float Damage => damage;
    public int Ammunition => ammunition;
    public float AttackFireRate => attackFireRate;
    public float AttackRange => attackRange;
    public float ProjectileSpeed => projectileSpeed;
    public float SequentialFireDelay => sequentialFireDelay;
    public float MaximumEnergy => maximumEnergy;
    public float EnergyDepletionRate => energyDepletionRate;
    public float EnergyRegenerationDelay => energyRegenerationDelay;
    public float EnergyRegenerationRate => energyRegenerationRate;

    public GameObject ProjectilePrefab => projectilePrefab;
    public GameObject WeaponModelPrefab => weaponModelPrefab;
    public GameObject BeamPrefab => beamPrefab;
    public WeaponFirePattern FirePattern => firePattern;

    public float SecondsBetweenAttacks
    {
        get
        {
            return attackFireRate > 0f
                ? 1f / attackFireRate
                : float.PositiveInfinity;
        }
    }
}