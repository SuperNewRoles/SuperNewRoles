
namespace SuperNewRoles.Map.Agartha
{
    public static class main
    {
        private static int thisfloor = 0x73;
        public static int floor
        {
            get
            {
                return thisfloor == 0x73 ? 1 : thisfloor;
            }
            set
            {
                thisfloor = value is 1 or 2 ? value : 1;
            }
        }
        public static void ClearAndReloads()
        {

        }
    }
}