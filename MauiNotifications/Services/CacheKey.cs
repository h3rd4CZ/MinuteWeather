namespace MauiNotifications.Services
{
    public class CacheKey
    {
        private readonly string key;

        private CacheKey(string key)
        {
            this.key = key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return String.Equals(key, ((CacheKey)obj).key, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public override string ToString()
        {
            return key;
        }

        public static CacheKey ForValueWith(string criterionType, int criterionValue)
        {
            return ForValueWith(criterionType, criterionValue.ToString());
        }

        public static CacheKey ForValueWith(string criterionType, string criterionValue)
        {
            if (String.IsNullOrEmpty(criterionType))
                throw new ArgumentNullException("criterionType");

            if (String.IsNullOrEmpty(criterionValue))
                throw new ArgumentNullException("criterionValue");

            string singleValueKey = criterionType + ":" + criterionValue;
            return new CacheKey(singleValueKey);
        }

        public static CacheKey ForAllValues()
        {
            const string allValuesKey = "$__specialKey:all";
            return new CacheKey(allValuesKey);
        }

        public static CacheKey ForValuesWhere(string criterionType, string criterionValue)
        {
            if (String.IsNullOrEmpty(criterionType))
                throw new ArgumentNullException("criterionType");

            if (String.IsNullOrEmpty(criterionValue))
                throw new ArgumentNullException("criterionValue");

            string filteredValuesKey = "$__specialKey:where:" + criterionType + ":" + criterionValue;
            return new CacheKey(filteredValuesKey);
        }

        public static CacheKey ForString(string key)
        {
            return new CacheKey(key);
        }

        public CacheKey CombineWith(CacheKey anotherKey)
        {
            string combinedKey = String.Format("{0}&{1}", key, anotherKey.key);
            return new CacheKey(combinedKey);
        }
    }
}
