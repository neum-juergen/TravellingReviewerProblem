using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YelpAcademicSet
{
    public class User
    {
        public string Id { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Tip> Tips { get; set; }
        public string ReviewCount { get; set; }
        public string AvgStars { get; set; }
        public string YelpingSince { get; set; }
        public bool WriteUser { get; set; }
        public string Fans { get; set; }
        public string Hometown { get; set; }

        public string HometownBasedOnTips { get; set; }
        public int ReviewsInHome { get; set; }
        public int TipsInHome { get; set; }
        public int VotedOnlyInHome { get; set; }

        public int NumbYearsElite { get; set; }

        public User() { WriteUser = true; }
        public void DetermineHometown()
        {
            var dict = new Dictionary<string, int>();
            var dict2 = new Dictionary<string, int>();

            Reviews.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
            foreach (var review in Reviews)
            {
                var reviewThing = review.Biz.City;
                if (!dict.ContainsKey(reviewThing))
                {
                    dict[reviewThing] = 1;
                    review.FirstTimeReview = 1;
                    
                }
                else dict[reviewThing] = dict[reviewThing] + 1;
                review.ReviewsUserPostedInCitySoFar = dict[reviewThing]-1;
            }
            foreach (var tip in Tips)
            {
                var tipThing = tip.Biz.City;
                if (!dict2.ContainsKey(tipThing))
                    dict2[tipThing] = 1;
                else dict2[tipThing] = dict2[tipThing] + 1;
            }
            var dict3 = new Dictionary<string, int>();
            foreach(var key in dict.Keys){
                if (!dict3.ContainsKey(key))
                    dict3[key] = 0;
                dict3[key] = dict3[key] + dict[key];
            }
            foreach (var key in dict2.Keys)
            {
                if (!dict3.ContainsKey(key))
                    dict3[key] = 0;
                dict3[key] = dict3[key] + dict2[key];
            }
            string home = "";
            string homeBasedOnTips = "";
            int homerevs = 0;
            int hometips = 0;
            foreach (var key in dict.Keys)
            {
                if (dict[key] > homerevs)
                {
                    home = key;
                    homerevs = dict[key];
                }
            }
            foreach (var key in dict2.Keys)
            {
                if (dict2[key] > hometips)
                {
                    homeBasedOnTips = key;
                    hometips = dict2[key];
                }
            }

            Hometown = home;
            ReviewsInHome = homerevs;
            HometownBasedOnTips = homeBasedOnTips;

            if (dict2.ContainsKey(home))
                TipsInHome = dict2[home];
            else TipsInHome = 0;
            if (Reviews.Count == ReviewsInHome) VotedOnlyInHome = 1;
            else VotedOnlyInHome = 0;

            foreach (var rev in Reviews)
            {
                if (rev.Biz.City.Equals(Hometown)) { rev.IsInHome = 1; rev.FirstTimeReview = 0; }
                else rev.IsInHome = 0;
            }
            foreach (var tip in Tips)
                if (tip.Biz.City.Equals(Hometown)) tip.IsInHome = 1;
                else tip.IsInHome = 0;
            
        }

        public void DeleteUser()
        {
            foreach (var rev in Reviews)
            {
                rev.DeleteRev();
            }
        }

        public bool CheckDayDifference(int days)
        {
            var result = false;
            List<string> dates = new List<string>();
            foreach (var rev in Reviews.Where(x => x.IsInHome == 1))
                dates.Add(rev.Date);
            foreach (var tip in Tips.Where(x => x.IsInHome == 1))
                dates.Add(tip.Date);
                        
            foreach(var date1 in dates)
                foreach(var date2 in dates)
                    if (Math.Abs((DateTime.Parse(date1) - DateTime.Parse(date2)).Days) >= days)
                        result = true;
            return result;
            
        }


        public bool DeleteHome()
        {
            Hometown = "Unknown";
            HometownBasedOnTips = "Unknown";
            ReviewsInHome = -1;
            TipsInHome = -1;
            foreach (var rev in Reviews)
            {
                rev.IsInHome = 0;
            }
            return true;
        }

        
    }
}
