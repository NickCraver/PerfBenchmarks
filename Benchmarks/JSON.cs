using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using StackExchange.Profiling;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class JSONTests
    {
        private static readonly MiniProfiler _testProfiler = GetMiniProfiler();
        private static readonly string _testProfilerJSON = Jil.JSON.Serialize(_testProfiler);
        
        [Benchmark]
        public string JilSerialize() => Jil.JSON.Serialize(_testProfiler);
        [Benchmark]
        public string NewtonsoftSerialize() => Newtonsoft.Json.JsonConvert.SerializeObject(_testProfiler);
        
        [Benchmark]
        public MiniProfiler JilDeserialize() => Jil.JSON.Deserialize<MiniProfiler>(_testProfilerJSON);
        [Benchmark]
        public MiniProfiler NewtonsoftDeserialize() => Newtonsoft.Json.JsonConvert.DeserializeObject<MiniProfiler>(_testProfilerJSON);

        private static MiniProfiler GetMiniProfiler()
        {
            var id = Guid.NewGuid();
            int timingPosition = 0;
            int timingId = 0;

            return new MiniProfiler
            {
                Id = id,
                CustomLinks = new Dictionary<string, string>
                {
                    ["google"] = "google.com",
                    ["stack overflow"] = "stackoverflow.com"
                },
                ClientTimings = new ClientTimings
                {
                    Timings = new List<ClientTiming>
                    {
                        new ClientTiming
                        {
                            Id = Guid.NewGuid(),
                            MiniProfilerId = id,
                            Name = "stuff",
                            Start = 12,
                            Duration = 2
                        },
                        new ClientTiming
                        {
                            Id = Guid.NewGuid(),
                            MiniProfilerId = id,
                            Name = "stuff 2",
                            Start = 14,
                            Duration = 2
                        },
                        new ClientTiming
                        {
                            Id = Guid.NewGuid(),
                            MiniProfilerId = id,
                            Name = "stuff 3",
                            Start = 16,
                            Duration = 3
                        }
                    }
                },
                DurationMilliseconds = 45,
                MachineName = Environment.MachineName,
                Name = "Test MiniProfiler",
                Root = new Timing
                {
                    Id = Guid.NewGuid(),
                    Name = $"Timing {timingId++}",
                    StartMilliseconds = timingPosition,
                    DurationMilliseconds = 12,
                    Children = new List<Timing>
                    {
                        new Timing
                        {
                            Id = Guid.NewGuid(),
                            Name = $"Timing {timingId++}",
                            StartMilliseconds = timingPosition += 10,
                            DurationMilliseconds = 10,
                            Children = new List<Timing>
                            {
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10,
                                    CustomTimings = new Dictionary<string, List<CustomTiming>>
                                    {
                                        {
                                            "SQL", new List<CustomTiming>
                                            {
                                                new CustomTiming
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Category = "SQL",
                                                    CommandString = "SELECT * FROM TABLE",
                                                    DurationMilliseconds = 2,
                                                    StartMilliseconds = 22,
                                                    ExecuteType = "SELECT",
                                                    StackTraceSnippet = "MethodC MethodB MethodA"
                                                }
                                            }
                                        }
                                    }
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                }
                            }
                        },
                        new Timing
                        {
                            Id = Guid.NewGuid(),
                            Name = $"Timing {timingId++}",
                            StartMilliseconds = timingPosition += 10,
                            DurationMilliseconds = 10,
                            Children = new List<Timing>
                            {
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                }
                            }
                        },
                        new Timing
                        {
                            Id = Guid.NewGuid(),
                            Name = $"Timing {timingId++}",
                            StartMilliseconds = timingPosition += 10,
                            DurationMilliseconds = 10,
                            Children = new List<Timing>
                            {
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                },
                                new Timing
                                {
                                    Id = Guid.NewGuid(),
                                    Name = $"Timing {timingId++}",
                                    StartMilliseconds = timingPosition += 10,
                                    DurationMilliseconds = 10
                                }
                            }
                        }
                    }
                },
                Started = DateTime.UtcNow,
                User = Environment.UserName
            };
        }
    }
}
