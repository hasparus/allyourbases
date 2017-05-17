namespace Sophie.Utils
{
    public class Hash<TKey, TValue> : SerializableDictionary<TKey, TValue?> where TValue : struct
    {
        public override TValue? this[TKey key]
        {
            get => this[key, null];
            set => Insert(key, value, false);
        }
    }
}