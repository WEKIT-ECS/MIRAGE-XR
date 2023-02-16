using System.Collections.Generic;

namespace Obi
{

    /**
     * Simple pool to avoid allocating job handles at runtime. Only a small number of handles
     * are expected, so once a handle is borrowed from the pool it cannot be individually returned: all
     * borrowed handles are returned to the pool at the end of each step. 
     */
    public class JobHandlePool<T> where T : IObiJobHandle , new()
    {
        private List<T> pool;
        private int borrowedHandles;

        public JobHandlePool(int initialSize)
        {
            borrowedHandles = 0;
            pool = new List<T>(initialSize);
            for (int i = 0; i < initialSize; ++i)
                pool.Add(new T());
        }

        public T Borrow()
        {
            // expand pool if needed (no pool doubling, simply add one extra handle).
            if (borrowedHandles == pool.Count)
                pool.Add(new T());

            return pool[borrowedHandles++];
        }

        public void ReleaseAll()
        {
            borrowedHandles = 0;
            for (int i = 0; i < pool.Count; ++i)
                pool[i].Release();
        }
    }
}
