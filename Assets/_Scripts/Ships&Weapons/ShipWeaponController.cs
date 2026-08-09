using System.Collections;
using System;
using UnityEngine;

[RequireComponent(typeof(ShipController))]
public class ShipWeaponController : MonoBehaviour
{
    [Header("References")]
    private ShipController shipController;
    private CollectorShip collectorShip;
    private WeaponData equippedWeapon;
    private EnemyHealth currentTarget;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetScanInterval = 0.25f;
    [SerializeField] private float requiredAimAngle = 8f;


    [Header("Weapon Firing")]
    [SerializeField] private Transform[] firePoints;
    private int nextFirePointIndex;
    private float nextScanTime;
    private float nextFireTime;
    private bool isSequentialFiring;

    private int currentAmmunition;
    public event Action<int, int> AmmunitionChanged;

    [Header("Laser Beam")]
    private float currentEnergy;
    private float beamStoppedTime;
    private bool isBeamFiring;
    private bool waitingForFullRecharge;
    private LaserBeam activeBeam;
    private EnemyHealth beamTarget;
    public event Action<float, float> EnergyChanged;


    public WeaponData EquippedWeapon => equippedWeapon;
    public EnemyHealth CurrentTarget => currentTarget;
    public int CurrentAmmunition => currentAmmunition;
    public float CurrentEnergy => currentEnergy;
    public bool IsBeamFiring => isBeamFiring;

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
        if (shipController == null || !shipController.IsOperational)
        {
            return;
        }

        if (equippedWeapon == null)
        {
            return;
        }

        if (collectorShip != null && !collectorShip.CanAttack)
        {
            StopBeam(false);
            currentTarget = null;
            RegenerateEnergy();
            return;
        }

        UpdateTarget();

        if (equippedWeapon.BeamPrefab != null)
        {
            UpdateBeamWeapon();
            return;
        }

        if (currentTarget == null)
        {
            return;
        }

