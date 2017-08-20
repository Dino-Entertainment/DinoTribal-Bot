﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;


namespace TheGodfatherBot
{
    [Description("SWAT4 related commands.")]
    public class CommandsSwat
    {
        private bool checking = false;
        private static Dictionary<string, string> ServerList = new Dictionary<string, string>();


        public static void LoadServers()
        {
            string[] serverlist = {
                "# Format: <name>$<IP>",
                "# You can add your own servers and IPs and reorder them as you wish",
                "# The program doesn't require server names to be exact, you can rename them as you wish",
                "# Every line starting with '#' is considered as a comment, blank lines are ignored",
                "",
                "wm$46.251.251.9:10880:10881",
                "myt$51.15.152.220:10480:10481",
                "4u$109.70.149.161:10480:10481",
                "soh$158.58.173.64:16480:10481",
                "sh$5.9.50.39:8480:8481",
                "esa$77.250.71.231:11180:11181",
                "kos$31.186.250.32:10480:10481"
            };

            if (!File.Exists("servers.txt")) {
                FileStream f = File.Open("servers.txt", FileMode.CreateNew);
                f.Close();
                File.WriteAllLines("servers.txt", serverlist);
            }

            try {
                serverlist = File.ReadAllLines("servers.txt");
                foreach (string line in serverlist) {
                    if (line.Trim() == "" || line[0] == '#')
                        continue;
                    var values = line.Split('$');
                    ServerList.Add(values[0], values[1]);
                }
            } catch (Exception) {
                return;
            }
        }


        [Command("servers")]
        [Description("Print the serverlist.")]
        public async Task Servers(CommandContext ctx)
        {
            await ctx.RespondAsync("Not implemented yet.");
        }


        [Command("query")]
        [Description("Return server information.")]
        [Aliases("info")]
        public async Task Query(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (ip == null || ip.Trim() == "") {
                await ctx.RespondAsync("IP missing.");
                return;
            }

            if (ServerList.ContainsKey(ip)) {
                ip = ServerList[ip];
            } else {
                await ctx.RespondAsync("Unknown short name.");
                return;
            }

            try {
                var split = ip.Split(':');
                var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                if (info != null)
                    await SendEmbedInfo(ctx, split[0], info);
                else
                    await ctx.RespondAsync("No reply from server.");
            } catch (Exception) {
                await ctx.RespondAsync("Invalid IP format.");
            }
        }


        [Command("startcheck")]
        [Description("Notifies of free space in server.")]
        [Aliases("checkspace", "spacecheck")]
        public async Task StartCheck(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (ip == null || ip.Trim() == "") {
                await ctx.RespondAsync("IP missing.");
                return;
            }

            if (checking) {
                await ctx.RespondAsync("Already checking for space!");
                return;
            }

            if (ServerList.ContainsKey(ip)) {
                ip = ServerList[ip];
            } else {
                await ctx.RespondAsync("Unknown short name.");
                return;
            }

            checking = true;
            while (checking) {
                try {
                    var split = ip.Split(':');
                    var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                    if (info != null && int.Parse(info[1]) < int.Parse(info[2]))
                        await ctx.RespondAsync(ctx.User.Mention + ", there is space on " + info[0]);
                    else {
                        await ctx.RespondAsync("No reply from server.");
                        await StopCheck(ctx);
                    }
                } catch (Exception) {
                    await ctx.RespondAsync("Invalid IP format.");
                    await StopCheck(ctx);
                }
                await Task.Delay(1000);
            }
        }


        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        public async Task StopCheck(CommandContext ctx)
        {
            checking = false;
            await ctx.RespondAsync("Checking stopped.");
        }


        private string[] QueryIP(CommandContext ctx, string ip, int port)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            client.Connect(ep);
            client.Client.SendTimeout = 1000;
            client.Client.ReceiveTimeout = 1000;

            byte[] receivedData = null;
            try {
                string query = "\\status\\";
                client.Send(Encoding.ASCII.GetBytes(query), query.Length);
                receivedData = client.Receive(ref ep);
            } catch (Exception) {
                return null;
            }

            if (receivedData == null)
                return null;

            client.Close();
            var data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);

            var split = data.Split('\\');
            int index = 0;
            foreach (var s in split) {
                if (s == "numplayers")
                    break;
                index++;
            }

            if (index < 10) {
                index++;
                return new string[] { split[4], split[index], split[index + 2] };
            }

            return null;
        }


        private async Task SendEmbedInfo(CommandContext ctx, string ip, string[] info)
        {
            var embed = new DiscordEmbed() {
                Title = info[0],
                Description = ip,
                Timestamp = DateTime.Now
            };
            var field = new DiscordEmbedField() {
                Name = "Players",
                Value = info[1] + "/" + info[2]
            };
            embed.Fields.Add(field);
            await ctx.RespondAsync("", embed: embed);
        }
    }
}
