using System;
using System.Collections.Generic;
using System.Drawing;

public class SponsorColor
{
    public static string GetColorForNickname(int DonateLvl)
    {
        string color = "#aa00ff";
        switch (DonateLvl)
        {
            case 1:
                color = "#af00af";
                return color;
                break;
            case 2:
                color = "#af00af";
                return color;
                break;
            case 3:
                color = "#af00af";
                return color;
                break;
            case 4:
                color = "#af00af";
                return color;
                break;
            case 5:
                color = "#af00af";
                return color;
                break;
        }
        return color;
    }
    public static string GetColorForText(int DonateLvl)
    {
        string color = "#aa00ff";
        switch (DonateLvl)
        {
            case 1:
                color = "#af00af";
                return color;
                break;
            case 2:
                color = "#af00af";
                return color;
                break;
            case 3:
                color = "#af00af";
                return color;
                break;
            case 4:
                color = "#af00af";
                return color;
                break;
            case 5:
                color = "#af00af";
                return color;
                break;
        }
        return color;
    }
}
