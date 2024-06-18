using System;

namespace RhythmGame
{
    [Serializable]
    public class SerializedKeyValue<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public SerializedKeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
