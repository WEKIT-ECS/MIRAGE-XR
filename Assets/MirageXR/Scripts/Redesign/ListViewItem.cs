
using UnityEngine;

namespace MirageXR
{
    public abstract class ListViewItem<T> : MonoBehaviour
    {
        public T Content { get; protected set; }

        public int Index { get; protected set; }

        public virtual void ShowData(T data, int index)
        {
            Content = data;
            Index = index;
        }
    }
}