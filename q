[1mdiff --git a/SanaraV2/Modules/Entertainment/Game.cs b/SanaraV2/Modules/Entertainment/Game.cs[m
[1mindex 6b32b49..0fbbabd 100644[m
[1m--- a/SanaraV2/Modules/Entertainment/Game.cs[m
[1m+++ b/SanaraV2/Modules/Entertainment/Game.cs[m
[36m@@ -21,6 +21,7 @@[m [musing System.Collections.Generic;[m
 using System.IO;[m
 using System.Linq;[m
 using System.Net;[m
[32m+[m[32musing System.Net.Http;[m
 using System.Text;[m
 using System.Threading.Tasks;[m
 [m
[36m@@ -75,14 +76,18 @@[m [mnamespace SanaraV2.Modules.Entertainment[m
                 foreach (string msg in GetPost())[m
                 {[m
                     Console.WriteLine(msg);[m
[32m+[m[32m                    using (HttpClient hc = new HttpClient())[m
[32m+[m[32m                        await m_chan.SendFileAsync(await hc.GetStreamAsync(msg), "Sanara-image." + msg.Split('.').Last());[m
[32m+[m[32m                    /*[m
                     if (Features.Utilities.IsImage(msg.Split('.').Last()))[m
[31m-                        await m_chan.SendMessageAsync("", false, new EmbedBuilder()[m
[32m+[m[32m                        await m_chan.SendFileAsync("", false, new EmbedBuilder()[m
                         {[m
                             Color = Color.Blue,[m
                             ImageUrl = msg[m
                         }.Build());[m
                     else[m
                         await m_chan.SendMessageAsync(msg);[m
[32m+[m[32m                        */[m
                 }[m
                 m_time = DateTime.Now;[m
             }[m
