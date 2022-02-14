using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.CustomCosmetics.ShareCosmetics
{
    class TextChange
    {
        public static List<string> textdate = new List<string>()
        { "a","b","c","d","e","f","g","h","i","j",
          "k","l","m","n","o","p","q","r","s","t",
          "u","v","w","x","y","z","A","B","C","D",
          "E","F","G","H","I","J","K","L","M","N",
          "O","P","Q","R","S","T","U","V","W","X",
          "Y","Z","/",".","?","%","0","1","2","3",
          "4","5","6","7","8","9",":","-","=","&",
          "#","_"," "
        };
        public static string gettext(char text) {
            for (int i = 0; i< textdate.Count; i++) {
                if (textdate[i][0] == text) {
                    if (i.ToString().Length == 1)
                    {
                        return "0" + i.ToString();
                    }
                    else {
                        return i.ToString();
                    }
                }
            }
            return "0";
        }
        public static string change(string texts) {
            string bytetxt = "";
            bool a = true;
            foreach (char txt in texts) {
                if (a)
                {
                    bytetxt = gettext(txt);
                    a = false;
                }
                else
                {
                    bytetxt += gettext(txt);
                }
            }
            return bytetxt;
        }
    }
}
