using System;
using System.Collections.Generic;

internal class SponsorInfoComponent
{
    public class SponsorInfo
    {
        public string Uid { get; set; }
        public int DonateLevel { get; set; }
        public SponsorInfo(string uid, int donateLevel)
        {
            Uid = uid;
            DonateLevel = donateLevel;
        }
    }
    public static List<SponsorInfo> listOfSponsors = new List<SponsorInfo>();
}
