using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAP.Helper
{
    public static class BooleanHelper
    {
        public static bool ConvertToBool(string value)
        {
            bool res = false;
            switch (value.ToLower())
            {
                case "0":
                    res = false;
                    break;
                case "1":
                    res = true;
                    break;
                case "true":
                    res = true;
                    break;
                case "false":
                    res = false;
                    break;
                default:
                    throw new Exception("Incorrect boolean format : only 0/1/true/false");
            }
            return res;
        }

        public static string ConvertToString(bool value)
        {
            string res = string.Empty;
            switch (value)
            {
                case true:
                    res = "1";
                    break;
                case false:
                    res = "0";
                    break;
            }
            return res;
        }
    }
}
