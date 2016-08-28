﻿using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public partial class HumanWalkSnipeTask
    {


        public class PokecrewWrap
        {
            public class PokecrewItem
            {
                public double latitude { get; set; }
                public double longitude { get; set; }
                public int pokemon_id { get; set; }
                public DateTime expires_at { get; set; }
            }
            public List<PokecrewItem> seens { get; set; }
        }

        private static RarePokemonInfo Map(PokecrewWrap.PokecrewItem result)
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            var unixBase = new DateTime(1970, 1, 1);
            long unixTime = ((result.expires_at.AddMinutes(-15).Ticks - epochTicks) / TimeSpan.TicksPerSecond);
            //double ticks = Math.Truncate((result.expires_at.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            //unixTime = result.expires_at.AddMinutes(-15) - 
            return new RarePokemonInfo()
            {
                latitude = result.latitude,
                longitude = result.longitude,
                pokemonId = result.pokemon_id,
                created = unixTime
            };
        }

        private static async Task<List<RarePokemonInfo>> FetchFromPokecrew(double lat, double lng)
        {
            List<RarePokemonInfo> results = new List<RarePokemonInfo>();
            // if (!_setting.HumanWalkingSnipeUsePokeRadar) return results;
            try
            {

                HttpClient client = new HttpClient();
                double offset = _setting.HumanWalkingSnipeSnipingScanOffset; //0.015 
                string url = $"https://api.pokecrew.com/api/v1/seens?center_latitude={lat}&center_longitude={lng}&live=true&minimal=false&northeast_latitude={lat + offset}&northeast_longitude={lng + offset}&pokemon_id=&southwest_latitude={lat - offset}&southwest_longitude={lng - offset}";

                var task = await client.GetStringAsync(url);

                var data = JsonConvert.DeserializeObject<PokecrewWrap>(task);
                foreach (var item in data.seens)
                {
                    var pItem = Map(item);
                    if (pItem != null)
                    {
                        results.Add(pItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error loading data", LogLevel.Error, ConsoleColor.DarkRed);
            }
            return results;
        }

    }
}
