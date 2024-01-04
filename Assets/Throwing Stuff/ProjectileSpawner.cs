using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    //  Settings include list of prefabs(?), maximum spawns at once, maximum wait time, cooldown period, spawn location spread
	//  Spawn points instantiate (1 per update) and hold queue of projectiles waiting

    [SerializeField][Tooltip("The maximum number of projectiles waiting to fire in a batch")] private int maximumSpawns = 1000;
    [SerializeField][Tooltip("Number of seconds a batch of projectiles will wait before firing")] private float maximumWaitTime = 3f;
    [SerializeField][Tooltip("Number of seconds before a new batch of projectiles will start loading")] private float cooldownPeriod = 3f;
    [SerializeField][Tooltip("The maximum number of projectiles spawned per frame")] private int maximumSpawnPerFrame = 10;
    [SerializeField][Tooltip("Radius of spawn spread for projectiles starting location")] private float spawnSpread = 0.2f;
    [SerializeField][Tooltip("List of projectiles available for random selection")] private Projectile[] projectileTypes;
    [SerializeField][Tooltip("Target to launch projectiles at")] private ProjectileTarget target;

    private Queue<ProjectileQueueItem> itemQueue;
    private Projectile[] waitingProjectiles;
    private ProjectileQueueItem nextProjectileItem;
    private bool batchStarted = false;
    private bool cooldown = false;
    private float batchTime = 0f;
    private float cooldownTime = 0f;
    private int batchSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        waitingProjectiles = new Projectile[maximumSpawns];
        itemQueue = new Queue<ProjectileQueueItem>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTimeCapped = Math.Min(Time.deltaTime, 0.2f);
        if(cooldown)
        {
            cooldownTime += deltaTimeCapped;
            if(cooldownTime > cooldownPeriod)
                cooldown = false;
        }
        else
        {
            int spawnedThisFrame = 0;
            while((itemQueue.Count > 0 || nextProjectileItem != null) && spawnedThisFrame < maximumSpawnPerFrame && batchSize < maximumSpawns)
            {
                if(!batchStarted)
                    batchStarted = true;
                if(nextProjectileItem == null)
                {
                    nextProjectileItem = itemQueue.Dequeue();
                }
                Projectile projectile = Instantiate(nextProjectileItem.projectile, transform.position + UnityEngine.Random.Range(-spawnSpread, spawnSpread) * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0), transform.rotation);
                if(--nextProjectileItem.amount < 1)
                {
                    nextProjectileItem = null;
                }
                waitingProjectiles[batchSize++] = projectile;
                spawnedThisFrame++;
            }
            if(batchStarted)
            {
                batchTime += deltaTimeCapped;
                if(batchSize >= maximumSpawns || batchTime > maximumWaitTime)
                {
                    for(int i = 0; i < batchSize; i++)
                    {
                        waitingProjectiles[i].GetComponent<Projectile>().launchProjectile(target);
                    }
                    batchSize = 0;
                    batchTime = 0f;
                    batchStarted = false;
                    waitingProjectiles = new Projectile[maximumSpawns];
                    cooldownTime = 0f;
                    cooldown = true;
                }
            }
        }
    }

    // Null projectile = random from projectileTypes array
    public int addProjectileToQueue(int amount = 1, Projectile projectile = null)
    {
        if(amount < 1)
            amount = 1;
        if(projectile == null)
        {
            if(projectileTypes == null)
                return -1;
            projectile = projectileTypes[UnityEngine.Random.Range(0, projectileTypes.Length)];
        }
        itemQueue.Enqueue(new ProjectileQueueItem(amount, projectile));
        return itemQueue.Count;
    }

    // Testing functions cause I don't know how Unity buttons and events work
    public void addRandomBatch(int amount)
    {
        addProjectileToQueue(amount);
    }

    public void addRandomBatches(int batches)
    {
        for(int i = 0; i < batches; i++)
            addProjectileToQueue(5);
    }
}
