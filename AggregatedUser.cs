using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YelpAcademicSet
{
    class AggregatedUser
    {
       
        public AggregatedUser(User user)
        {
            // TODO: Complete member initialization
            this.Id = user.Id;
            this.Reviews = new List<Review>();
            this.ReviewsHome = 0;
            this.ReviewsForeign = 0;
            this.ReviewsObserved = 0;
            this.ReviewCount = user.ReviewCount;
            this.TotalReviewsObserved = user.Reviews.Count;
            this.Hometown = user.Hometown;
            this.YelpingSince = user.YelpingSince;
            this.AvgStars = user.AvgStars;
            this.VotedOnlyInHome = user.VotedOnlyInHome;
            this.ReviewsInHome = user.ReviewsInHome;
            this.TipsInHome = user.TipsInHome;
        }

        public int TotalReviewsObserved { get; set; }
        public int ReviewsObserved { get; set; }
        public int ReviewsHome { get; set; }
        public int ReviewsForeign { get; set; }
        public string Id { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Tip> Tips { get; set; }
        public string ReviewCount { get; set; }
        public string AvgStars { get; set; }
        public string YelpingSince { get; set; }
        public string Fans { get; set; }
        public string Hometown { get; set; }
        public int ReviewsInHome { get; set; }
        public int TipsInHome { get; set; }
        public int VotedOnlyInHome { get; set; }
        public double ObservedAvg { get; set; }
        public string Date { get; set; }

        public int NumbYearsElite { get; set; }

        public static AggregatedUser GetCumulative(User user, DateTime d)
        {
            var newUser = new AggregatedUser(user);
            if (user.Reviews.Count == 0) return newUser;
            user.Reviews.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
            var cumSum = 0.0;
            var cumHome = 0;
            var cumForeign = 0;
            var cumReviews = 0;
            var observations = false;
            newUser.Date = d.ToString("yyyy-MM-dd");


            foreach(var rev in user.Reviews.Where(x=>d>DateTime.Parse(x.Date)))
            {
                observations = true;
                cumSum += Convert.ToDouble(rev.Stars);
                cumHome += rev.IsInHome;
                cumForeign += 1 - rev.IsInHome;
                cumReviews++;

            }
            if (observations)
            {
                newUser.ObservedAvg = cumSum / cumReviews;
                newUser.ReviewsHome = cumHome;
                newUser.ReviewsForeign = cumForeign;
                newUser.ReviewsObserved = cumReviews;
            }

            return newUser;
        }
    }
}
