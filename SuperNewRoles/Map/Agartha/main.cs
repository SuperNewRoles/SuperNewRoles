using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Map.Agartha
{
    public static class main
    {
        private static int thisfloor;
        public static int floor
        {
            get
            {
                if (thisfloor == null)
                {
                    return 1;
                }
                else
                {
                    return thisfloor;
                }
            }
            set
            {
                if (value is 1 or 2)
                {
                    thisfloor = value;
                }
                else
                {
                    thisfloor = 1;
                }
            }
        }
        public static void ClearAndReloads()
        {

        }
    }
}
