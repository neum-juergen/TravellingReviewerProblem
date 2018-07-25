using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YelpAcademicSet
{
    class AggregatedBusiness
    {
        

        public AggregatedBusiness(Business biz)
        {
            // TODO: Complete member initialization
            this.City = biz.City;
            this.Id = biz.Id;
            this.Latitude = biz.Latitude;
            this.Longitude = biz.Longitude;
            this.Open = biz.Open;
            this.ZIPCode = biz.ZIPCode;
            this.Categories = biz.Categories;
            this.Reviews = new List<Review>();
            this.ReviewsHome = 0;
            this.ReviewsForeign = 0;
            this.ReviewsObserved = 0;
            this.ReviewCount = biz.ReviewCount;
            this.TotalReviewsObserved = biz.ReviewsObserved;
        }

        public AggregatedBusiness()
        {
            this.Reviews = new List<Review>();
            this.ReviewsHome = 0;
            this.ReviewsForeign = 0;
            this.ReviewsObserved = 0;
            // TODO: Complete member initialization
        }
        public string City { get; set; }
        public string Id { get; set; }
        public string ReviewCount { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Open { get; set; }
        public string Categories { get; set; }
        public int TotalReviewsObserved { get; set; }
        public int ReviewsObserved { get; set; }
        public int ReviewsHome { get; set; }
        public List<Review> Reviews { get; set; }
        public int ReviewsForeign { get; set; }
        public double ObservedAvg { get; set; }
        public string ZIPCode { get; set; }
        public string Date { get; set; }

        public static List<AggregatedBusiness> TransformBusiness(Business biz)
        {
            var result = new List<AggregatedBusiness>();
            if (biz.Revs.Count == 0) return result;
            var d = DateTime.Parse("2010-01-01");
            var d2 = DateTime.Parse("2016-01-01");
            biz.Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
            var date = DateTime.Parse(biz.Revs[0].Date);
            int c = 0;
            var cumSum = 0.0;
            var cumHome = 0;
            var cumForeign = 0;
            var cumReviews = 0;
            var observations = false;
            var lastBiz = new AggregatedBusiness();
            while (d<=d2 & c<biz.Revs.Count)
            {
                var newBiz = new AggregatedBusiness(biz);
                newBiz.Date = d.ToString("yyyy-MM-dd");

                var reviewsInMonth = 0;
                var foreignInMonth = 0;
                var homeInMonth = 0;
                var sumInMonth = 0.0;
                
                while (date <= d & c<biz.Revs.Count)
                {
                    observations = true;
                    cumSum += Convert.ToDouble(biz.Revs[c].Stars);
                    cumHome += biz.Revs[c].IsInHomeArea;
                    cumForeign += 1-biz.Revs[c].IsInHomeArea;
                    cumReviews++;
                    sumInMonth += Convert.ToDouble(biz.Revs[c].Stars);
                    homeInMonth += biz.Revs[c].IsInHomeArea;
                    foreignInMonth = 1 - biz.Revs[c].IsInHomeArea;
                    reviewsInMonth++;
                    c++;
                    if(c<biz.Revs.Count)
                        date = DateTime.Parse(biz.Revs[c].Date);
                }
                if(cumReviews>0)
                    newBiz.ObservedAvg = cumSum / cumReviews;
                newBiz.ReviewsHome = cumHome;
                newBiz.ReviewsForeign = cumForeign;
                newBiz.ReviewsObserved = cumReviews;
                newBiz.ReviewsInMonth = reviewsInMonth;
                if(reviewsInMonth>0)
                    newBiz.AvgInMonth = sumInMonth / reviewsInMonth;
                if (newBiz.AvgInMonth > 5) throw new Exception();
                newBiz.ForeignInMonth = foreignInMonth;
                newBiz.HomeInMonth = homeInMonth;
                d = d.AddMonths(1);
                if (observations)
                {
                    result.Add(newBiz);
                    lastBiz = newBiz;
                }
            }
            return result;
        }

        public static AggregatedBusiness GetCumulative(Business biz, DateTime d)
        {
            var newBiz = new AggregatedBusiness(biz);
            if (biz.Revs.Count == 0) return newBiz;
            biz.Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
            var cumSum = 0.0;
            var cumHome = 0;
            var cumForeign = 0;
            var cumReviews = 0;
            var observations = false;
            newBiz.Date = d.ToString("yyyy-MM-dd");



            foreach (var rev in biz.Revs.Where(x => d > DateTime.Parse(x.Date)))
                {
                    observations = true;
                    cumSum += Convert.ToDouble(rev.Stars);
                    cumHome += rev.IsInHome;
                    cumForeign += 1 - rev.IsInHome;
                    cumReviews++;
                }
                if (observations)
                {
                    newBiz.ObservedAvg = cumSum / cumReviews;
                    newBiz.ReviewsHome = cumHome;
                    newBiz.ReviewsForeign = cumForeign;
                    newBiz.ReviewsObserved = cumReviews;
                }
            
            return newBiz;
        }

        public int ReviewsInMonth { get; set; }

        public double AvgInMonth { get; set; }

        public int ForeignInMonth { get; set; }

        public int HomeInMonth { get; set; }
    }
}
