namespace Sophie.Utils
{
    public class ReferenceHash<TKey, TValue> : SerializableDictionary<TKey, TValue> where TValue : class
    {
        public override TValue this[TKey key]
        {
            get => this[key, null];
            set => Insert(key, value, false);
        }
    }
}