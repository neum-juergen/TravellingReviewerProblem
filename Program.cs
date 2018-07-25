using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        static Dictionary<String, Review> _reviews = new Dictionary<String, Review>();
        static List<AggregatedBusiness> _aggregatedBusinesses = new List<AggregatedBusiness>();
        static Dictionary<String, String> zip_city_map = new Dictionary<string, string>();
        static string[] real_distance_cues = {"the other day", "days ago", "one week ago", "last month", "last year", "last week", "weeks ago", "last weekend", "last friday", "last saturday", "last sunday", "last monday", "last tuesday", "last wednesday", "last thursday", "that morning", "that afternoon" ,"that evening", "that night", "that day"};
        static string[] real_contiguity_cues = { "yesterday", "just now", "just got back", "just came back" , "today", "this morning", "tonight", "last night", "just visited", "this afternoon", "this evening", "hours ago" };

        public static bool analyzeText(string[] list, string text)
        {
            foreach (var s in list)
                if (text.Contains(s))
                    return true;
            return false;
            
        }


        private static string transformBool(string s)
        {
            if (s.Contains("True")) return "1";
            if (s.Contains("False")) return "0";
            return s;
        }

        private static void fillCityMap()
        {
            System.IO.StreamReader file =new System.IO.StreamReader("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\zip_list.csv");
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
        private static int user_max_friends = 0;
        private static int user_max_elite = 0;
        private static int business_max_cats = 0;
        private static void _handleUser(string line)
        {
            dynamic stuff = JsonConvert.DeserializeObject(line);
            if (stuff!=null && stuff.type == "user")
            {
                _users[stuff.user_id.ToString()] = (new User
                {
                    Reviews = new List<Review>(),
                    YearsElite = new List<string>(),
                    
                    Tips = new List<Tip>(),
                    AvgStars = stuff.average_stars,
                    ReviewCount = stuff.review_count,
                    Fans = stuff.fans,
                    Id = stuff.user_id,
                    YelpingSince = stuff.yelping_since,
                    NumbYearsElite = stuff.elite.Count,
                    NumbFriends = stuff.friends.Count,
                    UsefulSent = stuff.useful,
                    FunnySent = stuff.funny,
                    CoolSent = stuff.cool,
                    ComplimentsHot = stuff.compliment_hot,
                    ComplimentsCool = stuff.compliment_cool,
                    ComplimentsCute= stuff.compliment_cute,
                    ComplimentsFunny = stuff.compliment_funny,
                    ComplimentsList = stuff.compliment_list,
                    ComplimentsMore = stuff.compliment_more,
                    ComplimentsNote = stuff.compliment_note,
                    ComplimentsPhotos = stuff.compliment_photos,
                    ComplimentsPlain = stuff.compliment_plain,
                    ComplimentsProfile = stuff.compliment_profile,
                    ComplimentsWriter = stuff.compliment_writer,
                    Name = stuff.name
                });
                if (stuff.friends[0].ToString().Contains("None")) _users[stuff.user_id.ToString()].NumbFriends = 0;
                user_max_elite = Math.Max(user_max_elite, stuff.elite.Count);
                //user_max_friends = Math.Max(user_max_friends, stuff.friends.Count);
                foreach (string s in stuff.elite)
                    _users[stuff.user_id.ToString()].YearsElite.Add(s);
               // foreach (string s in stuff.friends)
                 //   _users[stuff.user_id.ToString()].Friends.Add(s);
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
                        if(stuff.categories!=null)
                            foreach (var cat in stuff.categories)
                                realCat = cat;
                        var zip="";
                        //if (stuff.address.ToString().Length > 4)
                            zip = stuff.postal_code;
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
                            CityArea = getCityArea(realCity.ToString()),
                            Open = stuff.is_open,
                            Categories = realCat,
                            ZIPCode = zip,
                            OrigCity = stuff.city,
                            State = stuff.state,
                            Address = stuff.address,
                            Hood = stuff.neighborhood,
                            CategoryList = new List<string>(),
                            Name = stuff.name
                        });
                        if (stuff.attributes != null)
                        {
                            var biz = _businesses[stuff.business_id.ToString()];
                            var atts = stuff.attributes;
                            foreach (var att in atts)
                            {
                                var val = transformBool(att.Value.Split(' ')[1]);
                                if (att.Value.Contains("BikeParking"))
                                    biz.BikeParking = val;
                                else if (att.Value.Contains("BusinessAcceptsBitcoin"))
                                    biz.AcceptsBitcoin = val;
                                else if (att.Value.Contains("BusinessAcceptsCreditCards"))
                                    biz.AcceptsCreditCards = val;
                                else if (att.Value.Contains("DogsAllowed"))
                                    biz.DogsAllowed = val;
                                else if (att.Value.Contains("RestaurantsPriceRange2"))
                                    biz.PriceRange = val;
                                else if (att.Value.Contains("WheelchairAccessible"))
                                    biz.WheelchairAccessible = val;
                                else if (att.Value.Contains("BusinessParking"))
                                {
                                    var parking = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var parkSpot in parking)
                                    {
                                        var parkVal = transformBool(parkSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]); 
                                        if (parkSpot.Contains("garage"))
                                            biz.ParkingGarage = parkVal;
                                        else if (parkSpot.Contains("street"))
                                            biz.ParkingStreet = parkVal;
                                        else if (parkSpot.Contains("validated"))
                                            biz.ParkingValidated = parkVal;
                                        else if (parkSpot.Contains("lot"))
                                            biz.ParkingLot = parkVal;
                                        else if (parkSpot.Contains("valet"))
                                            biz.ParkingValet = parkVal;
                                    }
                                }
                                else if (att.Value.Contains("GoodForMeal"))
                                {
                                    var meal = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var mealSpot in meal)
                                    {
                                        var mealVal = transformBool(mealSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (mealSpot.Contains("dessert"))
                                            biz.GoodForDessert = mealVal;
                                        else if (mealSpot.Contains("latenight"))
                                            biz.GoodForLatenight = mealVal;
                                        else if (mealSpot.Contains("lunch"))
                                            biz.GoodForLunch = mealVal;
                                        else if (mealSpot.Contains("dinner"))
                                            biz.GoodForDinner = mealVal;
                                        else if (mealSpot.Contains("breakfast"))
                                            biz.GoodForBreakfast = mealVal;
                                        else if (mealSpot.Contains("brunch"))
                                            biz.GoodForBrunch = mealVal;

                                    }

                                }
                                else if (att.Value.Contains("Ambience"))
                                {
                                    var ambience = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var ambienceSpot in ambience)
                                    {
                                        var ambienceVal = transformBool(ambienceSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (ambienceSpot.Contains("romantic"))
                                            biz.AmbienceRomantic = ambienceVal;
                                        else if (ambienceSpot.Contains("intimate"))
                                            biz.AmbienceIntimate = ambienceVal;
                                        else if (ambienceSpot.Contains("classy"))
                                            biz.AmbienceClassy = ambienceVal;
                                        else if (ambienceSpot.Contains("hipster"))
                                            biz.AmbienceHipster = ambienceVal;
                                        else if (ambienceSpot.Contains("divey"))
                                            biz.AmbienceDivey = ambienceVal;
                                        else if (ambienceSpot.Contains("touristy"))
                                            biz.AmbienceTouristy = ambienceVal;
                                        else if (ambienceSpot.Contains("trendy"))
                                            biz.AmbienceTrendy = ambienceVal;
                                        else if (ambienceSpot.Contains("upscale"))
                                            biz.AmbienceUpscale = ambienceVal;
                                        else if (ambienceSpot.Contains("casual"))
                                            biz.AmbienceCasual = ambienceVal;

                                    }
                             
                                }
                                 else if (att.Value.Contains("HairSpecializesIn"))
                                {
                                    var hair = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var HairSpot in hair)
                                    {
                                        var hairVal = transformBool(HairSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (HairSpot.Contains("coloring"))
                                            biz.HairSpecialColor = hairVal;
                                        else if (HairSpot.Contains("africanamerican"))
                                            biz.HairSpecialAfroAmerican = hairVal;
                                        else if (HairSpot.Contains("curly"))
                                            biz.HairSpecialCurly = hairVal;
                                        else if (HairSpot.Contains("perms"))
                                            biz.HairSpecialPerms = hairVal;
                                        else if (HairSpot.Contains("kids"))
                                            biz.HairSpecialKids = hairVal;
                                        else if (HairSpot.Contains("extensions"))
                                            biz.HairSpecialExtensions = hairVal;
                                        else if (HairSpot.Contains("asian"))
                                            biz.HairSpecialAsian = hairVal;
                                        else if (HairSpot.Contains("straightperms"))
                                            biz.HairSpecialStraightPerms = hairVal;

                                    }

                                }
                                else if (att.Value.Contains("BestNights"))
                                {
                                    var bestnights = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var NightSpot in bestnights)
                                    {
                                        var nightVal = transformBool(NightSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (NightSpot.Contains("monday"))
                                            biz.BestNightMonday = nightVal;
                                        else if (NightSpot.Contains("tuesday"))
                                            biz.BestNightTuesday = nightVal;
                                        else if (NightSpot.Contains("wednesday"))
                                            biz.BestNightWednesday = nightVal;
                                        else if (NightSpot.Contains("thursday"))
                                            biz.BestNightThursday = nightVal;
                                        else if (NightSpot.Contains("friday"))
                                            biz.BestNightFriday = nightVal;
                                        else if (NightSpot.Contains("saturday"))
                                            biz.BestNightSaturday = nightVal;
                                        else if (NightSpot.Contains("sunday"))
                                            biz.BestNightSunday = nightVal;

                                    }

                                }
                                else if (att.Value.Contains("Music"))
                                {
                                    var music = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var MusicSpot in music)
                                    {
                                        var musicVal = transformBool(MusicSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (MusicSpot.Contains("dj"))
                                            biz.MusicDJ = musicVal;
                                        else if (MusicSpot.Contains("no_music"))
                                            biz.MusicNo = musicVal;
                                        else if (MusicSpot.Contains("karaoke"))
                                            biz.MusicKaraoke = musicVal;
                                        else if (MusicSpot.Contains("live"))
                                            biz.MusicLive = musicVal;
                                        else if (MusicSpot.Contains("jukebox"))
                                            biz.MusicJukebox = musicVal;
                                        else if (MusicSpot.Contains("video"))
                                            biz.MusicVideo = musicVal;
                                        else if (MusicSpot.Contains("background_music"))
                                            biz.MusicBackground = musicVal;


                                    }

                                }
                                else if (att.Value.Contains("DietaryRestrictions"))
                                {
                                    var diet = att.Value.Split('{')[1].Split('}')[0].Split(',');
                                    foreach (var dietSpot in diet)
                                    {
                                        var dietVal = transformBool(dietSpot.Split(new string[] { ": " }, StringSplitOptions.None)[1]);
                                        if (dietSpot.Contains("dairy-free"))
                                            biz.DietDairyFree = dietVal;
                                        else if (dietSpot.Contains("gluten-free"))
                                            biz.DietGlutenFree = dietVal;
                                        else if (dietSpot.Contains("vegan"))
                                            biz.DietVegan = dietVal;
                                        else if (dietSpot.Contains("kosher"))
                                            biz.DietKosher = dietVal;
                                        else if (dietSpot.Contains("halal"))
                                            biz.DietHalal = dietVal;
                                        else if (dietSpot.Contains("soy-free"))
                                            biz.DietSoyFree = dietVal;
                                        else if (dietSpot.Contains("vegetarian"))
                                            biz.DietVegetarian = dietVal;


                                    }

                                }
                                else if (att.Value.Contains("GoodForKids"))
                                    biz.GoodForKids = val;
                                else if (att.Value.Contains("RestaurantsAttire"))
                                    biz.Attire = val;
                                else if (att.Value.Contains("RestaurantsDelivery"))
                                    biz.Delivery = val;
                                else if (att.Value.Contains("RestaurantsGoodForGroups"))
                                    biz.GoodForGroups = val;
                                else if (att.Value.Contains("RestaurantsTakeOut"))
                                    biz.TakeOut = val;
                                else if (att.Value.Contains("RestaurantsReservations"))
                                    biz.Reservations = val;
                                else if (att.Value.Contains("RestaurantsTableService"))
                                    biz.TableService = val;
                                else if (att.Value.Contains("RestaurantsCounterService"))
                                    biz.CounterService = val;
                                else if (att.Value.Contains("WiFi"))
                                    biz.WiFi = val;
                                else if (att.Value.Contains("Alcohol"))
                                    biz.Alcohol = val;
                                else if (att.Value.Contains("Caters"))
                                    biz.Catering = val;
                                else if (att.Value.Contains("HasTV"))
                                    biz.HasTV = val;
                                else if (att.Value.Contains("NoiseLevel"))
                                    biz.NoiseLevel = val;
                                else if (att.Value.Contains("OutdoorSeating"))
                                    biz.OutdoorSeating = val;
                                else if (att.Value.Contains("ByAppointmentOnly"))
                                    biz.ByAppointmentOnly = val;
                                else if (att.Value.Contains("AcceptsInsurance"))
                                    biz.AcceptsInsurance = val;
                                else if (att.Value.Contains("BYOBCorkage"))
                                    biz.BYOBCorkage = val;
                                else if (att.Value.Contains("BYOB"))
                                    biz.BYOB = val;
                                else if (att.Value.Contains("Corkage"))
                                    biz.Corkage = val;
                                else if (att.Value.Contains("DriveThru"))
                                    biz.DriveThru = val;
                                else if (att.Value.Contains("CoatCheck"))
                                    biz.CoatCheck = val;
                                else if (att.Value.Contains("GoodForDancing"))
                                    biz.GoodForDancing = val;
                                else if (att.Value.Contains("HappyHour"))
                                    biz.HappyHour = val;
                                else if (att.Value.Contains("Smoking"))
                                    biz.Smoking = val;
                                else if (att.Value.Contains("AgesAllowed"))
                                    biz.AgesAllowed = val;
                                else if (att.Value.Contains("Open24Hours"))
                                    biz.Open24Hours = val;

                            }
                        }
                        if (stuff.categories != null)
                        {
                            business_max_cats = Math.Max(business_max_cats, stuff.categories.Count);

                            //user_max_friends = Math.Max(user_max_friends, stuff.friends.Count);
                            foreach (string s in stuff.categories)
                            {
                                _businesses[stuff.business_id.ToString()].CategoryList.Add(s);
                            }
                        }
                        if (stuff.hours != null)
                        {
                            
                            //user_max_friends = Math.Max(user_max_friends, stuff.friends.Count);
                            foreach (string s in stuff.hours)
                            {
                                var hour = s.Split(' ')[1];
                                if (s.Contains("Monday"))
                                    _businesses[stuff.business_id.ToString()].HoursMonday = hour;
                                if (s.Contains("Tuesday"))
                                    _businesses[stuff.business_id.ToString()].HoursTuesday = hour;
                                if (s.Contains("Wednesday"))
                                    _businesses[stuff.business_id.ToString()].HoursWednesday = hour;
                                if (s.Contains("Thursday"))
                                    _businesses[stuff.business_id.ToString()].HoursThursday = hour;
                                if (s.Contains("Friday"))
                                    _businesses[stuff.business_id.ToString()].HoursFriday = hour;
                                if (s.Contains("Saturday"))
                                    _businesses[stuff.business_id.ToString()].HoursSaturday = hour;
                                if (s.Contains("Sunday"))
                                    _businesses[stuff.business_id.ToString()].HoursSunday = hour;
                            }
                        }
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
                    var rev = new Review { Length = stuff.text.ToString().Length, Cool = Convert.ToInt32(stuff.cool), Funny = Convert.ToInt32(stuff.funny), Useful = Convert.ToInt32(stuff.useful), Date = stuff.date, Stars = stuff.stars, Biz = _businesses[stuff.business_id.ToString()], Id = stuff.review_id.ToString(), User = _users[stuff.user_id.ToString()] };
                    _users[stuff.user_id.ToString()].Reviews.Add(rev);
                    if (rev.Biz != null)
                    {
                        if (rev.Biz.Revs == null) rev.Biz.Revs = new List<Review>();
                        rev.Biz.Revs.Add(rev);
                    }
                    var text = stuff.text.ToString().ToLower();
                    //var cont_today = text.Contains("today");
                    //var cont_this_morning = text.Contains("this morning");
                    var tempCont = text.Contains("today") | text.Contains("this morning") | text.Contains("just got back") | text.Contains("tonight");
                    var tempDist = text.Contains("monday") | text.Contains("tuesday") | text.Contains("wednesday") | text.Contains("thursday") | text.Contains("friday") | text.Contains("saturday") | text.Contains("sunday") | text.Contains("yesterday") | text.Contains("last night") | text.Contains("last week") | text.Contains("weekend") | text.Contains("weekday") | text.Contains("last month") | text.Contains("last year");
                    if (tempCont) rev.TemporalContiguity = 1;
                    if (tempDist) rev.TemporalDistance = 1;
                    var social = text.Contains("with my husband") | text.Contains("with my wife") | text.Contains("with my family") | text.Contains("with my brother") | text.Contains("with my sister") | text.Contains("with my kid") | text.Contains("with my child") | text.Contains("with my uncle") | text.Contains("with my aunt") | text.Contains("with my father") | text.Contains("with my mother") | text.Contains("with my dad") | text.Contains("with my mom") | text.Contains("with my grandfather") | text.Contains("with my grandmother") | text.Contains("with my grandpa") | text.Contains("with my grandma");
                    if (social) rev.SocialConsumption = 1;
                    if (analyzeText(real_contiguity_cues, text)) rev.RealContiguity = 1;
                    if (analyzeText(real_distance_cues, text)) rev.RealDistance = 1;
                 }
                reviewId++;
                numbRev++;
                Console.WriteLine("Reviews:" + numbRev);
            }
            /*if (stuff.type == "tip")
            {
                if (_users.ContainsKey(stuff.user_id.ToString()) && _businesses.ContainsKey(stuff.business_id.ToString()))
                {
                    var tip = new Tip { Biz = _businesses[stuff.business_id.ToString()], Date = stuff.date };
                    _users[stuff.user_id.ToString()].Tips.Add(tip);
                }
                numbTip++;
                Console.WriteLine("Tips:" + numbTip);
            }*/
            
        }



        private static void _handleRev2(string line)
        {
            dynamic stuff = JsonConvert.DeserializeObject(line);
            if (stuff.type == "review")
            {
                
                {
                    var rev = new Review { Length = stuff.text.ToString().Length, Cool = Convert.ToInt32(stuff.cool), Funny = Convert.ToInt32(stuff.funny), Useful = Convert.ToInt32(stuff.useful), Date = stuff.date, Stars = stuff.stars, Id = stuff.review_id.ToString(),  };
                    
                    if (rev.Biz != null)
                    {
                        if (rev.Biz.Revs == null) rev.Biz.Revs = new List<Review>();
                        rev.Biz.Revs.Add(rev);
                    }
                    rev.Text = stuff.text.ToString();
                    var text = stuff.text.ToString().ToLower();
                    var tempCont = text.Contains("today") | text.Contains("this morning") | text.Contains("just got back") | text.Contains("tonight");
                    var tempDist = text.Contains("monday") | text.Contains("tuesday") | text.Contains("wednesday") | text.Contains("thursday") | text.Contains("friday") | text.Contains("saturday") | text.Contains("sunday") | text.Contains("yesterday") | text.Contains("last night") | text.Contains("last week") | text.Contains("weekend") | text.Contains("weekday") | text.Contains("last month") | text.Contains("last year");
                    if (tempCont) rev.TemporalContiguity = 1;
                    if (tempDist) rev.TemporalDistance = 1;
                    _revWriter.WriteLine(rev.Id + "|" + Regex.Replace(rev.Text.Replace("\"","").Replace("|",""), @"\r\n?|\n", "") + "");
                    _revWriter.Flush();
                    
                }
                reviewId++;
                numbRev++;
                Console.WriteLine("Reviews:" + numbRev);
            }
            /*if (stuff.type == "tip")
            {
                if (_users.ContainsKey(stuff.user_id.ToString()) && _businesses.ContainsKey(stuff.business_id.ToString()))
                {
                    var tip = new Tip { Biz = _businesses[stuff.business_id.ToString()], Date = stuff.date };
                    _users[stuff.user_id.ToString()].Tips.Add(tip);
                }
                numbTip++;
                Console.WriteLine("Tips:" + numbTip);
            }*/

        }
        private static int numbTip = 0;

        private static void writeAll(StreamWriter userWriter, StreamWriter businessWriter, StreamWriter reviewWriter, StreamWriter aggregatedBusinessWriter)
        {

            var numbUsers = 0;
            foreach (var uId in _users.Keys)
            {
                Console.WriteLine("Users written:" + numbUsers++);
                var user = _users[uId];
                user.DetermineHometown();
               
                if ( user.ReviewsInHome<3 ||user.ReviewsInHome * 1.0 / Convert.ToDouble(user.ReviewCount) < 0.5 || !user.CheckDayDifference(360)) user.WriteUser = user.DeleteHome();
                if (user.ReviewsInHomeArea < 3 || user.ReviewsInHomeArea * 1.0 / Convert.ToDouble(user.ReviewCount) < 0.5 || !user.CheckDayDifferenceArea(360))  user.DeleteHomeArea();
                if(user.WriteUser)
                {
                    user.Reviews.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
                    var cumSum = 0.0;
                    var cumHomeArea = 0;
                    var cumForeignArea = 0;
                    var cumHomeAreaSum = 0.0;
                    var cumForeignAreaSum = 0.0;
                    var cumReviews = 0;

                    var user_content = user.Id+";"+user.Name+";"+user.ReviewCount+";"+user.AvgStars+";"+user.YelpingSince+";"+user.Fans+";"+user.NumbYearsElite+";"+user.NumbFriends+";"+user.UsefulSent+";"+user.FunnySent+";"+user.CoolSent+";"+user.ComplimentsHot+";"+user.ComplimentsMore+";"+user.ComplimentsProfile+";"+user.ComplimentsCute+";"+user.ComplimentsList+";"+user.ComplimentsNote+";"+user.ComplimentsPlain+";"+user.ComplimentsCool+";"+user.ComplimentsFunny+";"+user.ComplimentsWriter+";"+user.ComplimentsPhotos;
                    for (int i = 1; i <= user_max_elite; i++)
                        if (user.NumbYearsElite >= i)
                            user_content += ";" + user.YearsElite[i - 1];
                        else user_content += ";";
                    for (int i = 1; i <= user_max_friends; i++)
                        if (user.NumbFriends >= i)
                            user_content += ";" + user.Friends[i - 1];
                    userWriter.WriteLine(user_content + ";"+user.Hometown+";"+user.ReviewsInHome+";"+user.TipsInHome+";"+user.VotedOnlyInHome+";"+user.Reviews.Count+";"+user.HometownBasedOnTips+";"+user.HomeArea+";"+user.ReviewsInHomeArea);


                    userWriter.Flush();
                    Review lastRev = null;
                    foreach (var rev in user.Reviews)
                    {
                        if (cumReviews > 0)
                            rev.CumulatedUserAvg = cumSum / cumReviews;
                        else
                            rev.CumulatedUserAvg = 0.0;
                        if (cumHomeArea > 0)
                            rev.CumulatedUserHomeAvg = cumHomeAreaSum / cumHomeArea;
                        else
                            rev.CumulatedUserHomeAvg = 0.0;
                        if (cumHomeArea > 0)
                            rev.CumulatedUserForeignAvg = cumForeignAreaSum / cumForeignArea;
                        else
                            rev.CumulatedUserForeignAvg = 0.0;
                        rev.CumulatedUserReviewCount = cumReviews;
                        rev.CumulatedUserHomeCount = cumHomeArea;
                        rev.CumulatedUserForeignCount = cumForeignArea;

                        cumSum += Convert.ToDouble(rev.Stars);
                        cumHomeAreaSum = rev.IsInHomeArea * Convert.ToDouble(rev.Stars);
                        cumHomeArea += rev.IsInHomeArea;
                        cumForeignAreaSum = (1-rev.IsInHomeArea) * Convert.ToDouble(rev.Stars);
                        cumForeignArea += 1 - rev.IsInHomeArea;
                        cumReviews++;

                        if(lastRev!=null && !lastRev.Biz.City.Equals(rev.Biz.City))
                            rev.Travelled = 1;
                        lastRev = rev;
                    }
                }
            }
            _users.Clear();
            Console.WriteLine("Written users.");
            int bizWritten = 0;
            foreach (var bId in _businesses.Keys)
            {
                var biz = _businesses[bId];
                biz.DetermineAggregatedValues();
                
                biz.Revs.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)));
                var cumSum = 0.0;
                var cumHome = 0;
                var cumForeign = 0;
                var cumReviews = 0;
                var business_content = biz.Id + ";" + biz.City + ";" + biz.CityArea + ";" + biz.Latitude + ";" + biz.Longitude + ";" + biz.Stars + ";" + biz.ReviewCount + ";" + biz.Open + ";" + biz.Categories + ";" + biz.ReviewsObserved + ";" + biz.ObservedAvg + ";" + biz.ReviewsHome + ";" + biz.ReviewsForeign + ";" + biz.ReviewsHomeArea + ";" + biz.ReviewsForeignArea + ";" + biz.ZIPCode + ";" + biz.ReviewsCool + ";" + biz.ReviewsFunny + ";" + biz.ReviewsUseful + ";" + biz.ReviewsTravelled + ";" + biz.FirstTimeReviews + ";" + biz.Name + ";" + biz.State + ";" + biz.OrigCity + ";" + biz.Hood + ";" + biz.Address + ";" + biz.BikeParking + ";" + biz.AcceptsBitcoin + ";" + biz.AcceptsCreditCards + ";" + biz.ParkingGarage + ";" + biz.ParkingStreet + ";" + biz.ParkingValidated + ";" + biz.ParkingLot + ";" + biz.ParkingValet + ";" + biz.DogsAllowed + ";" + biz.PriceRange + ";" + biz.WheelchairAccessible + ";" + biz.GoodForKids + ";" + biz.GoodForDessert + ";" + biz.GoodForLatenight + ";" + biz.GoodForLunch + ";" + biz.GoodForDinner + ";" + biz.GoodForBreakfast + ";" + biz.GoodForBrunch + ";" + biz.Attire + ";" + biz.Delivery + ";" + biz.GoodForGroups + ";" + biz.TakeOut + ";" + biz.WiFi + ";" + biz.Alcohol + ";" + biz.Catering + ";" + biz.HasTV + ";" + biz.NoiseLevel + ";" + biz.OutdoorSeating + ";" + biz.Reservations + ";" + biz.TableService + ";" + biz.CounterService + ";" + biz.ByAppointmentOnly + ";" + biz.AmbienceRomantic + ";" + biz.AmbienceIntimate + ";" + biz.AmbienceClassy + ";" + biz.AmbienceHipster + ";" + biz.AmbienceDivey + ";" + biz.AmbienceTouristy + ";" + biz.AmbienceTrendy + ";" + biz.AmbienceUpscale + ";" + biz.AmbienceCasual + ";" + biz.AcceptsInsurance + ";" + biz.BYOB + ";" + biz.BYOBCorkage + ";" + biz.Corkage + ";" + biz.HoursMonday + ";" + biz.HoursTuesday + ";" + biz.HoursWednesday + ";" + biz.HoursThursday + ";" + biz.HoursFriday + ";" + biz.HoursSaturday + ";" + biz.HoursSunday + ";" + biz.HairSpecialColor + ";" + biz.HairSpecialAfroAmerican + ";" + biz.HairSpecialCurly + ";" + biz.HairSpecialPerms + ";" + biz.HairSpecialKids + ";" + biz.HairSpecialExtensions + ";" + biz.HairSpecialAsian + ";" + biz.HairSpecialStraightPerms + ";" + biz.DriveThru + ";" + biz.BestNightMonday + ";" + biz.BestNightTuesday + ";" + biz.BestNightWednesday + ";" + biz.BestNightThursday + ";" + biz.BestNightFriday + ";" + biz.BestNightSaturday + ";" + biz.BestNightSunday + ";" + biz.CoatCheck + ";" + biz.GoodForDancing + ";" + biz.HappyHour + ";" + biz.MusicDJ + ";" + biz.MusicNo + ";" + biz.MusicKaraoke + ";" + biz.MusicLive + ";" + biz.MusicJukebox + ";" + biz.MusicVideo + ";" + biz.MusicBackground + ";" + biz.Smoking + ";" + biz.AgesAllowed + ";" + biz.DietDairyFree + ";" + biz.DietGlutenFree + ";" + biz.DietVegan + ";" + biz.DietKosher + ";" + biz.DietHalal + ";" + biz.DietSoyFree + ";" + biz.DietVegetarian + ";" + biz.Open24Hours; 
                for (int i = 1; i <= business_max_cats; i++)
                    if (biz.CategoryList.Count >= i)
                        business_content += ";" + biz.CategoryList[i - 1];
                    else business_content += ";";
                if (biz.CategoryList.Count == 23)
                {
                    Console.WriteLine("ad");
                }
                businessWriter.WriteLine(business_content);
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
                }
                biz.CalcualteStandardDev();
                biz.CalcualteStratifiedLag();
                foreach (var rev in biz.Revs)
                {
                        var content2 = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};{30}", rev.Biz.Id, rev.User.Id, rev.Stars, rev.Date, rev.IsInHome, rev.CumulatedBusinessAvg, rev.CumulatedBusinessReviewCount, rev.CumulatedUserAvg, rev.CumulatedUserReviewCount, rev.Length, rev.Cool, rev.Funny, rev.Useful, rev.Travelled, rev.FirstTimeReview, rev.ReviewsUserPostedInCitySoFar, rev.TemporalContiguity, rev.IsInHomeArea, rev.TemporalDistance, rev.ReviewsUserPostedInCityAreaSoFar, rev.Id, rev.CumulatedUserHomeAvg, rev.CumulatedUserForeignAvg, rev.CumulatedUserHomeCount, rev.CumulatedUserForeignCount, rev.LagStandardDev, rev.Top20Avg, rev.Top20StdDev, rev.SocialConsumption, rev.RealContiguity, rev.RealDistance);
                    //var content2 = string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", rev.Biz.Id, rev.User.Id, rev.Stars, rev.Date, rev.Length, rev.Cool, rev.Funny, rev.Useful);
                    reviewWriter.WriteLine(content2);
                    reviewWriter.Flush();
                    writtenReviews++;

                }
                    foreach (var aggBiz in AggregatedBusiness.TransformBusiness(biz))
                    {
                        var content3 = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17}", aggBiz.Id, aggBiz.City, aggBiz.Latitude, aggBiz.Longitude, aggBiz.TotalReviewsObserved, aggBiz.ReviewCount, aggBiz.Open, aggBiz.Categories, aggBiz.ReviewsObserved, aggBiz.ObservedAvg, aggBiz.ReviewsHome, aggBiz.ReviewsForeign, aggBiz.ZIPCode, aggBiz.Date, aggBiz.ReviewsInMonth, aggBiz.AvgInMonth, aggBiz.HomeInMonth, aggBiz.ForeignInMonth);
                        aggregatedBusinessWriter.WriteLine(content3);
                        aggregatedBusinessWriter.Flush();
                    }
                bizWritten++;
                Console.WriteLine(bizWritten);
            }
            Console.WriteLine("Written businesses and reviews.");
        }
        static void Main(string[] args)
        {


            basic_main();
        }
        private static StreamWriter _revWriter;

        static void writeRevsWithText()
        {
            _revWriter = new StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\review_text.csv");
            _revWriter.WriteLine("Id|Text|");
            //reviewWriter.WriteLine("Business;User;Review_Stars;Date;Length;Votes_Cool;Votes_Funny;Votes_Useful");
            _revWriter.Flush();
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_review.json", _handleRev2,1000);


            _revWriter.Close();
        }


        static void WriteUserLocs()
        {
            fillCityMap();
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_business.json", _handleUser);
            var writer = new System.IO.StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\user_locs_revision.csv");
            writer.WriteLine("User;RealLocation;RealHomeArea");
            var dic = new Dictionary<string, string>();
            foreach (var biz in _businesses.Values)
                if (!dic.ContainsKey(biz.City.ToLower()))
                    dic.Add(biz.City.ToLower(), biz.CityArea);
            int c = 0;
            System.IO.StreamReader file = new System.IO.StreamReader("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\UserLocsRevision.csv");
            var line = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                try
                {


                    var s = line.Split(';');
                    var loc = s[0];
                    var id = "";
                    if (s[s.Length - 1].Contains("userid"))
                        id = s[s.Length - 1].Split(new string[] { "userid=" }, StringSplitOptions.None)[1];
                    var area = "Unknown";
                    if (dic.ContainsKey(loc.ToLower()))
                        area = dic[loc.ToLower()];
                    if (id != "")
                        writer.WriteLine(id + ";" + loc + ";" + area);

                }
                catch (JsonReaderException) { }
                catch (RuntimeBinderException) { }
                c++;
                Console.WriteLine(c);

            }
            file.Close();

            writer.Close();
        }


        static void WriteCityMapping()
        {
            fillCityMap();
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_business.json", _handleUser);
            var writer = new System.IO.StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\city_mapping.csv");
            writer.WriteLine("City;Mapped_City");
            var dic = new Dictionary<string, string>();
            foreach (var biz in _businesses.Values)
                if(!dic.ContainsKey(biz.City))
                    dic.Add(biz.City, biz.CityArea);
            foreach (var key in dic.Keys)
                writer.WriteLine(key + ";" + dic[key]);
            writer.Close();
        }


        static void readLIWC()
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            var writer = new System.IO.StreamWriter(@"C:\Users\Jürgen Neumann\Desktop\reviews\LIWC2015_Results.csv");
            System.IO.StreamReader file = 
               new System.IO.StreamReader(@"C:\Users\Jürgen Neumann\Desktop\reviews\LIWC2015 Results (review_text.csv).csv");
            //file.ReadLine();
            line = file.ReadLine();
            var splitter = line.Split('|');
            var result = "Review_Id|";
            for (int i = 0; i < splitter.Length; i++)
                if (i > 2)
                    result += splitter[i] + "|";
            writer.WriteLine(result);
            
            while ((line = file.ReadLine()) != null)
            {
                splitter = line.Split('|');
                result = "";
                for (int i = 0; i < splitter.Length; i++)
                {
                    if (i != 1)
                        result += splitter[i] + "|";
                }
                //235
                counter++;
                Console.WriteLine(counter);
                //for (int i = 1; i < 17; i++)
                writer.WriteLine(result);
                writer.Flush();
            }
            writer.Close();
            file.Close();

            // Suspend the screen.
            Console.ReadLine();

        }


        public static void readFile(string fileName, Action<string> handleLine)
        {
            string line;
            System.IO.StreamReader file =
              new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                //try
                //{
                    
                    
                    handleLine(line);

                //}
                //catch (JsonReaderException) { Console.WriteLine(line); }
                //catch (RuntimeBinderException e) { Console.WriteLine(e.Message); }

            }
            file.Close();
        }

        public static void readFile(string fileName, Action<string> handleLine, int c)
        {
            string line;
            System.IO.StreamReader file =
              new System.IO.StreamReader(fileName);
            int count = 0;
            while ((line = file.ReadLine()) != null && count<c)
            {
                //try
                //{


                handleLine(line);
                count++;
                //}
                //catch (JsonReaderException) { Console.WriteLine(line); }
                //catch (RuntimeBinderException e) { Console.WriteLine(e.Message); }

            }
            file.Close();
        }

        public static void readFile(string fileName, Action<string> handleLine, int a, int c)
        {
            string line;
            System.IO.StreamReader file =
              new System.IO.StreamReader(fileName);
            int count = 0;
            while ((line = file.ReadLine()) != null && count < c)
            {
                //try
                //{

                if(count>a)
                    handleLine(line);
                count++;
                //}
                //catch (JsonReaderException) { Console.WriteLine(line); }
                //catch (RuntimeBinderException e) { Console.WriteLine(e.Message); }

            }
            file.Close();
        }

        private static string getCityArea(string city)
        {
            if (city.Contains(", AZ")) return "Phoenix";
            if (city.Contains(", NV")) return "Las Vegas";
            if (city.Contains(", PA")) return "Pittsburgh";
            if (city.Contains(", WI")) return "Madison";
            if (city.Contains(", IL")) return "Urbana-Champaign";
            return "Unknown";
        }


        public static void basic_main()
        {
            fillCityMap();
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_business.json", _handleUser);
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_user.json", _handleUser);
            Console.WriteLine("Users and businesses initialized");
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_tip.json", _handleRev);
            readFile("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\yelp_academic_dataset_review.json", _handleRev);
            Console.WriteLine("Reviews and tips initiated");

            // Read the file and display it line by line.

            var reviewWriter = new StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\reviews.csv");
            var businessWriter = new StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\businesses.csv");
            var userWriter = new StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\users.csv");
            var aggregatedBusinessWriter = new StreamWriter("C:\\Users\\Jürgen Neumann\\Dropbox\\Master\\Masterarbeit\\Round_9\\aggregated_businesses.csv");
            reviewWriter.WriteLine("Business;User;Review_Stars;Date;In_Hometown;Cumulated_Business_Avg;Cumulated_Business_Review_Count;Cumulated_User_Avg;Cumulated_User_Review_Count;Length;Votes_Cool;Votes_Funny;Votes_Useful;Travelled;First_Time_Review;Reviews_In_City_So_Far;Temp_Contiguity;In_Home_Area;Temporal_Distance;Reviews_In_City_Area_So_Far;Review_Id;Cumulated_User_Home_Avg;Cumulated_User_Foreign_Avg;Cumulated_User_Home_Count;Cumulated_User_Foreign_Count;Lag_Standard_Dev;Top_20_Avg;Top_20_StdDev;With_My_Relative;Real_Contiguity;Real_Distance");
            //reviewWriter.WriteLine("Business;User;Review_Stars;Date;Length;Votes_Cool;Votes_Funny;Votes_Useful");
            reviewWriter.Flush();
            var business_title = "Business;City;City_Area;Latitude;Longitude;Business_Stars;Business_Review_Count;Open;Category;Reviews_Observed;Observed_Avg;Reviews_Home;Reviews_Foreign;Reviews_Home_Area;Reviews_Foreign_Area;ZIP_Code;Business_Cool_Votes;Business_Funny_Votes;Business_Useful_Votes;Reviews_Travelled;Reviews_First_Time;Name;State;OrigCity;Hood;Address;Bike_Parking;Accepts_Bitcoin;Accepts_Credit_Cards;Parking_Garage;Parking_Street;Parking_Validated;Parking_Lot;Parking_Valet;Dogs_Allowed;Price_Range;Wheelchair_Accessible;Good_For_Kids;Good_For_Dessert;Good_For_Latenight;Good_For_Lunch;Good_For_Dinner;Good_For_Breakfast;Good_For_Brunch;Attire;Delivery;Good_For_Groups;Take_Out;WiFi;Alcohol;Catering;Has_TV;Noise_Level;Outdoor_Seating;Reservations;Table_Service;Counter_Service;By_Appointment_Only;Ambience_Romantic;Ambience_Intimate;Ambience_Classy;Ambience_Hipster;Ambience_Divey;Ambience_Touristy;Ambience_Trendy;Ambience_Upscale;Ambience_Casual;Accepts_Insurance;BYOB;BYOB_Corkage;Corkage;Hours_Monday;Hours_Tuesday;Hours_Wednesday;Hours_Thursday;Hours_Friday;Hours_Saturday;Hours_Sunday;Hair_Special_Color;Hair_Special_Afro_American;Hair_Special_Curly;Hair_Special_Perms;Hair_Special_Kids;Hair_Special_Extensions;Hair_Special_Asian;Hair_Special_Straight_Perms;Drive_Thru;Best_Night_Monday;Best_Night_Tuesday;Best_Night_Wednesday;Best_Night_Thursday;Best_Night_Friday;Best_Night_Saturday;Best_Night_Sunday;Coat_Check;Good_For_Dancing;Happy_Hour;Music_DJ;Music_No;Music_Karaoke;Music_Live;Music_Jukebox;Music_Video;Music_Background;Smoking;Ages_Allowed;Diet_Dairy_Free;Diet_Gluten_Free;Diet_Vegan;Diet_Kosher;Diet_Halal;Diet_Soy_Free;Diet_Vegetarian;Open_24_Hours";
            for (int i = 1; i <= business_max_cats; i++)
                business_title += ";category_" + i;
            businessWriter.WriteLine(business_title);
            businessWriter.Flush();
            aggregatedBusinessWriter.WriteLine("Business;City;Latitude;Longitude;Total_Reviews_Observed;Business_Review_Count;Open;Category;Reviews_Observed;Observed_Avg;Reviews_Home;Reviews_Foreign;ZIP_Code;Date;Reviews_In_Month;Avg_In_Month;Home_In_Month;Foreign_In_Month");
            aggregatedBusinessWriter.Flush();
            //;Hometown;Reviews_In_Hometown;Tips_In_Hometown;Voted_Only_Home;Reviews_Observed;Hometown_Based_On_Tips"
            var user_title = "User;Name;User_Review_Count;User_Avg_Stars;Yelping_Since;Fans;Numb_Years_Elite;Numb_Friends;Useful_Sent;Funny_Sent;Cool_Sent;Compliments_Hot;Compliments_More;Compliments_Profile;Compliments_Cute;Compliments_List;Compliments_Note;Compliments_Plain;Compliments_Cool;Compliments_Funny;Compliments_Writer;Compliments_Photos";
            for (int i = 1; i <= user_max_elite; i++)
                user_title += ";elite_" + i;
            for (int i = 1; i <= user_max_friends; i++)
                user_title += ";friends_" + i;
            userWriter.WriteLine(user_title + ";Hometown;Reviews_In_Hometown;Tips_In_Hometown;Voted_Only_Home;Reviews_Observed;Hometown_Based_On_Tips;Home_Area;Reviews_In_Home_Area");
            userWriter.Flush();

            writeAll(userWriter, businessWriter, reviewWriter, aggregatedBusinessWriter);


            reviewWriter.Close();
            businessWriter.Close();
            userWriter.Close();
            aggregatedBusinessWriter.Close();
            Console.WriteLine("Written reviews:" + writtenReviews);
            Console.ReadLine();
        }
        
    }
}
