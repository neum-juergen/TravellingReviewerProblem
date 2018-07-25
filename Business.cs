using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YelpAcademicSet
{
    public class Business
    {
        public string City { get; set; }
        public string CityArea { get; set; }
        public string Id { get; set; }
        public string ReviewCount { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Open { get; set; }
        public string Stars { get; set; }
        public string DriveThru { get; set; }
        public string Corkage { get; set; }
        public string CounterService { get; set; }

       public string HairSpecialColor { get; set; }
        public string HairSpecialAfroAmerican { get; set; }
        public string HairSpecialCurly { get; set; }
        public string HairSpecialPerms { get; set; }
        public string HairSpecialKids { get; set; }
        public string HairSpecialExtensions { get; set; }
        public string HairSpecialAsian { get; set; }
        public string HairSpecialStraightPerms { get; set; }
        public string BikeParking {get; set;}
        public string AcceptsBitcoin {get; set;}
        public string AcceptsCreditCards {get; set;}
        public string ParkingGarage {get; set;}
        public string ParkingStreet {get; set;}
        public string ParkingValidated {get; set;}
        public string ParkingLot {get; set;}
        public string ParkingValet {get; set;}
        public string DogsAllowed {get; set;}
        public string PriceRange {get; set;}
        public string GoodForKids { get; set; }
        public string GoodForGroups { get; set; }
        public string GoodForDessert { get; set; }
        public string TakeOut { get; set; }
        public string Alcohol { get; set; }
        public string WiFi { get; set; }

        public string Catering { get; set; }
        public string HasTV { get; set; }
        public string NoiseLevel { get; set; }
        public string OutdoorSeating { get; set; }
        public string Reservations { get; set; }
        public string TableService { get; set; }
        public string ByAppointmentOnly { get; set; }
        public string GoodForLatenight { get; set; }

        public string GoodForDinner { get; set; }
        public string GoodForLunch { get; set; }
        public string GoodForBreakfast { get; set; }
        public string GoodForBrunch { get; set; }
        public string AmbienceRomantic { get; set; }
        public string AmbienceIntimate { get; set; }
        public string AmbienceClassy { get; set; }
        public string AmbienceHipster { get; set; }
        public string AmbienceDivey { get; set; }
        public string AmbienceTouristy { get; set; }
        public string AmbienceTrendy { get; set; }
        public string AmbienceUpscale { get; set; }
        public string AmbienceCasual { get; set; }
        public string Attire { get; set; }
        public string AcceptsInsurance { get; set; }
        public string BYOB { get; set; }
        public string BYOBCorkage { get; set; }
        public string HoursMonday { get; set; }
        public string HoursTuesday { get; set; }
        public string HoursWednesday { get; set; }
        public string HoursThursday { get; set; }
        public string HoursFriday { get; set; }
        public string HoursSaturday { get; set; }
        public string HoursSunday { get; set; }
        public string BestNightMonday { get; set; }
        public string BestNightTuesday { get; set; }
        public string BestNightWednesday { get; set; }
        public string BestNightThursday { get; set; }
        public string BestNightFriday { get; set; }
        public string BestNightSaturday { get; set; }
        public string BestNightSunday { get; set; }
        public string CoatCheck { get; set; }
        public string GoodForDancing { get; set; }
        public string HappyHour { get; set; }
        public string Smoking { get; set; }
        public string AgesAllowed { get; set; }
        public string Open24Hours { get; set; }
        public string DietDairyFree { get; set; }
        public string DietGlutenFree { get; set; }
        public string DietVegan { get; set; }
        public string DietKosher { get; set; }
        public string DietHalal { get; set; }
        public string DietSoyFree { get; set; }
        public string DietVegetarian { get; set; }
        public string MusicDJ { get; set; }
        public string MusicNo { get; set; }
        public string MusicKaraoke { get; set; }
        public string MusicLive { get; set; }
        public string MusicJukebox { get; set; }
        public string MusicVideo { get; set; }
        public string MusicBackground { get; set; }
        public string Delivery { get; set; }
        public string WheelchairAccessible {get; set;}

        public string Categories { get; set; }
        public List<string> CategoryList { get; set; }

        public List<Review> Revs { get; set; }
        public int ReviewsObserved { get; set; }
        public int ReviewsHome { get; set; }
        public int ReviewsForeign { get; set; }
        public int ReviewsHomeArea { get; set; }
        public int ReviewsForeignArea { get; set; }
        public int ReviewsFunny { get; set; }
        public int ReviewsCool { get; set; }
        public int ReviewsUseful { get; set; }

        public int FirstTimeReviews { get; set; }
        public double ObservedAvg { get; set; }
        public string ZIPCode { get; set; }
        public string OrigCity { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Hood { get; set; }
        public string Address { get; set; }
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
                if (Rev.IsInHomeArea == 1)
                    ReviewsHomeArea += 1;
                else
                    ReviewsForeignArea += 1;
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

        public void CalcualteStandardDev()
        {
           // Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date))); needs to be done before
            foreach (var rev in Revs)
            {
                var squaredSum = 0.0;
                int i = 0;
                while (rev != Revs[i])
                {
                    var stars = Convert.ToDouble(Revs[i].Stars);
                    squaredSum = (stars - rev.CumulatedBusinessAvg) * (stars - rev.CumulatedBusinessAvg);
                    i++;
                }
                if (i > 0)
                    rev.LagStandardDev = squaredSum / i;
            }
        }

        public void CalcualteStratifiedLag()
        {
            // Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date))); needs to be done before
            for(int i = 0; i < Revs.Count; i++)
            {
                var sum = 0.0;
                var count = 0;
                
                for (int j=0;j<i & j < 20; j++)
                {
                    sum+= Convert.ToDouble(Revs[j].Stars);
                    count++;
                }
                if (count > 0)
                    Revs[i].Top20Avg = sum / count;
                var sqSum = 0.0;
                for (int j = 0; j < i & j < 20; j++)
                {
                    var stars = Convert.ToDouble(Revs[j].Stars);
                    sqSum += (stars- Revs[i].Top20Avg)* (stars - Revs[i].Top20Avg);
                    count++;
                }
                if (count > 0)
                    Revs[i].Top20StdDev = sqSum / count;
            }
        }

    }
}
