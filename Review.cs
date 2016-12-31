using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YelpAcademicSet
{
    public class Review
    {
        public string Id { get; set; }
        public string Stars { get; set; }
        public Business Biz { get; set; }
        public User User { get; set; }
        public int IsInHome { get; set; }
        public int Travelled { get; set; }

        public int Funny { get; set; }
        public int Useful { get; set; }
        public int Cool { get; set; }

        public int Length { get; set; }
        public string Date { get; set; }
        public double CumulatedBusinessAvg { get; set; }
        public int CumulatedBusinessReviewCount { get; set; }

        public int ReviewsUserPostedInCitySoFar { get; set; }
        public double CumulatedUserAvg { get; set; }
        public int CumulatedUserReviewCount { get; set; }

        public int FirstTimeReview { get; set; }

        
        public void DeleteRev()
        {
            Biz.DeleteReview(Id);
        }

    }
}
