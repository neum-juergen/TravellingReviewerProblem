using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YelpAcademicSet
{
    public class Business
    {
        public string City { get; set; }
        public string Id { get; set; }
        public string ReviewCount { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Open { get; set; }
        public string Stars { get; set; }

        public string Categories { get; set; }
        public List<Review> Revs { get; set; }
        public int ReviewsObserved { get; set; }
        public int ReviewsHome { get; set; }
        public int ReviewsForeign { get; set; }
        public int ReviewsFunny { get; set; }
        public int ReviewsCool { get; set; }
        public int ReviewsUseful { get; set; }

        public int FirstTimeReviews { get; set; }
        public double ObservedAvg { get; set; }
        public string ZIPCode { get; set; }
        public int ReviewsTravelled { get; set; }

        public double distanceToHome { get; set; }

        public void DetermineAggregatedValues()
        {
            var count = Convert.ToDouble(ReviewCount);
            
            if (Revs == null) Revs = new List<Review>();
            ReviewsObserved = Revs.Count;
            var sum = 0.0;
            foreach (var Rev in Revs)
            {
                sum += Convert.ToInt32(Rev.Stars);
                if (Rev.IsInHome == 1)
                    ReviewsHome += 1;
                else
                    ReviewsForeign += 1;
                ReviewsCool += Rev.Cool;
                ReviewsFunny += Rev.Funny;
                ReviewsUseful += Rev.Useful;
                ReviewsTravelled += Rev.Travelled;
                FirstTimeReviews += Rev.FirstTimeReview;
                count--;
            }
            ObservedAvg = sum / Revs.Count;
        }
        public void DeleteReview(string id)
        {
            Revs.RemoveAll(item => item.Id.Equals(id));
        }
    }
}
