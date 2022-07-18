/*using DiscordRPC;
using DiscordRPC.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SuperNewRoles.DiscordStatus
{
    public static class Patch
    {
        public static DiscordRpcClient client;
		[HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
		class DiscordStartPatch
		{
			public static void Postfix()
			{
				client = new DiscordRpcClient("952318590617518180");

				//Set the logger
				client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

				//Subscribe to events
				client.OnReady += (sender, e) =>
				{
					SuperNewRolesPlugin.Logger.LogInfo(string.Format("Received Ready from user {0}", e.User.Username));
				};

				client.OnPresenceUpdate += (sender, e) =>
				{
					SuperNewRolesPlugin.Logger.LogInfo(string.Format("Received Update! {0}", e.Presence));
				};

				//Connect to the RPC
				client.Initialize();

				//Set the rich presence
				//Call this as many times as you want and anywhere in your code.
				client.SetPresence(new RichPresence()
				{
					Details = "Example Project",
					State = "csharp example",
					Assets = new DiscordRPC.Assets()
					{
						LargeImageKey = "image_large",
						LargeImageText = "Lachee's Discord IPC Library",
						SmallImageKey = "image_small"
					}
				});
			}
		}
    }
}
*/