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
// *****************************************************************************

namespace TaskWarriorLib
{
    public enum ErrorCode
    {
        Success,
        SuccessNoChanges,

        Redirect,
        TryLater,
        AccessDenied,
        AccountSuspended,
        CannotParseMessage,
       
        MessageErrors,
        ClientSyncKeyNotFound,

        CouldNotFindCommonAncestor,

        CannotCreateSocket,
        ConfigParseError, 
        NotEnabled,
        NoSuchCert,
        
        AccountVanished,
        NotSupportedSeconedRecurring
    }

    public sealed class ErrorConverter
    {
        public static ErrorCode FromInt(int code, out string msg)
        {
            ErrorCode ret;
            switch (code)
            {
                case 200:
                    msg = ("Success");
                    ret = ErrorCode.Success;
                    break;
                case 201:
                    msg = ("No change");
                    ret = ErrorCode.SuccessNoChanges;
                    break;
                case 300:
                    msg = ("Deprecated message type\n" + "This message will not be supported in future task server releases.");
                    ret = ErrorCode.Success;
                    break;
                case 301:
                    msg = ("Redirect\n" + "Further requests should be made to the specified server/port.");
                    // TODO
                    ret = ErrorCode.Success;
                    break;
                case 302:
                    msg = ("Retry\n" + "The client is requested to wait and retry the same request.  The wait\n" + "time is not specified, and further retry responses are possible.");
                    ret = ErrorCode.TryLater;
                    break;
                case 400:
                    msg = ("Malformed data");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 401:
                    msg = ("Unsupported encoding");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 420:
                    msg = ("Server temporarily unavailable");
                    ret = ErrorCode.TryLater;
                    break;
                case 421:
                    msg = ("Server shutting down at operator request");
                    ret = ErrorCode.TryLater;
                    break;
                case 430:
                    msg = ("Access denied");
                    ret = ErrorCode.AccessDenied;
                    break;
                case 431:
                    msg = ("Account suspended");
                    ret = ErrorCode.AccountSuspended;
                    break;
                case 432:
                    msg = ("Account terminated");
                    ret = ErrorCode.AccountSuspended;
                    break;
                case 500:
                    msg = ("Syntax error in request");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 501:
                    msg = ("Syntax error, illegal parameters");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 502:
                    msg = ("Not implemented");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 503:
                    msg = ("Command parameter not implemented");
                    ret = ErrorCode.MessageErrors;
                    break;
                case 504:
                    msg = ("Request too big");
                    ret = ErrorCode.MessageErrors;
                    break;
                default:
                    msg = ("Unknown code: " + code);
                    ret = ErrorCode.CannotParseMessage;
                    break;
            }

            return ret;
        }

        public static ErrorCode Convert(int replyCode, string headerStatus, out string errmsg)
        {
            if (replyCode == 500 && headerStatus != null && headerStatus.Contains("Client sync key not found"))
            {
                errmsg = headerStatus;
                return ErrorCode.ClientSyncKeyNotFound;
            }
            if (replyCode == 500 && headerStatus != null && headerStatus.Contains("Could not find common ancestor"))
            {
                errmsg = headerStatus;
                return ErrorCode.CouldNotFindCommonAncestor;
            }

            return FromInt(replyCode, out errmsg);
        }
    }

}