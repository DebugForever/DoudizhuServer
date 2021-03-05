using System;
using System.Collections.Generic;

namespace ServerApp.Session
{
    public static class Sessions
    {
        public static MatchSession matchSession { get; private set; }
        static Sessions()
        {
            matchSession = new MatchSession();
        }
    }
}