        AimAtTarget();
        TryFire();
    }

    private void LoadStartingWeapon()
    {
        if (shipController == null || shipController.ShipData == null)
        {
            Debug.LogError($"{name} cannot load a weapon because ShipData is missing.", this);
            return;
        }

        ShipWeaponSlot[] weaponSlots = shipController.ShipData.WeaponSlots;

        if (weaponSlots == null || weaponSlots.Length == 0)
        {
            Debug.LogWarning($"{name} has no weapon slots configured.", this);
            return;
        }

        ShipWeaponSlot firstSlot = weaponSlots[0];

        if (firstSlot == null || firstSlot.StartingWeapon == null)
        {
            Debug.LogWarning($"{name}'s first weapon slot has no starting weapon.", this);
            return;
        }

        WeaponData startingWeapon = firstSlot.StartingWeapon;

        if (!firstSlot.CanEquip(startingWeapon))
        {
            Debug.LogError($"{startingWeapon.WeaponName} is not allowed in {name}'s first weapon slot.", this);
            return;
        }

        equippedWeapon = startingWeapon;
        currentAmmunition = Mathf.Max(0, equippedWeapon.Ammunition);
        AmmunitionChanged?.Invoke(currentAmmunition, equippedWeapon.Ammunition);
        currentEnergy = equippedWeapon.MaximumEnergy;
        EnergyChanged?.Invoke(currentEnergy, equippedWeapon.MaximumEnergy);
        nextFirePointIndex = 0;

        Debug.Log($"{name} equipped {equippedWeapon.WeaponName}.");
    }

    private void UpdateTarget()
    {
        if (currentTarget != null && IsTargetValid(currentTarget))
        {
            return;
        }

        currentTarget = null;

        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime = Time.time + targetScanInterval;
        FindClosestTarget();
    }

    private void FindClosestTarget()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, equippedWeapon.AttackRange, enemyLayer);

        EnemyHealth closestEnemy = null;
        float closestDistanceSquared = float.PositiveInfinity;

        foreach (Collider enemyCollider in enemyColliders)
        {
            EnemyHealth enemy = enemyCollider.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.CurrentHealth <= 0f)
            {
                continue;
            }

            float distanceSquared = (enemy.transform.position - transform.position).sqrMagnitude;

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
        if (target == null || target.CurrentHealth <= 0f)
        {
            return false;
        }

        float attackRangeSquared = equippedWeapon.AttackRange * equippedWeapon.AttackRange;
        float distanceSquared = (target.transform.position - transform.position).sqrMagnitude;

        return distanceSquared <= attackRangeSquared;
    }

    private void AimAtTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        Vector3 directionToTarget = currentTarget.transform.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, shipController.RotationSpeed * Time.deltaTime);
    }

    private void UpdateBeamWeapon()
    {
        if (currentTarget == null)
        {
            StopBeam(false);
            RegenerateEnergy();
            return;
        }

        AimAtTarget();

        if (isBeamFiring)
        {
            ContinueBeam();
            return;
        }

        if (waitingForFullRecharge)
        {
            RegenerateEnergy();

            if (currentEnergy < equippedWeapon.MaximumEnergy)
            {
                return;
            }

            waitingForFullRecharge = false;
        }

        if (currentEnergy <= 0f)
        {
            waitingForFullRecharge = true;
            RegenerateEnergy();
            return;
        }

        if (!IsAimedAtTarget())
        {
            return;
        }

        StartBeam();
    }

    private void StartBeam()
    {
        Transform selectedFirePoint = GetFirstValidFirePoint();

        if (selectedFirePoint == null || currentTarget == null)
        {
            return;
        }

        GameObject beamObject = Instantiate(equippedWeapon.BeamPrefab, selectedFirePoint.position, Quaternion.identity);
        activeBeam = beamObject.GetComponent<LaserBeam>();

        if (activeBeam == null)
        {
            Debug.LogWarning($"{beamObject.name} does not contain LaserBeam.", beamObject);
            Destroy(beamObject);
            return;
        }

        beamTarget = currentTarget;
        activeBeam.Initialize(selectedFirePoint, beamTarget.transform);
        isBeamFiring = true;
    }

    private void ContinueBeam()
    {
        if (beamTarget == null || activeBeam == null)
        {
            StopBeam(false);
            return;
        }

        if (currentTarget != beamTarget)
        {
            StopBeam(false);
            return;
        }

        if (!IsTargetValid(beamTarget))
        {
            StopBeam(false);
            return;
        }

        currentEnergy -= equippedWeapon.EnergyDepletionRate * Time.deltaTime;
        currentEnergy = Mathf.Max(0f, currentEnergy);

        EnergyChanged?.Invoke(currentEnergy, equippedWeapon.MaximumEnergy);

        beamTarget.TakeDamage(equippedWeapon.Damage * Time.deltaTime);

        if (currentEnergy <= 0f)
        {
            StopBeam(true);
        }
    }

    private void StopBeam(bool depleted)
    {
        if (!isBeamFiring && activeBeam == null)
        {
            return;
        }

        isBeamFiring = false;

        if (depleted)
        {
            waitingForFullRecharge = true;
            beamStoppedTime = Time.time;
        }

        if (activeBeam != null)
        {
            activeBeam.StopBeam();
            activeBeam = null;
        }

        beamTarget = null;
    }

    private void RegenerateEnergy()
    {
        if (isBeamFiring || currentEnergy >= equippedWeapon.MaximumEnergy)
        {
            return;
        }

        if (waitingForFullRecharge && Time.time < beamStoppedTime + equippedWeapon.EnergyRegenerationDelay)
        {
            return;
        }

        currentEnergy += equippedWeapon.EnergyRegenerationRate * Time.deltaTime;
        currentEnergy = Mathf.Min(currentEnergy, equippedWeapon.MaximumEnergy);

        EnergyChanged?.Invoke(currentEnergy, equippedWeapon.MaximumEnergy);
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

        bool firedSuccessfully = FireWeapon();

        if (!firedSuccessfully)
        {
            return;
        }

        currentAmmunition--;
        AmmunitionChanged?.Invoke(currentAmmunition, equippedWeapon.Ammunition);
        nextFireTime = Time.time + equippedWeapon.SecondsBetweenAttacks;

        Debug.Log($"{name} fired {equippedWeapon.WeaponName}. Ammo remaining: {currentAmmunition}");
    }

    private bool IsAimedAtTarget()
    {
        if (currentTarget == null)
        {
            return false;
        }

        Vector3 directionToTarget = currentTarget.transform.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);
        return angleToTarget <= requiredAimAngle;
    }

    private bool FireWeapon()
    {
        if (currentTarget == null || equippedWeapon == null)
        {
            return false;
        }

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

                case WeaponFirePattern.Sequential:
                    return StartSequentialProjectiles();

                default:
                    return false;
            }
        }

        return FireInstantHit();
    }

    private bool FireSingleProjectile()
    {
        Transform selectedFirePoint = GetFirstValidFirePoint();

        if (selectedFirePoint == null)
        {
            return false;
        }

        return SpawnProjectile(selectedFirePoint);
    }

    private bool FireAlternatingProjectile()
    {
        Transform selectedFirePoint = GetCurrentAlternatingFirePoint();

        if (selectedFirePoint == null)
        {
            return false;
        }

        bool spawnedSuccessfully = SpawnProjectile(selectedFirePoint);

        if (spawnedSuccessfully)
        {
            AdvanceFirePoint();
        }

        return spawnedSuccessfully;
    }

    private bool FireSimultaneousProjectiles()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            return false;
        }

        bool spawnedAtLeastOneProjectile = false;

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != null && SpawnProjectile(firePoint))
            {
                spawnedAtLeastOneProjectile = true;
            }
        }

        return spawnedAtLeastOneProjectile;
    }

    private bool StartSequentialProjectiles()
    {
        if (isSequentialFiring)
        {
            return false;
        }

        if (firePoints == null || firePoints.Length < 2)
        {
            return false;
        }

        StartCoroutine(FireSequentialProjectiles());
        return true;
    }

    private IEnumerator FireSequentialProjectiles()
    {
        isSequentialFiring = true;

        SpawnProjectile(firePoints[0]);

        yield return new WaitForSeconds(equippedWeapon.SequentialFireDelay);

        if (currentTarget != null && currentTarget.CurrentHealth > 0f)
        {
            SpawnProjectile(firePoints[1]);
        }

        isSequentialFiring = false;
    }

    private bool SpawnProjectile(Transform selectedFirePoint)
    {
        if (selectedFirePoint == null || currentTarget == null || equippedWeapon.ProjectilePrefab == null)
        {
            return false;
        }

        Vector3 direction = currentTarget.transform.position - selectedFirePoint.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        Quaternion projectileRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        GameObject projectile = Instantiate(equippedWeapon.ProjectilePrefab, selectedFirePoint.position, projectileRotation);

        ShipProjectile shipProjectile = projectile.GetComponent<ShipProjectile>();

        if (shipProjectile == null)
        {
            Destroy(projectile);
            return false;
        }

        shipProjectile.Initialize(currentTarget, equippedWeapon.Damage, equippedWeapon.ProjectileSpeed);

        return true;
    }

    private bool FireInstantHit()
    {
        if (currentTarget == null)
        {
            return false;
        }

        currentTarget.TakeDamage(equippedWeapon.Damage);
        return true;
    }

    private Transform GetFirstValidFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
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
        if (firePoints == null || firePoints.Length == 0)
        {
            return transform;
        }

        for (int attempt = 0; attempt < firePoints.Length; attempt++)
        {
            int index = (nextFirePointIndex + attempt) % firePoints.Length;

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
        if (firePoints == null || firePoints.Length == 0)
        {
            return;
        }

        nextFirePointIndex = (nextFirePointIndex + 1) % firePoints.Length;
    }

    private void OnDestroy()
    {
        StopBeam(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (equippedWeapon != null)
        {
            Gizmos.DrawWireSphere(transform.position, equippedWeapon.AttackRange);
        }
    }
}