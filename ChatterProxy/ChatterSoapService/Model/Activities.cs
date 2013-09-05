using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatterService.Model
{
    class Activities
    {
        public readonly int RequestedCount;
        public readonly List<Activity> items;

        public Activities(int reqCount, List<Activity> activities)
        {
            RequestedCount = reqCount;
            items = activities;
        }

        public Activity[] GetRandomList(int count)
        {
            SortedDictionary<int, Activity> list = new SortedDictionary<int, Activity>();
            
            Random random = new Random();
            for (int i = 0; i < count && i < items.Count; i++)
            {
                Activity act = items[i];
                int key = random.Next(items.Count);
                while (list.ContainsKey(key))
                {
                    key = random.Next(count * 10);
                }

                list.Add(key, act);
            }

            return list.Values.ToArray();
        }
    }
}
