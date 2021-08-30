using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nop.Services.Google
{
    struct InfoShopsErrors
    {
        public string StoreId;
        public string StoreName;
        public string StoreUrl;
        public string StorePhoneNumber;



        public string Error;

        public static List<object> Headers() => new List<object> { "StoreId", "StoreName", "StoreUrl", "StorePhoneNumber", "Error"};
        public List<object> ToStringList() => new List<object> { StoreId, StoreName, StoreUrl, StorePhoneNumber, Error};
        public bool HasErrors()
        {
            if (StorePhoneNumber==null)
            {
                Error = "Null Number";
                return true;
            }
            else
            {
                if (StorePhoneNumber == "Sin numero")
                {
                    Error = "Sin Numero";
                    return true;
                }
                else
                { 
                    if (!Regex.IsMatch(StorePhoneNumber, "^[0-9]+$"))
                    {
                        Error = "Caracteres especiales";
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

    }
}
