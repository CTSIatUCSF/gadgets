using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatterService.Model
{
    public class DeclumpedList : List<Activity>
    {
        private Random random = new Random();
        private List<List<Activity>> clumpedData = new List<List<Activity>>();

        public void Clump()
        {
            // clear ours out first
            clumpedData.Clear();
            int id = -1;
            List<Activity> clumpedList = null;

            foreach (Activity activity in this)
            {
                if (id != activity.ParentId)
                {
                    //store the old clumpedList
                    if (clumpedList != null)
                    {
                        clumpedData.Add(clumpedList);
                    }
                    // create a new clump
                    clumpedList = new List<Activity>();
                    id = activity.ParentId;
                }
                clumpedList.Add(activity);
            }
            // ad the last one
            if (clumpedList != null)
            {
                clumpedData.Add(clumpedList);
            }
        }

        public List<Activity> TakeUnclumped(int count)
        {
            List<Activity> retval = new List<Activity>();
            foreach(List<Activity> clumpedList in clumpedData)
            {
                // add a random one
                retval.Add(clumpedList[random.Next(0, clumpedList.Count)]);
                if (retval.Count == count)
                {
                    break;
                }
            }
            return retval;
        }
    }
}