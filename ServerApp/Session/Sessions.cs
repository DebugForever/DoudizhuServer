using System;
using System.Collections.Generic;

namespace ServerApp.Session
{
    public static class Sessions
    {
        public static MatchSession matchSession { get; private set; }
        public static PlaySession playSession { get; private set; }
        static Sessions()
        {
            matchSession = new MatchSession();
            playSession = new PlaySession();
        }
    }
}
