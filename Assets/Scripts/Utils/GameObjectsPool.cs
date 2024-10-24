using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class GameObjectsPool
    {
        private readonly List<GameObject> m_Pool = new List<GameObject>();
        private readonly GameObject m_Prefab;

        public GameObjectsPool(GameObject prefab)
        {
            m_Prefab = prefab;
        }

        public GameObject GetOrCreateObject()
        {
            if (m_Pool.Count == 0)
            {
                CreateObject();
            }

            var go = m_Pool[^1];
            m_Pool.RemoveAt(m_Pool.Count - 1);
            return go;
        }

        private void CreateObject()
        {
            var go = GameObject.Instantiate(m_Prefab);
            m_Pool.Add(go);
        }

        public void ReleaseObject(GameObject go)
        {
            m_Pool.Add(go);
        }

        public void Clear()
        {
            m_Pool.Clear();
        }
    }
}