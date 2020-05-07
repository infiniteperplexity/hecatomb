using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Hecatomb8
{
    class ExceptionHandling
    {
        public static void Handle(Exception e)
        {
            Debug.WriteLine(e.ToString());
            if (!HecatombOptions.NoErrorLog)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
                System.IO.Directory.CreateDirectory(path + @"\logs");
                var body = new List<string>();
                body.Add("Build Date:" + "\"" + GameManager.BuildDate.ToString() + "\"");
                body.Add(" ");
                body.Add(e.ToString());
                string json = "";
                if (GameState.World != null)
                {
                    json = CommandLogger.DumpLog();
                    body[0] += (", Seed: " + "\"" + GameState.World!.Random.Seed + "\"");
                    body.Add(" ");
                    body.Add("Logged Commands:");
                    body.Add(json);
                }
                string filePath = path + @"\logs\" + "HecatombCrashReport" + timestamp + ".txt";
                System.IO.File.WriteAllLines(filePath, body);
                string messageBody = "Oh no!  Hecatomb has crashed!  Please send this crash report to the supplied address.  If possible, please attach the crash log file: " + filePath + "%0A%0A" + String.Join("%0A", body);
                try
                {
                    Process.Start(String.Format(
                        "mailto:{0}?subject={1}&body={2}",
                        "hecatomb.gamedev@gmail.com",
                        "Hecatomb crash report: " + timestamp,
                        messageBody
                    ));
                }
                catch (Exception e2)
                {
                    Debug.WriteLine("Error in e-mail call");
                    // this is allowed to fail silently
                }
                var replaced = (path + @"\logs\").Replace(@"\", "-").Replace(":", "~");
                try
                {
                    Process.Start("https://infiniteperplexity.github.io/hecatomb/crashReport.html?timestamp=" + timestamp + "&path=" + replaced);
                    
                }
                catch (Exception e2)
                {
                    Debug.WriteLine("Error in browser call");
                    // this is allowed to fail silently
                }
            }
            // this method of handling preserves the stack trace
            var capturedException = ExceptionDispatchInfo.Capture(e);
            Debug.WriteLine(capturedException.ToString());
            capturedException.Throw();
        }
    }
}
