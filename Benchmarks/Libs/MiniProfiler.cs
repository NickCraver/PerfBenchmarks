using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StackExchange.Profiling
{
    /// <summary>
    /// A single MiniProfiler can be used to represent any number of steps/levels in a call-graph, via Step()
    /// </summary>
    /// <remarks>Totally baller.</remarks>
    [DataContract]
    public class MiniProfiler
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public DateTime Started { get; set; }

        [DataMember(Order = 4)]
        public decimal DurationMilliseconds { get; set; }

        [DataMember(Order = 5)]
        public string MachineName { get; set; }

        [DataMember(Order = 6)]
        public Dictionary<string, string> CustomLinks { get; set; }
        
        [DataMember(Order = 7)]
        public Timing Root { get; set; }

        [DataMember(Order = 8)]
        public ClientTimings ClientTimings { get; set; }

        [DataMember(Order = 9)]
        public string User { get; set; }

        [DataMember(Order = 10)]
        public bool HasUserViewed { get; set; }

        public override string ToString()
        {
            return Root != null ? Root.Name + " (" + DurationMilliseconds + " ms)" : "";
        }
        public override bool Equals(object other)
        {
            return other is MiniProfiler && Id.Equals(((MiniProfiler)other).Id);
        }
        public override int GetHashCode() => Id.GetHashCode();
    }
    
    [DataContract]
    public class Timing
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }
        
        [DataMember(Order = 2)]
        public string Name { get; set; }
        
        [DataMember(Order = 3)]
        public decimal? DurationMilliseconds { get; set; }
        
        [DataMember(Order = 4)]
        public decimal StartMilliseconds { get; set; }
        
        [DataMember(Order = 5)]
        public List<Timing> Children { get; set; }
        
        [DataMember(Order = 6)]
        public Dictionary<string, List<CustomTiming>> CustomTimings { get; set; }
    }

    [DataContract]
    public class CustomTiming
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }
        
        [DataMember(Order = 2)]
        public string CommandString { get; set; }
        
        [DataMember(Order = 3)]
        public string ExecuteType { get; set; }
        
        [DataMember(Order = 4)]
        public string StackTraceSnippet { get; set; }
        
        [DataMember(Order = 5)]
        public decimal StartMilliseconds { get; set; }
        
        [DataMember(Order = 6)]
        public decimal? DurationMilliseconds { get; set; }
        
        [DataMember(Order = 7)]
        public decimal? FirstFetchDurationMilliseconds { get; set; }

        internal string Category { get; set; }
    }
    
    [DataContract]
    public class ClientTimings
    {
        [DataMember(Order = 1)]
        public int RedirectCount { get; set; }

        [DataMember(Order = 2)]
        public List<ClientTiming> Timings { get; set; }
    }

    [DataContract]
    public class ClientTiming
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }
        
        [DataMember(Order = 2)]
        public decimal Start { get; set; }
        
        [DataMember(Order = 3)]
        public decimal Duration { get; set; }
        
        public Guid Id { get; set; }
        
        public Guid MiniProfilerId { get; set; }
    }
}