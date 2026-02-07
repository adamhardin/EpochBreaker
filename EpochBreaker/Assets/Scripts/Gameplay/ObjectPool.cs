using UnityEngine;
using System.Collections.Generic;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Lightweight object pool. Eliminates runtime Instantiate/Destroy for
    /// high-frequency objects (projectiles, particles, flashes).
    /// Auto-initializes on first Get call. Call ReturnAll() on level transitions.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private static ObjectPool s_instance;

        private readonly Stack<GameObject> _projectiles = new();
        private readonly Stack<GameObject> _particles = new();
        private readonly Stack<GameObject> _flashes = new();
        private readonly HashSet<GameObject> _active = new();

        private const int PROJECTILE_PREALLOC = 30;
        private const int PARTICLE_PREALLOC = 50;
        private const int FLASH_PREALLOC = 8;

        public static void EnsureInitialized()
        {
            if (s_instance != null) return;
            var go = new GameObject("[ObjectPool]");
            DontDestroyOnLoad(go);
            s_instance = go.AddComponent<ObjectPool>();
            s_instance.Preallocate();
        }

        private void Preallocate()
        {
            for (int i = 0; i < PROJECTILE_PREALLOC; i++)
                _projectiles.Push(CreateRaw(PoolCategory.Projectile));
            for (int i = 0; i < PARTICLE_PREALLOC; i++)
                _particles.Push(CreateRaw(PoolCategory.Particle));
            for (int i = 0; i < FLASH_PREALLOC; i++)
                _flashes.Push(CreateRaw(PoolCategory.Flash));
        }

        public static GameObject GetProjectile()
        {
            EnsureInitialized();
            var pool = s_instance._projectiles;
            var go = pool.Count > 0 ? pool.Pop() : s_instance.CreateRaw(PoolCategory.Projectile);
            Activate(go);
            return go;
        }

        public static GameObject GetParticle()
        {
            EnsureInitialized();
            var pool = s_instance._particles;
            var go = pool.Count > 0 ? pool.Pop() : s_instance.CreateRaw(PoolCategory.Particle);
            Activate(go);
            return go;
        }

        public static GameObject GetFlash()
        {
            EnsureInitialized();
            var pool = s_instance._flashes;
            var go = pool.Count > 0 ? pool.Pop() : s_instance.CreateRaw(PoolCategory.Flash);
            Activate(go);
            return go;
        }

        public static void Return(GameObject go)
        {
            if (go == null || s_instance == null) return;
            if (!go.activeSelf) return;

            go.SetActive(false);
            go.transform.SetParent(s_instance.transform);
            s_instance._active.Remove(go);

            var tag = go.GetComponent<PoolTag>();
            if (tag == null) { Object.Destroy(go); return; }

            switch (tag.Category)
            {
                case PoolCategory.Projectile: s_instance._projectiles.Push(go); break;
                case PoolCategory.Particle:   s_instance._particles.Push(go); break;
                case PoolCategory.Flash:      s_instance._flashes.Push(go); break;
            }
        }

        /// <summary>
        /// Return all active pooled objects. Call on level load/transition.
        /// </summary>
        public static void ReturnAll()
        {
            if (s_instance == null) return;
            foreach (var go in s_instance._active)
            {
                if (go == null) continue;
                go.SetActive(false);
                go.transform.SetParent(s_instance.transform);

                var tag = go.GetComponent<PoolTag>();
                if (tag == null) continue;
                switch (tag.Category)
                {
                    case PoolCategory.Projectile: s_instance._projectiles.Push(go); break;
                    case PoolCategory.Particle:   s_instance._particles.Push(go); break;
                    case PoolCategory.Flash:      s_instance._flashes.Push(go); break;
                }
            }
            s_instance._active.Clear();
        }

        private static void Activate(GameObject go)
        {
            go.transform.SetParent(null);
            go.SetActive(true);
            s_instance._active.Add(go);
        }

        private GameObject CreateRaw(PoolCategory category)
        {
            var go = new GameObject($"Pooled_{category}");
            go.transform.SetParent(transform);

            var tag = go.AddComponent<PoolTag>();
            tag.Category = category;

            switch (category)
            {
                case PoolCategory.Projectile:
                    go.AddComponent<SpriteRenderer>();
                    var rb = go.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.gravityScale = 0f;
                    var col = go.AddComponent<CircleCollider2D>();
                    col.isTrigger = true;
                    go.AddComponent<Projectile>();
                    break;

                case PoolCategory.Particle:
                    go.AddComponent<SpriteRenderer>();
                    go.AddComponent<Rigidbody2D>();
                    go.AddComponent<PoolTimer>();
                    break;

                case PoolCategory.Flash:
                    go.AddComponent<SpriteRenderer>();
                    go.AddComponent<PoolTimer>();
                    break;
            }

            go.SetActive(false);
            return go;
        }
    }

    public enum PoolCategory { Projectile, Particle, Flash }

    public class PoolTag : MonoBehaviour
    {
        public PoolCategory Category;
    }

    /// <summary>
    /// Auto-returns a pooled object after a duration elapses.
    /// </summary>
    public class PoolTimer : MonoBehaviour
    {
        private float _timer;
        private bool _running;

        public void StartTimer(float duration)
        {
            _timer = duration;
            _running = true;
        }

        private void OnDisable()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _running = false;
                ObjectPool.Return(gameObject);
            }
        }
    }
}
