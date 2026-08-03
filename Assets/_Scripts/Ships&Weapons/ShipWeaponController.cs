using UnityEngine;

[RequireComponent(typeof(ShipController))]
public class ShipWeaponController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("How often this ship searches for an enemy.")]
    [SerializeField, Min(0.05f)]
    private float targetScanInterval = 0.25f;

    [Header("Weapon Position")]
    [SerializeField] private Transform firePoint;

    [Header("Aiming")]
    [SerializeField, Min(0f)]
    private float requiredAimAngle = 8f;

    private ShipController shipController;
    private CollectorShip collectorShip;

    private WeaponData equippedWeapon;
    private EnemyHealth currentTarget;

    private int currentAmmunition;
    private float nextScanTime;
    private float nextFireTime;

    public WeaponData EquippedWeapon => equippedWeapon;
    public EnemyHealth CurrentTarget => currentTarget;
    public float CurrentAmmunition => currentAmmunition;

    private void Awake()
    {
        shipController = GetComponent<ShipController>();
        collectorShip = GetComponent<CollectorShip>();
    }

    private void Start()
    {
        LoadStartingWeapon();
    }

    private void Update()
    {
        if (!CanCurrentlyAttack())
        {
            currentTarget = null;
            return;
        }

        UpdateTarget();

        if (currentTarget == null)
        {
            return;
        }

        AimAtTarget();
        TryFire();
    }

    private void LoadStartingWeapon()
    {
        if (shipController == null ||
            shipController.ShipData == null)
        {
            Debug.LogError(
                $"{name} cannot load a weapon because ShipData is missing.",
                this
            );

            return;
        }

        ShipWeaponSlot[] weaponSlots =
            shipController.ShipData.WeaponSlots;

        if (weaponSlots == null ||
            weaponSlots.Length == 0)
        {
            Debug.LogWarning(
                $"{name} has no weapon slots configured.",
                this
            );

            return;
        }

        ShipWeaponSlot firstSlot = weaponSlots[0];

        if (firstSlot == null ||
            firstSlot.StartingWeapon == null)
        {
            Debug.LogWarning(
                $"{name}'s first weapon slot has no starting weapon.",
                this
            );

            return;
        }

        WeaponData startingWeapon =
            firstSlot.StartingWeapon;

        if (!firstSlot.CanEquip(startingWeapon))
        {
            Debug.LogError(
                $"{startingWeapon.WeaponName} is not allowed " +
                $"in {name}'s first weapon slot.",
                this
            );

            return;
        }

        equippedWeapon = startingWeapon;

        /*
         * WeaponData holds the starting amount.
         * This controller holds the amount remaining at runtime.
         */
        currentAmmunition =
            Mathf.Max(0, equippedWeapon.Ammunition);

        Debug.Log(
            $"{name} equipped {equippedWeapon.WeaponName} " +
            $"with {currentAmmunition} ammunition."
        );
    }

    private bool CanCurrentlyAttack()
    {
        if (equippedWeapon == null)
        {
            return false;
        }

        /*
         * Collector resource behaviour takes priority over combat.
         */
        if (collectorShip != null &&
            !collectorShip.CanAttack)
        {
            return false;
        }

        /*
         * Treat ammunition <= 0 as no ammunition remaining.
         * If you later want infinite-ammo weapons, add a dedicated
         * Boolean rather than using zero for two meanings.
         */
        if (currentAmmunition <= 0f)
        {
            return false;
        }

        return true;
    }

    private void UpdateTarget()
    {
        if (currentTarget != null &&
            IsTargetValid(currentTarget))
        {
            return;
        }

        currentTarget = null;

        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime =
            Time.time + targetScanInterval;

        FindClosestTarget();
    }

    private void FindClosestTarget()
    {
        Collider[] enemyColliders =
            Physics.OverlapSphere(
                transform.position,
                equippedWeapon.AttackRange,
                enemyLayer
            );

        EnemyHealth closestEnemy = null;

        float closestDistanceSquared =
            float.PositiveInfinity;

        foreach (Collider enemyCollider in enemyColliders)
        {
            EnemyHealth enemy =
                enemyCollider.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.CurrentHealth <= 0f)
            {
                continue;
            }

            float distanceSquared =
                (enemy.transform.position -
                 transform.position).sqrMagnitude;

            if (distanceSquared >= closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared = distanceSquared;
            closestEnemy = enemy;
        }

        currentTarget = closestEnemy;
    }

    private bool IsTargetValid(EnemyHealth target)
    {
        if (target == null ||
            target.CurrentHealth <= 0f)
        {
            return false;
        }

        float attackRangeSquared =
            equippedWeapon.AttackRange *
            equippedWeapon.AttackRange;

        float distanceSquared =
            (target.transform.position -
             transform.position).sqrMagnitude;

        return distanceSquared <= attackRangeSquared;
    }

    private void AimAtTarget()
    {
        Vector3 directionToTarget =
            currentTarget.transform.position -
            transform.position;

        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                directionToTarget.normalized,
                Vector3.up
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                shipController.RotationSpeed *
                Time.deltaTime
            );
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        if (!IsAimedAtTarget())
        {
            return;
        }

        if (currentAmmunition <= 0)
        {
            return;
        }

        FireWeapon();

        currentAmmunition--;

        nextFireTime =
            Time.time +
            equippedWeapon.SecondsBetweenAttacks;

        Debug.Log(
            $"{name} fired {equippedWeapon.WeaponName}. " +
            $"Ammo remaining: {currentAmmunition}"
        );
    }

    private bool IsAimedAtTarget()
    {
        Vector3 directionToTarget =
            currentTarget.transform.position -
            transform.position;

        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        float angleToTarget =
            Vector3.Angle(
                transform.forward,
                directionToTarget.normalized
            );

        return angleToTarget <= requiredAimAngle;
    }

    private void FireWeapon()
    {
        if (currentTarget == null ||
            equippedWeapon == null)
        {
            return;
        }

        if (equippedWeapon.ProjectilePrefab != null)
        {
            FireProjectile();
        }
        else
        {
            FireInstantHit();
        }
    }

    private void FireInstantHit()
    {
        /*
         * Useful for rail guns, lasers, or early testing.
         */
        currentTarget.TakeDamage(
            equippedWeapon.Damage
        );

        Vector3 shotOrigin =
            firePoint != null
            ? firePoint.position
            : transform.position;

        Debug.DrawLine(
            shotOrigin,
            currentTarget.transform.position,
            Color.red,
            0.2f
        );
    }

    private void FireProjectile()
    {
        Vector3 spawnPosition =
            firePoint != null
            ? firePoint.position
            : transform.position;

        Vector3 direction =
            currentTarget.transform.position -
            spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion projectileRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        GameObject projectile = Instantiate(
            equippedWeapon.ProjectilePrefab,
            spawnPosition,
            projectileRotation
        );

        ShipProjectile shipProjectile =
            projectile.GetComponent<ShipProjectile>();

        if (shipProjectile == null)
        {
            Debug.LogWarning(
                $"{projectile.name} does not contain a ShipProjectile component.",
                projectile
            );

            return;
        }

        shipProjectile.Initialize(
            currentTarget,
            equippedWeapon.Damage,
            equippedWeapon.ProjectileSpeed
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (equippedWeapon == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            equippedWeapon.AttackRange
        );
    }
}