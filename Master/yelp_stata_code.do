clear all
set more off
cd "D:\Master\Masterarbeit\"

import delimited using "reviews.csv", delimiters(";") clear	
save "reviews.dta", replace

import delimited using "businesses.csv", delimiters(";") clear	
rename reviews_observed business_reviews_observed
save "businesses.dta", replace

import delimited using "users.csv", delimiters(";") clear	
rename reviews_observed user_reviews_observed
save "users.dta", replace

import delimited using "./TravelAccommodationData/tourism_data_2014.csv", delimiters(";") clear
drop if meaning!="All establishments"
gen zip_code = string(zip)
keep zip_code numberofestablishments
save "tourism.dta", replace

import delimited using "./TravelAccommodationData/tourism_data_2012.csv", delimiters(";") clear
drop if meaning!="All establishments"	
keep key numberofestablishments
save "tourism_2012.dta", replace

import delimited using "./TravelAccommodationData/tourism_data_2013.csv", delimiters(";") clear
drop if meaning!="All establishments"	
keep key numberofestablishments
save "tourism_2013.dta", replace

import delimited using "./TravelAccommodationData/tourism_data_2014.csv", delimiters(";") clear
drop if meaning!="All establishments"	
keep key numberofestablishments
save "tourism_2014.dta", replace
append using "tourism_2013.dta"
append using "tourism_2012.dta"
save "tourism_three_years.dta", replace

import delimited using "Controls.csv", delimiters(";") clear
gen med_rent = real(subinstr(medianrent,",","",.))
gen med_income = real(subinstr(medianincome,",","",.))
gen med_hval = real(subinstr(medianhval,",","",.))
gen pop_change = real(subinstr(realchange,"%","",.))
gen pop = real(subinstr(realpop,",","",.))
gen hisp_rate = real(subinstr(hisprate,"%","",.))
gen asian_rate = real(subinstr(asianrate,"%","",.))
gen black_rate = real(subinstr(blackrate,"%","",.))
gen amer_ind_rate = real(subinstr(amerindrate,"%","",.))
replace hisp_rate = 0 if hisp_rate==.
replace asian_rate = 0 if asian_rate==.
replace black_rate = 0 if black_rate==.
replace amer_ind_rate = 0 if amer_ind_rate==.
keep city med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea
save "controls.dta", replace

import delimited using "WaybackCrawl.CSV", delimiters(";") clear
rename medrent med_rent
rename medincome med_income
rename medhval med_hval
rename unemployment unemprate
rename hisprate hisp_rate
rename asianrate asian_rate
rename blackrate black_rate
rename amerindrate amer_ind_rate
rename medage med_age
rename pop04 pop
rename popchange pop_change
keep city med_* pop* *_rate unemprate costofliving year
gen key=city+string(year)
save "panel_controls.dta", replace


//Review level
use "reviews.dta", clear
merge m:1 user using "users.dta", force
drop if _merge!=3
drop _merge
merge m:1 business using "businesses.dta", force
drop if _merge!=3
drop _merge
drop if category=="Hotels" || category=="Hostels" || category=="Hotels & Travel" || category=="Bed & Breakfast"
merge m:1 zip_code using "tourism.dta"
drop if _merge==2
replace numberofestablishments=0 if _merge==1
drop _merge
merge m:1 city using "controls.dta"
drop if _merge!=3
gen cum_user_avg = real(subinstr(cumulated_user_avg,",",".",.))
gen cum_business_avg = real(subinstr(cumulated_business_avg,",",".",.))
gen corrected_user_avg_stars = (user_avg_stars*user_review_count-review_stars)*(user_review_count-1)
gen obs_business_avg = real(subinstr(observed_avg,",",".",.))
gen corrected_business_avg = (obs_business_avg*business_reviews_observed-review_stars)*(business_reviews_observed-1)
gen open_bin = open=="True"
//IV
reg review_stars in_hometown open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea, robust
reg votes_useful in_hometown open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea, robust
ivregress 2sls review_stars open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea (in_hometown=numberofestablishments) , first robust
ivregress 2sls votes_useful open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea (in_hometown=numberofestablishments) , first robust

reg review_stars reviews_in_city_so_far open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea, robust
reg votes_useful reviews_in_city_so_far open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans reviews_in_hometown med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea, robust
ivregress 2sls review_stars open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea (reviews_in_city_so_far=numberofestablishments) , first robust
ivregress 2sls votes_useful open_bin corrected_user_avg_stars corrected_business_avg cum_user_avg cum_business_avg cumulated_user_review_count cumulated_business_review_count years_elite fans med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea (reviews_in_city_so_far=numberofestablishments) , first robust


//Business level
use "businesses.dta", clear
drop if business_reviews_observed==0
drop if !strpos(city, ",")
gen obs_avg = real(subinstr(observed_avg,",",".",.))
merge m:1 zip_code using "tourism.dta"
drop if _merge==2
replace numberofestablishments=0 if _merge==1
drop _merge
merge m:1 city using "controls.dta"
drop if _merge!=3
drop _merge
drop if category=="Hotels" || category=="Hostels" || category=="Hotels & Travel" || category=="Bed & Breakfast"

gen home_share = reviews_home/(reviews_home+reviews_foreign)*100
gen open_bin = open=="True"
gen not_recommended_reviews = business_review_count-business_reviews_observed
set matsize 11000

reg obs_avg home_share med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea not_recommended_reviews business_reviews_observed open_bin, first robust
ivregress 2sls obs_avg med_rent med_income med_hval pop_change pop *_rate medianage unemprate costofliving landarea not_recommended_reviews business_reviews_observed open_bin (home_share=numberofestablishments), first robust

import delimited using "aggregated_businesses.csv", delimiters(";") clear	
drop if category=="Hotels" || category=="Hostels" || category=="Hotels & Travel" || category=="Bed & Breakfast"
rename reviews_observed business_reviews_observed
replace date = subinstr(date,"-","/",.)
gen date2 = date(date,"YMD")
format date2 %ty
sort date2
egen date_id = group(date)
sort business date_id
egen id = group(business)
xtset id date_id

gen home_share = reviews_home/(reviews_home+reviews_foreign)*100
gen foreign_share = reviews_foreign/(reviews_home+reviews_foreign)*100
gen obs_avg = real(subinstr(observed_avg,",",".",.))
xtreg obs_avg foreign_share business_reviews_observed, fe robust
gen key=city+string(year(date(date,"YMD")))

merge m:1 key using "panel_controls.dta"
drop if _merge!=3
drop _merge
xtreg obs_avg home_share business_reviews_observed med_* unemprate costofliving *_rate pop pop_change, fe robust

replace key=zip_code+"_"+string(year(date(date,"YMD")))

merge m:1 key using "tourism_three_years.dta"
drop if _merge==2
replace numberofestablishments=0 if _merge==1 & year<2015
drop _merge


xtivreg obs_avg (home_share=numberofestablishments) business_reviews_observed med_* unemprate costofliving *_rate pop pop_change, fe



