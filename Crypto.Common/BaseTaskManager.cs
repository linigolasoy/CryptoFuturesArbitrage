using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{
    internal class BaseTaskManager<T> : ITaskManager<T>
    {
        private List<Task<T>> m_aTasks = new List<Task<T>>();   
        public BaseTaskManager( int nMaximum ) 
        { 
            Maximum = nMaximum; 
        }
        public int Maximum { get; }

        private List<T> m_aResults = new List<T>();


        private async Task EndTasks()
        {
            if (m_aTasks.Count <= 0) return;
            await Task.WhenAll(m_aTasks);
            foreach (var oDone in m_aTasks)
            {
                m_aResults.Add(oDone.Result);
            }
            m_aTasks.Clear();

        }

        public async Task Add(Task<T> oTask)
        {
            m_aTasks.Add(oTask);
            if (m_aTasks.Count < Maximum) return;
            await EndTasks();

        }

        public async Task<T[]> GetResults()
        {
            await EndTasks();
            T[] aResults = m_aResults.ToArray();
            m_aResults.Clear();
            return aResults;    
        }
    }
}
