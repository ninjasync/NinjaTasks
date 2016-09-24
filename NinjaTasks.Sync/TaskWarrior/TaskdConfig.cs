﻿using System.Diagnostics;
using System.IO;
using System.Text;

namespace NinjaTasks.Sync.TaskWarrior
{
    public class TaskdConfig
    {
        public string Username { get; set; }
        public string Org { get; set; }
        public string Key { get; set; }
        public string ServerHostname { get; set; }
        public int ServerPort { get; set; }

        public string ClientCertificateAndKey { get; set; }
        public string RootCaCertificate { get; set; }
    }

    /// <summary>
    /// parses a .taskdconfig file, as generated by (...TODO!)
    /// </summary>
    public static class TaskdConfigFile
    {

        //public static TaskdConfig Load(string filename)
        //{
        //    using (var s = new StreamReader(filename, Encoding.UTF8))
        //        return Load(s);
        //}

        public static TaskdConfig Load(StreamReader stream)
        {
            return Parse(stream.ReadToEnd());
        }

        public static TaskdConfig Parse(string txt)
        {
            TaskdConfig ret = new TaskdConfig();
            string clientCert="", clientKey="", caCert="";
            
            string[] lines = txt.Split('\n');

            for(int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();

                if (string.IsNullOrEmpty(line)) continue;
                int idxColon = line.IndexOf(':');
                if (idxColon == -1)
                {
                    Debug.WriteLine("warning: invalid line: " + line);
                    continue;
                }

                string key = line.Substring(0, idxColon).Trim();
                string value = line.Substring(idxColon + 1).Trim();

                if (key == "username") ret.Username = value;
                else if (key == "org") ret.Org = value;
                else if (key == "user key") ret.Key = value;
                else if (key == "server")
                {
                    string[] split = value.Split(':');
                    ret.ServerHostname = split[0].Trim();
                    if(split.Length > 1)
                        ret.ServerPort = int.Parse(split[1].Trim());
                }
                else if (key == "Client.cert")
                    clientCert = ReadCertificate(ref i, lines);
                else if (key == "Client.key")
                    clientKey = ReadCertificate(ref i, lines);
                else if (key == "ca.cert")
                    caCert = ReadCertificate(ref i, lines);
                else
                {
                    Debug.WriteLine("warning: invalid line: " + line);
                    continue;
                }
            }

            ret.ClientCertificateAndKey = clientCert + "\n" + clientKey;
            ret.RootCaCertificate = caCert;
            return ret;
        }

        private static string ReadCertificate(ref int i, string[] lines)
        {
            StringBuilder ret = new StringBuilder();
            for (++i; i < lines.Length; ++i)
            {
                ret.AppendLine(lines[i]);
                if (lines[i].StartsWith("-----END"))
                    break;
            }
            return ret.ToString();

        }
    }
}