using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.DataManage
{
    public static class RoleDataManager
    {
        public static Dictionary<string, byte> ByteValues;
        public static Dictionary<string, bool> BoolValues;
        public static Dictionary<string, float> FloatValues;
        public static void SetValueByte(string name, byte value) => ByteValues[name] = value;
        public static void SetValueBool(string name, bool value) => BoolValues[name] = value;
        public static void SetValueFloat(string name, float value) => FloatValues[name] = value;
        public static float GetValueFloat(string name) => FloatValues[name];
        public static byte GetValueByte(string name) => ByteValues[name];
        public static bool GetValueBool(string name) => BoolValues[name];

        public static void ClearAndReloads()
        {
            ByteValues = new();
            BoolValues = new();
            FloatValues = new();
        }
    }
}
