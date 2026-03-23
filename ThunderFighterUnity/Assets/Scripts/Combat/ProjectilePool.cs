using System.Collections.Generic;
using UnityEngine;

namespace ThunderFighter.Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        [System.Serializable]
        public class PoolEntry
        {
            public Projectile prefab;
            public int prewarmCount = 24;
            public bool expandable = true;
        }

        [SerializeField] private PoolEntry[] entries;

        private readonly Dictionary<int, Queue<Projectile>> available = new Dictionary<int, Queue<Projectile>>();
        private readonly Dictionary<int, PoolEntry> entryByKey = new Dictionary<int, PoolEntry>();
        private readonly Dictionary<int, int> instanceToKey = new Dictionary<int, int>();

        private static ProjectilePool instance;
        public static ProjectilePool Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (entries == null)
            {
                return;
            }

            foreach (PoolEntry entry in entries)
            {
                if (entry.prefab == null)
                {
                    continue;
                }

                int key = entry.prefab.GetInstanceID();
                entryByKey[key] = entry;
                if (!available.ContainsKey(key))
                {
                    available[key] = new Queue<Projectile>();
                }

                for (int i = 0; i < Mathf.Max(0, entry.prewarmCount); i++)
                {
                    Projectile projectile = CreateNew(entry.prefab, key);
                    projectile.gameObject.SetActive(false);
                    available[key].Enqueue(projectile);
                }
            }
        }

        public Projectile Spawn(Projectile prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            int key = prefab.GetInstanceID();
            if (!available.ContainsKey(key))
            {
                return null;
            }

            Projectile projectile;
            if (available[key].Count > 0)
            {
                projectile = available[key].Dequeue();
            }
            else
            {
                PoolEntry entry = entryByKey[key];
                if (!entry.expandable)
                {
                    return null;
                }

                projectile = CreateNew(prefab, key);
            }

            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public static bool TryRecycle(Projectile projectile)
        {
            if (instance == null || projectile == null)
            {
                return false;
            }

            int instanceId = projectile.GetInstanceID();
            if (!instance.instanceToKey.TryGetValue(instanceId, out int key))
            {
                return false;
            }

            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(instance.transform);
            instance.available[key].Enqueue(projectile);
            return true;
        }

        private Projectile CreateNew(Projectile prefab, int key)
        {
            Projectile projectile = Instantiate(prefab, transform);
            instanceToKey[projectile.GetInstanceID()] = key;
            return projectile;
        }
    }
}
