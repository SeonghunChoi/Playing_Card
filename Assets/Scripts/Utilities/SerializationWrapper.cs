using System;
using System.Collections.Generic;

namespace PlayingCard.Utilities
{
    [Serializable]
    public class SerializationWrapper<T>
    {
        public List<T> List;

        public SerializationWrapper(List<T> list)
        {
            this.List = list;
        }

        public List<T> ToList() => List;
    }
}
