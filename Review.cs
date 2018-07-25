using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YelpAcademicSet
{
    public class Review
    {
        public int ReviewsUserPostedInCityAreaSoFar {get; set;}
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

        public int TemporalContiguity { get; set; }

        public int IsInHomeArea {get; set;}
        public int TemporalDistance { get; set; }

        public int ReviewsUserPostedInCitySoFar { get; set; }
        public double CumulatedUserAvg { get; set; }
        public int CumulatedUserReviewCount { get; set; }

        public int FirstTimeReview { get; set; }
        public string Text { get; set; }
        public double CumulatedUserHomeAvg { get; set; }
        public double CumulatedUserForeignAvg { get; set; }
        public int CumulatedUserHomeCount { get; set; }
        public int CumulatedUserForeignCount { get; set; }
        public int MultiCatContiguity { get; set; }
        public double LagStandardDev { get; set; }
        public double Top20Avg { get; set; }
        public double Top20StdDev { get; set; }
        public int SocialConsumption { get; set; }
        public int RealContiguity { get; set; }
        public int RealDistance { get; set; }

        public void DeleteRev()
        {
            Biz.DeleteReview(Id);
        }

    }
}
