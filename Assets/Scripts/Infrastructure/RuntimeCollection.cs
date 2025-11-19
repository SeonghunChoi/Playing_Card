using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.Infrastructure
{
    /// <summary>
    /// 특정 타입의 리스트를 포함하는 ScriptableObject 클래스.
    /// 이 ScriptableObject의 인스턴스는 시스템 간에 직접적인 참조 없이 컴포넌트에서 참조할 수 있다.
    /// </summary>
    public abstract class RuntimeCollection<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();

        public event Action<T> ItemAdded;
        public event Action<T> ItemRemoved;

        public virtual void Add(T item)
        {
            if (!Items.Contains(item))
            {
                Items.Add(item);
                ItemAdded?.Invoke(item);
            }
        }

        public virtual void Remove(T item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                ItemRemoved?.Invoke(item);
            }
        }
    }
}
