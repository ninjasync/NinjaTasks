//*****************************************************************************
// Mirakel is an Android App for managing your ToDo-Lists
// 
// Copyright (c) 2013-2014 Anatolij Zelenin, Georg Semmler.
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// *****************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TaskWarriorLib.Network
{
	public class TaskWarriorMsg
	{
		private readonly Dictionary<string, string> _header = new Dictionary<string, string>(5);

	    public TaskWarriorMsg()
		{
			this.Payload = "";
			// All messages are marked with the version number, so that the messages
			// may be properly evaluated in context.
		    this._header["client"] = "ninjatasks 0.5";
		}

		public void Clear()
		{
			this._header.Clear();
			this.Payload = "";
		}

        public List<string> Headers { get { return _header.Keys.ToList(); }}

		public void Set<T>(string key, T value)
		{
			this._header[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
		}

	    public string Payload { get; set; }

		public string GetHeader(string name)
		{
			if (_header.ContainsKey(name))
				return _header[name];
			else
			    return null;
		}

		public string Serialize()
		{
			StringBuilder output = new StringBuilder();
			foreach (var entry in _header)
			{
				output.Append(entry.Key + ": " + entry.Value + '\n');
			}
			output.Append('\n' + Payload + "\n");
			return output.ToString();
		}

		public void Parse(string input)
		{
            this.Clear();
#if DOT42
            int separator = input.IndexOf("\n\n");
#else
            int separator = input.IndexOf("\n\n", StringComparison.Ordinal);
#endif
            if (separator == -1)
				throw new Exception("malformed input.");
			
            // Parse header.
			string[] a = input.Substring(0, separator).Split('\n');
			foreach (String s in a)
			{
			    if (string.IsNullOrEmpty(s)) continue;

				int delimiter = s.IndexOf(':');
				if (delimiter == -1)
                    throw new Exception("malformed input.");

				this._header[s.Substring(0, delimiter).Trim()] = s.Substring(delimiter + 1).Trim();
			}
			this.Payload = input.Substring(separator + 2).Trim();
		}
	}

}