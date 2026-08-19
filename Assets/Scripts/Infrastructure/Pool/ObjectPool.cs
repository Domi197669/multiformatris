using UnityEngine;
using System.Collections.Generic;

namespace Multiformatris.Infrastructure.Pool
{
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();
        private readonly int _maxSize;

        public ObjectPool(GameObject prefab, Transform parent, int initialSize = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Object.Instantiate(_prefab, _parent);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public GameObject Get()
        {
            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(_prefab, _parent);
            }

            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);

            if (_pool.Count < _maxSize)
                _pool.Enqueue(obj);
            else
                Object.Destroy(obj);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                GameObject obj = _pool.Dequeue();
                if (obj != null)
                    Object.Destroy(obj);
            }
        }
    }
}
