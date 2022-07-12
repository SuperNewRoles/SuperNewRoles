
namespace SuperNewRoles.Map.Agartha
{
    public static class main
    {
        private static int thisfloor = 0x73;
        public static int floor
        {
            get
            {
                if (thisfloor == 0x73)
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
