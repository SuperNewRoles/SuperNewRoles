using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.CustomCosmetics.ShareCosmetics
{
    class HttpConnect
    {
        public static async Task<string> Download(string url)
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles CustomCosmetics");
            var response = await http.GetAsync(new System.Uri(url), HttpCompletionOption.ResponseContentRead);
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return "";
            }
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<bool> ShareCosmeticDateDownload(byte id, string url)
        {
            var dldata = await Download(url);

            Logger.Info("DLDATA:" + dldata);
            SharePatch.PlayerDatas[id] = dldata;
            Logger.Info("c");
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(dldata));
            Logger.Info("e");
            var serializer = new DataContractJsonSerializer(typeof(CosmeticsObject));
            Logger.Info("f");
            var data = serializer.ReadObject(ms);
            Logger.Info("g");
            SharePatch.PlayerObjects[id] = (CosmeticsObject)data;
            Logger.Info("h");
            return false;
        }
    }
}