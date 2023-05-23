using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VideoPlayer
{

    public class Save
    {
        public string Path { get; set; }
        public double Volume { get; set; }
        public double Speed { get; set; }
        public TimeSpan TimeLine { get; set; }
        public List<string> lastOpenFiles { get; set; }
        public List<string> files { get; set; }

        [JsonConstructor]
        public Save()
        { }

        public Save(string path, double volume, double speed, TimeSpan timeline , List<string> lastOpenFiles, List<string> files)
        {
            Path = path;
            Volume = volume;
            Speed = speed;
            TimeLine = timeline;
            this.lastOpenFiles = lastOpenFiles;
            this.files = files;
        }
    }
}
