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
using System.Globalization;
using System.Text.RegularExpressions;
using NinjaTools.Logging;

namespace TaskWarriorLib.Parser
{
	public class TaskWarriorRecurrenceParser
	{
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public TaskWarriorRecurrenceParser(string recur) 
		{
            int number = 1;

            var reNumber = Regex.Match(recur, "[^0-9]+");

		    if (reNumber.Success)
		        number = int.Parse(reNumber.NextMatch().Value, CultureInfo.InvariantCulture);
			
			// remove number and possible sign(recurrence should be positive but who knows)
			switch (recur.Replace(Convert.ToString(number, CultureInfo.InvariantCulture), "")
                         .Replace("-", ""))
			{
			case "yearly":
			case "annual":
				number = 1;
				goto case "years";
			case "years":
			case "year":
			case "yrs":
			case "yr":
			case "y":
				Years = number;
				break;
			case "semiannual":
				Months = 6;
				break;
			case "biannual":
			case "biyearly":
				Years = 2;
				break;
			case "bimonthly":
				Months = 2;
				break;
			case "biweekly":
			case "fortnight":
				Days = 14;
				break;
			case "daily":
				number = 1;
				goto case "days";
			case "days":
			case "day":
			case "d":
				Days = number;
				break;
			case "hours":
			case "hour":
			case "hrs":
			case "hr":
			case "h":
				Hours = number;
				break;
			case "minutes":
			case "mins":
			case "min":
				Minutes = number;
				break;
			case "monthly":
				number = 1;
				goto case "months";
			case "months":
			case "month":
			case "mnths":
			case "mths":
			case "mth":
			case "mos":
			case "mo":
				Months = number;
				break;
			case "quarterly":
				number = 1;
				goto case "quarters";
			case "quarters":
			case "qrtrs":
			case "qtrs":
			case "qtr":
			case "q":
				Months = 3 * number;
				break;
			default: // Was genau soll das sein? Default hat doch hier nichts verloren…
				goto case "seconds";
			case "seconds":
			case "secs":
			case "sec":
			case "s":
                    Log.Warn("reccurrence not supported: ", recur);
                    throw new NotSupportedException("reccurrence not supported: " + recur);
			case "weekdays":
				bool[] weekdays = new bool[7];
				for (int i = 0; i <= 5; i++)
				    weekdays[i] = true;
				Weekdays = weekdays;
				break;
			case "sennight":
			case "weekly":
				number = 1;
				goto case "weeks";
			case "weeks":
			case "week":
			case "wks":
			case "wk":
			case "w":
				Days = 7 * number;
				break;
			}

			SourceString = recur;
		}

        public int Minutes { get; set; }
        public int Hours { get; set; }
        public int Days { get; set; }
        public int Months { get; set; }
        public int Years { get; set; }

	    public bool[] Weekdays { get; set; }

	    public string SourceString { get; set; }


	    
	}

}