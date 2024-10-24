using System.Collections.Generic;

namespace Utils
{
    public class HashSetPool<T>
    {
        private readonly List<HashSet<T>> m_Pool = new List<HashSet<T>>();
        private const int m_MinCapacity = 10;
        
        public HashSet<T> GetOrCreateObject()
        {
            if (m_Pool.Count == 0)
            {
                CreateObject();
            }

            var result = m_Pool[^1];
            m_Pool.RemoveAt(m_Pool.Count - 1);
            result.Clear();
            return result;
        }

        private void CreateObject()
        {
            m_Pool.Add(new HashSet<T>(m_MinCapacity));
        }

        public void ReleaseObject(HashSet<T> hashSet)
        {
            hashSet.Clear();
            m_Pool.Add(hashSet);
        }

        public void Clear()
        {
            m_Pool.Clear();
        }
    }
}