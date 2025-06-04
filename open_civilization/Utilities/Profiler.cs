using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class Profiler
    {
        private Dictionary<string, ProfilerData> _data;
        private Stack<string> _stack;

        public Profiler()
        {
            _data = new Dictionary<string, ProfilerData>();
            _stack = new Stack<string>();
        }

        public void BeginSample(string name)
        {
            _stack.Push(name);
            if (!_data.ContainsKey(name))
            {
                _data[name] = new ProfilerData();
            }
            _data[name].StartTime = DateTime.UtcNow;
        }

        public void EndSample()
        {
            if (_stack.Count == 0) return;

            string name = _stack.Pop();
            var data = _data[name];
            data.TotalTime += (DateTime.UtcNow - data.StartTime).TotalMilliseconds;
            data.CallCount++;
        }

        public void Reset()
        {
            foreach (var data in _data.Values)
            {
                data.Reset();
            }
        }

        public void LogResults()
        {
            Console.WriteLine("=== PROFILER RESULTS ===");
            foreach (var kvp in _data.OrderByDescending(x => x.Value.TotalTime))
            {
                var data = kvp.Value;
                double avgTime = data.CallCount > 0 ? data.TotalTime / data.CallCount : 0;
                Console.WriteLine($"{kvp.Key}: {data.TotalTime:F2}ms total, {avgTime:F2}ms avg, {data.CallCount} calls");
            }
        }

        private class ProfilerData
        {
            public DateTime StartTime;
            public double TotalTime;
            public int CallCount;

            public void Reset()
            {
                TotalTime = 0;
                CallCount = 0;
            }
        }
    }
}
