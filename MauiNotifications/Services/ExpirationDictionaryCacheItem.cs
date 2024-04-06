namespace MauiNotifications.Services
{
    public class ExpirationDictionaryCacheItem<TValue>
    {
        public TValue Value { get; private set; }
        public DateTime InsertionDateTime { get; private set; }

        private ExpirationDictionaryCacheItem(TValue value, DateTime dtm)
        {
            Value = value;
            InsertionDateTime = dtm;
        }

        public static ExpirationDictionaryCacheItem<TValue> Create(TValue value, DateTime now)
        {
            return new ExpirationDictionaryCacheItem<TValue>(value, now);
        }
    }

}
