using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace YelpAcademicSet
{
    class Program
    {
        static int reviewId = 0;
        static Dictionary<String, User> _users = new Dictionary<String, User>();
        static Dictionary<String, Business> _businesses = new Dictionary<String, Business>();
        static List<AggregatedBusiness> _aggregatedBusinesses = new List<AggregatedBusiness>();
        static Dictionary<String, String> zip_city_map = new Dictionary<string, string>();

        private static void fillCityMap()
        {
            System.IO.StreamReader file =new System.IO.StreamReader(".\\Master\\zip_list.csv");
            var line = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                try
                {


                    var s = line.Split(';');
                    if(!zip_city_map.ContainsKey(s[0]))
                        zip_city_map.Add(s[0], s[1]+", "+s[2]);

                }
                catch (JsonReaderException) { }
                catch (RuntimeBinderException) { }

            }
            file.Close();

        }
        private static void _handleUser(string line)
        {
            dynamic stuff = JsonConvert.DeserializeObject(line);
            if (stuff!=null && stuff.type == "user")
            {
                _users[stuff.user_id.ToString()] = (new User
                {
                    Reviews = new List<Review>(),
                    Tips = new List<Tip>(),
                    AvgStars = stuff.average_stars,
                    ReviewCount = stuff.review_count,
                    Fans = stuff.fans,
                    Id = stuff.user_id,
                    YelpingSince = stuff.yelping_since,
                    NumbYearsElite = stuff.elite.Count
                });
                Console.WriteLine("Users:" + _users.Keys.Count);
            }
            if (stuff!=null && stuff.type == "business")
            {
                    //var cats = new List<string>();
                    //for (var i = 0; i < 10;i++ )
                      //  cats.Add("");
                    //for (var i = 0; i < stuff.categories.Count; i++)
                      //  cats[i] += stuff.categories[i];
                        var realCat = "";
                        foreach (var cat in stuff.categories)
                            realCat = cat;
                        var zip = stuff.full_address.ToString().Substring(stuff.full_address.ToString().Length - 5, 5);
                        var realCity = stuff.city;
                        if (zip_city_map.ContainsKey(zip))
                            realCity = zip_city_map[zip];
                        _businesses[stuff.business_id.ToString()] = (new Business
                        {
                            Stars = stuff.stars,
                            ReviewCount = stuff.review_count,
                            Latitude = stuff.latitude,
                            Id = stuff.business_id,
                            Longitude = stuff.longitude,
                            City = realCity,
                            Open = stuff.open,
                            Categories = realCat,
                            ZIPCode = stuff.full_address.ToString().Substring(stuff.full_address.ToString().Length-5,5)
                        });
                    Console.WriteLine("Businesses:" + _businesses.Keys.Count);
                    
            }
            
            
        }
        private static int writtenReviews = 0;
        private static int numbRev = 0;
        private static void _handleRev(string line)
        {
            dynamic stuff = JsonConvert.DeserializeObject(line);
            if (stuff.type == "review")
            {
                
                if (_users.ContainsKey(stuff.user_id.ToString()) && _businesses.ContainsKey(stuff.business_id.ToString()))
                {
                    var rev = new Review { Length = stuff.text.ToString().Length, Cool = Convert.ToInt32(stuff.votes.cool), Funny = Convert.ToInt32(stuff.votes.funny), Useful = Convert.ToInt32(stuff.votes.useful), Date = stuff.date, Stars = stuff.stars, Biz = _businesses[stuff.business_id.ToString()], Id = reviewId.ToString(), User = _users[stuff.user_id.ToString()] };
                    _users[stuff.user_id.ToString()].Reviews.Add(rev);
                    if (rev.Biz != null)
                    {
                        if (rev.Biz.Revs == null) rev.Biz.Revs = new List<Review>();
                        rev.Biz.Revs.Add(rev);
                    }
                    
                }
                reviewId++;
                numbRev++;
                Console.WriteLine("Reviews:" + numbRev);
            }
            if (stuff.type == "tip")
            {
                if (_users.ContainsKey(stuff.user_id.ToString()) && _businesses.ContainsKey(stuff.business_id.ToString()))
                {
                    var tip = new Tip { Biz = _businesses[stuff.business_id.ToString()], Date = stuff.date };
                    _users[stuff.user_id.ToString()].Tips.Add(tip);
                }
                numbTip++;
                Console.WriteLine("Tips:" + numbTip);
            }
            
        }
        private static int numbTip = 0;

        private static void writeAll(StreamWriter userWriter, StreamWriter businessWriter, StreamWriter reviewWriter, StreamWriter aggregatedBusinessWriter)
        {


            foreach (var uId in _users.Keys)
            {
                var user = _users[uId];
                user.DetermineHometown();
               
                if ( user.TipsInHome+user.ReviewsInHome<3 ||user.ReviewsInHome * 1.0 / Convert.ToDouble(user.ReviewCount) < 0.5 || !user.CheckDayDifference(360)) user.WriteUser = user.DeleteHome();
                if(user.WriteUser)
                {
                    user.Reviews.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
                    var cumSum = 0.0;
                    var cumHome = 0;
                    var cumForeign = 0;
                    var cumReviews = 0;

                    var content = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11}", user.Id, user.ReviewCount, user.AvgStars, user.YelpingSince, user.Fans, user.Hometown, user.ReviewsInHome, user.TipsInHome, user.VotedOnlyInHome, user.NumbYearsElite, user.Reviews.Count, user.HometownBasedOnTips);
                    userWriter.WriteLine(content);
                    userWriter.Flush();
                    Review lastRev = null;
                    foreach (var rev in user.Reviews)
                    {
                        if (cumReviews > 0)
                            rev.CumulatedUserAvg = cumSum / cumReviews;
                        else
                            rev.CumulatedUserAvg = 0.0;
                        rev.CumulatedUserReviewCount = cumReviews;

                        cumSum += Convert.ToDouble(rev.Stars);
                        cumHome += rev.IsInHome;
                        cumForeign += 1 - rev.IsInHome;
                        cumReviews++;

                        if(lastRev!=null && !lastRev.Biz.City.Equals(rev.Biz.City))
                            rev.Travelled = 1;
                        lastRev = rev;
                    }
                }
            }
            Console.WriteLine("Written users.");
            foreach (var bId in _businesses.Keys)
            {
                var biz = _businesses[bId];
                biz.DetermineAggregatedValues();
                _aggregatedBusinesses.AddRange(AggregatedBusiness.TransformBusiness(biz));

                biz.Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
                var cumSum = 0.0;
                var cumHome = 0;
                var cumForeign = 0;
                var cumReviews = 0;
                var content = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17}", biz.Id, biz.City, biz.Latitude, biz.Longitude, biz.Stars, biz.ReviewCount, biz.Open, biz.Categories, biz.ReviewsObserved, biz.ObservedAvg, biz.ReviewsHome, biz.ReviewsForeign, biz.ZIPCode, biz.ReviewsCool, biz.ReviewsFunny, biz.ReviewsUseful, biz.ReviewsTravelled, biz.FirstTimeReviews);
                businessWriter.WriteLine(content);
                businessWriter.Flush();

                foreach (var rev in biz.Revs)
                {
                    if (cumReviews > 0)
                        rev.CumulatedBusinessAvg = cumSum / cumReviews;
                    else
                        rev.CumulatedBusinessAvg = 0.0;
                    rev.CumulatedBusinessReviewCount = cumReviews;

                    cumSum += Convert.ToDouble(rev.Stars);
                    cumHome += rev.IsInHome;
                    cumForeign += 1 - rev.IsInHome;
                    cumReviews++;

                    var content2 = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15}", rev.Biz.Id, rev.User.Id, rev.Stars, rev.Date, rev.IsInHome, rev.CumulatedBusinessAvg, rev.CumulatedBusinessReviewCount, rev.CumulatedUserAvg, rev.CumulatedUserReviewCount, rev.Length, rev.Cool, rev.Funny, rev.Useful, rev.Travelled, rev.FirstTimeReview, rev.ReviewsUserPostedInCitySoFar);
                    reviewWriter.WriteLine(content2);
                    reviewWriter.Flush();
                    writtenReviews++;
                }
            }
            Console.WriteLine("Written businesses and reviews.");
            foreach (var biz in _aggregatedBusinesses)
            {
                var content = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13}", biz.Id, biz.City, biz.Latitude, biz.Longitude, biz.TotalReviewsObserved, biz.ReviewCount, biz.Open, biz.Categories, biz.ReviewsObserved, biz.ObservedAvg, biz.ReviewsHome, biz.ReviewsForeign, biz.ZIPCode,biz.Date);
                aggregatedBusinessWriter.WriteLine(content);
                aggregatedBusinessWriter.Flush();
            }
        }
        static void Main(string[] args)
        {
            
            fillCityMap();
            readFile(".\\Master\\yelp_academic_dataset_business.json", _handleUser);
            readFile(".\\Master\\yelp_academic_dataset_user.json", _handleUser);
            Console.WriteLine("Users and businesses initialized");
            readFile(".\\Master\\yelp_academic_dataset_tip.json",_handleRev);
            readFile(".\\Master\\yelp_academic_dataset_review.json", _handleRev);
            Console.WriteLine("Reviews and tips initiated");
 
            // Read the file and display it line by line.
            
            var reviewWriter = new StreamWriter(".\\Master\\reviews.csv");
            var businessWriter = new StreamWriter(".\\Master\\businesses.csv");
            var userWriter = new StreamWriter(".\\Master\\users.csv");
            var aggregatedBusinessWriter = new StreamWriter(".\\Master\\aggregated_businesses.csv");
            reviewWriter.WriteLine("Business;User;Review_Stars;Date;In_Hometown;Cumulated_Business_Avg;Cumulated_Business_Review_Count;Cumulated_User_Avg;Cumulated_User_Review_Count;Length;Votes_Cool;Votes_Funny;Votes_Useful;Travelled;First_Time_Review;Reviews_In_City_So_Far");
            reviewWriter.Flush();
            businessWriter.WriteLine("Business;City;Latitude;Longitude;Business_Stars;Business_Review_Count;Open;Category;Reviews_Observed;Observed_Avg;Reviews_Home;Reviews_Foreign;ZIP_Code;Business_Cool_Votes;Business_Funny_Votes;Business_Useful_Votes;Reviews_Travelled;Reviews_First_Time");
            businessWriter.Flush();
            aggregatedBusinessWriter.WriteLine("Business;City;Latitude;Longitude;Total_Reviews_Observed;Business_Review_Count;Open;Category;Reviews_Observed;Observed_Avg;Reviews_Home;Reviews_Foreign;ZIP_Code;Date");
            businessWriter.Flush();
            userWriter.WriteLine("User;User_Review_Count;User_Avg_Stars;Yelping_Since;Fans;Hometown;Reviews_In_Hometown;Tips_In_Hometown;Voted_Only_Home;Years_Elite;Reviews_Observed;Hometown_Based_On_Tips");
            userWriter.Flush();

            writeAll(userWriter, businessWriter, reviewWriter, aggregatedBusinessWriter);
           

            reviewWriter.Close();
            businessWriter.Close();
            userWriter.Close();
            aggregatedBusinessWriter.Close();
            Console.WriteLine("Written reviews:" + writtenReviews);
            Console.ReadLine();
        }

        public static void readFile(string fileName, Action<string> handleLine)
        {
            string line;
            System.IO.StreamReader file =
              new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    
                    
                    handleLine(line);

                }
                catch (JsonReaderException) { Console.WriteLine(line); }
                catch (RuntimeBinderException) { Console.WriteLine(line); }

            }
            file.Close();
        }


        
    }
}
