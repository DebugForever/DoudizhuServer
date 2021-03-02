using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp
{
    /// <summary>
    /// 用于强制单线程执行的类，在访问数据库的时候会用到。
    /// 相当于所有线程都能用的一个互斥锁
    /// </summary>
    public class SingleExec
    {
        private static Mutex mutex = new Mutex();
        public static void Exec(Action action)
        {
            mutex.WaitOne();
            action();
            mutex.ReleaseMutex();
        }
    }
}
