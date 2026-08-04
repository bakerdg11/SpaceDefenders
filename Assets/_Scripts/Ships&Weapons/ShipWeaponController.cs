using UnityEngine;

[RequireComponent(typeof(ShipController))]
public class ShipWeaponController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("How often this ship searches for an enemy.")]
    [SerializeField, Min(0.05f)]
    private float targetScanInterval = 0.25f;

    [Header("Weapon Positions")]
    [Tooltip(
        "Assign one or more projectile spawn points. " +
        "Their usage depends on the weapon's Fire Pattern."
    )]
    [SerializeField] private Transform[] firePoints;

    [Header("Aiming")]
    [SerializeField, Min(0f)]
    private float requiredAimAngle = 8f;

    private ShipController shipController;
    private CollectorShip collectorShip;

    private WeaponData equippedWeapon;
    private EnemyHealth currentTarget;

    private int currentAmmunition;
    private int nextFirePointIndex;

    private float nextScanTime;
    private float nextFireTime;

    public WeaponData EquippedWeapon => equippedWeapon;
    public EnemyHealth CurrentTarget => currentTarget;
    public int CurrentAmmunition => currentAmmunition;

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

        currentAmmunition =
            Mathf.Max(0, equippedWeapon.Ammunition);

        nextFirePointIndex = 0;

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
         * Resource collection has priority over combat
         * for any ship containing CollectorShip.
         */
        if (collectorShip != null &&
            !collectorShip.CanAttack)
        {
            return false;
        }

        if (currentAmmunition <= 0)
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

        bool firedSuccessfully =
            FireWeapon();

        if (!firedSuccessfully)
        {
            return;
        }

        /*
         * One attack cycle consumes one ammunition,
         * even if a simultaneous weapon launches two projectiles.
         */
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

    private bool FireWeapon()
    {
        if (currentTarget == null ||
            equippedWeapon == null)
        {
            return false;
        }

        /*
         * Beam weapons apply damage immediately and display
         * a temporary Line Renderer effect.
         */
        if (equippedWeapon.BeamPrefab != null)
        {
            return FireBeam();
        }

        /*
         * Weapons with projectile prefabs use their selected
         * projectile firing pattern.
         */
        if (equippedWeapon.ProjectilePrefab != null)
        {
            switch (equippedWeapon.FirePattern)
            {
                case WeaponFirePattern.Single:
                    return FireSingleProjectile();

                case WeaponFirePattern.Alternating:
                    return FireAlternatingProjectile();

                case WeaponFirePattern.Simultaneous:
                    return FireSimultaneousProjectiles();

                default:
                    Debug.LogWarning(
                        $"{name} has an unsupported fire pattern.",
                        this
                    );

                    return false;
            }
        }

        /*
         * Fallback for instant-hit weapons with no visual prefab.
         */
        return FireInstantHit();
    }

    private bool FireBeam()
    {
        if (currentTarget == null ||
            equippedWeapon == null ||
            equippedWeapon.BeamPrefab == null)
        {
            return false;
        }

        Transform selectedFirePoint =
            GetFirstValidFirePoint();

        if (selectedFirePoint == null)
        {
            return false;
        }

        GameObject beamObject = Instantiate(
            equippedWeapon.BeamPrefab,
            selectedFirePoint.position,
            Quaternion.identity
        );

        LaserBeam laserBeam =
            beamObject.GetComponent<LaserBeam>();

        if (laserBeam == null)
        {
            Debug.LogWarning(
                $"{beamObject.name} does not contain LaserBeam.",
                beamObject
            );

            Destroy(beamObject);
            return false;
        }

        laserBeam.Initialize(
            selectedFirePoint,
            currentTarget.transform,
            equippedWeapon.BeamDuration
        );

        /*
         * Laser damage happens immediately.
         * The beam object is only the visual representation.
         */
        currentTarget.TakeDamage(
            equippedWeapon.Damage
        );

        return true;
    }

    private bool FireSingleProjectile()
    {
        Transform selectedFirePoint =
            GetFirstValidFirePoint();

        if (selectedFirePoint == null)
        {
            return false;
        }

        return SpawnProjectile(selectedFirePoint);
    }

    private bool FireAlternatingProjectile()
    {
        Transform selectedFirePoint =
            GetCurrentAlternatingFirePoint();

        if (selectedFirePoint == null)
        {
            return false;
        }

        bool spawnedSuccessfully =
            SpawnProjectile(selectedFirePoint);

        if (spawnedSuccessfully)
        {
            AdvanceFirePoint();
        }

        return spawnedSuccessfully;
    }

    private bool FireSimultaneousProjectiles()
    {
        if (firePoints == null ||
            firePoints.Length == 0)
        {
            Debug.LogWarning(
                $"{name} has no fire points assigned.",
                this
            );

            return false;
        }

        bool spawnedAtLeastOneProjectile = false;

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null)
            {
                continue;
            }

            if (SpawnProjectile(firePoint))
            {
                spawnedAtLeastOneProjectile = true;
            }
        }

        return spawnedAtLeastOneProjectile;
    }

    private bool SpawnProjectile(
        Transform selectedFirePoint)
    {
        if (selectedFirePoint == null ||
            currentTarget == null ||
            equippedWeapon == null ||
            equippedWeapon.ProjectilePrefab == null)
        {
            return false;
        }

        Vector3 spawnPosition =
            selectedFirePoint.position;

        Vector3 direction =
            currentTarget.transform.position -
            spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        Quaternion projectileRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        GameObject projectile =
            Instantiate(
                equippedWeapon.ProjectilePrefab,
                spawnPosition,
                projectileRotation
            );

        ShipProjectile shipProjectile =
            projectile.GetComponent<ShipProjectile>();

        if (shipProjectile == null)
        {
            Debug.LogWarning(
                $"{projectile.name} does not contain " +
                "a ShipProjectile component.",
                projectile
            );

            Destroy(projectile);
            return false;
        }

        shipProjectile.Initialize(
            currentTarget,
            equippedWeapon.Damage,
            equippedWeapon.ProjectileSpeed
        );

        return true;
    }

    private bool FireInstantHit()
    {
        if (currentTarget == null)
        {
            return false;
        }

        Transform selectedFirePoint =
            GetFirstValidFirePoint();

        Vector3 shotOrigin =
            selectedFirePoint != null
                ? selectedFirePoint.position
                : transform.position;

        currentTarget.TakeDamage(
            equippedWeapon.Damage
        );

        Debug.DrawLine(
            shotOrigin,
            currentTarget.transform.position,
            Color.red,
            0.2f
        );

        return true;
    }

    private Transform GetFirstValidFirePoint()
    {
        if (firePoints == null ||
            firePoints.Length == 0)
        {
            /*
             * Fall back to the ship root so the weapon still works
             * while a fire point is being configured.
             */
            return transform;
        }

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != null)
            {
                return firePoint;
            }
        }

        return transform;
    }

    private Transform GetCurrentAlternatingFirePoint()
    {
        if (firePoints == null ||
            firePoints.Length == 0)
        {
            return transform;
        }

        /*
         * Search from the current index until a valid fire point
         * is found, in case an array element is empty.
         */
        for (int attempt = 0;
             attempt < firePoints.Length;
             attempt++)
        {
            int index =
                (nextFirePointIndex + attempt) %
                firePoints.Length;

            if (firePoints[index] == null)
            {
                continue;
            }

            nextFirePointIndex = index;
            return firePoints[index];
        }

        return transform;
    }

    private void AdvanceFirePoint()
    {
        if (firePoints == null ||
            firePoints.Length == 0)
        {
            return;
        }

        nextFirePointIndex =
            (nextFirePointIndex + 1) %
            firePoints.Length;
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