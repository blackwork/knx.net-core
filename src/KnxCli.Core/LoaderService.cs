using System;
using System.Collections.Generic;
using System.IO;
using KnxCli.Core.Actors;
using Newtonsoft.Json;

namespace KnxCli.Core
{
    public class LoaderService
    {
        public ActorsModel LoadActors(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Actors file not found.", filename);
            }

            var json = File.ReadAllText(filename);
            var actors = JsonConvert.DeserializeObject<List<Actor>>(json);
            return new ActorsModel(actors);
        }

        public KnxSettings LoadSettings(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Settings file not found.", filename);
            }

            var json = File.ReadAllText(filename);
            var settings = JsonConvert.DeserializeObject<KnxSettings>(json);
            return settings;
        }
    }
}
