namespace Client.Common
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
	using System;

    public static class SystemExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
