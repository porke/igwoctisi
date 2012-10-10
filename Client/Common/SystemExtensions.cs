namespace Client.Common
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    public static class SystemExtensions
    {
        public static void AddRange<T>(this Collection<T> collection, ICollection<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
