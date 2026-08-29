using UnityEngine;

public class EnemyBaseShip : MonoBehaviour
{
    private EnemyBaseSpawner spawner;
    private int lane;
    private bool destructionReported;

    public void Initialize(EnemyBaseSpawner newSpawner, int newLane)
    {
        spawner = newSpawner;
        lane = newLane;
    }

    public void NotifyDestroyed()
    {
        if (destructionReported)
        {
            return;
        }

        destructionReported = true;

        if (spawner != null)
        {
            spawner.NotifyBaseDestroyed(lane);
        }
    }
}