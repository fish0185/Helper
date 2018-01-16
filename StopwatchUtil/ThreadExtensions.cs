using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helper
{
    public static class ThreadExtensions
    {
        /// <summary>
        /// one of four useful values: Unstarted, Running, WaitSleepJoin, and Stopped:
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static ThreadState Simplify(this ThreadState ts)
        {
            return ts & (ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin |
                         ThreadState.Stopped);
        }
    }
}
