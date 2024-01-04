using UnityEngine;

public class ProjectileQueueItem
{
    public int amount;
    public Projectile projectile;

    public ProjectileQueueItem(int amount, Projectile projectile)
    {
        this.amount = amount;
        this.projectile = projectile;
    }
}
