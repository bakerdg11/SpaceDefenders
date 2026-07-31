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
    [SerializeField] private float ammunition;
    [SerializeField] private float attackFireRate;
    [SerializeField] private float attackRange;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float energyDepletionRate;
    [SerializeField] private float energyRegenerationRate;

    [Header("Visuals")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject weaponModelPrefab;

    public string WeaponName => weaponName;
    public string Description => description;
    public Sprite Icon => icon;
    public WeaponType WeaponType => weaponType;

    public float Damage => damage;
    public float Ammunition => ammunition;
    public float AttackFireRate => attackFireRate;
    public float AttackRange => attackRange;
    public float ProjectileSpeed => projectileSpeed;
    public float EnergyDepletionRate => energyDepletionRate;
    public float EnergyRegenerationRate => energyRegenerationRate;

    public GameObject ProjectilePrefab => projectilePrefab;

    public GameObject WeaponModelPrefab => weaponModelPrefab;

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