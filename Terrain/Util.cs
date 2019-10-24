// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Collections.Generic;
using EconSim.Math;

namespace EconSim.Terrain
{
    public static class Util
    {
        public enum TerrainType
        {
            Blank = 0,
            Land = 100,
            Water = 200,
            Lake = 201,
            Ocean = 202,
            Coast = 203
        }

        public enum BiomeType
        {
            TropicalRainForest = 100,
            TropicalSeasonalForest = 101,
            TemperateRainForest = 102,
            Grassland = 200,
            SubtropicalDesert = 300,
            TemperateDesert = 301,
            TemperateDeciduousForest = 302,
            Taiga = 400,
            Shrubland = 401,
            Scorched = 500,
            Bare = 501,
            Tundra = 502,
            Snow = 503
        }

        /// <summary>
        /// Moisture zones are on x-axis, elevation on y-axis
        /// </summary>
        public static BiomeType[,] BiomeMatrix =>
          new[,]
          {
        {
          BiomeType.Snow, BiomeType.Snow, BiomeType.Snow,
          BiomeType.Tundra, BiomeType.Bare, BiomeType.Scorched
        },
        {
          BiomeType.Taiga, BiomeType.Taiga, BiomeType.Shrubland,
          BiomeType.Shrubland, BiomeType.TemperateDesert, BiomeType.TemperateDesert
        },
        {
          BiomeType.TemperateRainForest, BiomeType.TemperateDeciduousForest,
          BiomeType.TemperateDeciduousForest,
          BiomeType.Grassland, BiomeType.Grassland, BiomeType.TemperateDesert
        },
        {
          BiomeType.TropicalRainForest, BiomeType.TropicalRainForest, BiomeType.TropicalSeasonalForest,
          BiomeType.TropicalSeasonalForest, BiomeType.Grassland, BiomeType.SubtropicalDesert
        }
          };

        /// <summary>
        /// Iterate through terrain types with an action
        /// </summary>
        /// <param name="action"></param>
        public static void ForEachTerrainType(Action<TerrainType> action)
        {
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                action(terrainType);
            }
        }

        /// <summary>
        /// Iterate through biomes with an action
        /// </summary>
        /// <param name="action"></param>
        public static void ForEachBiome(Action<BiomeType> action)
        {
            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
            {
                action(biome);
            }
        }

        /// <summary>
        /// Get a biome based on moisture and elevation in range [0,1]
        /// </summary>
        /// <param name="moisture"></param>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public static BiomeType GetBiomeType(in float moisture, in float elevation)
        {
            BiomeType[,] biomeMatrix = BiomeMatrix;

            int moistureZones = biomeMatrix.GetLength(0);
            int elevationZones = biomeMatrix.GetLength(1);

            int moistureZone = Convert.ToInt32(moisture.Clamp01() * (moistureZones - 1));
            int elevationZone = Convert.ToInt32(elevation.Clamp01() * (elevationZones - 1));

            return biomeMatrix[moistureZone, elevationZone];
        }

        /// <summary>
        /// Returns true if any of the terrain types in the list is aquatic
        /// </summary>
        /// <param name="terrainTypes"></param>
        /// <returns></returns>
        public static bool ContainsAquatic(this IEnumerable<TerrainType> terrainTypes)
        {
            foreach (var terrainType in terrainTypes)
            {
                if (IsAquatic(terrainType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if terrainTypes contains any of the terrain types in checkTypes
        /// </summary>
        /// <param name="terrainTypes"></param>
        /// <param name="checkTypes"></param>
        /// <returns></returns>
        public static bool ContainsAnyTerrain(this IEnumerable<TerrainType> terrainTypes, params TerrainType[] checkTypes)
        {
            foreach (var checkType in checkTypes)
            {
                if (terrainTypes.ContainsTerrain(checkType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the enumerable contains a terrain type
        /// </summary>
        /// <param name="terrainTypes"></param>
        /// <param name="checkType"></param>
        /// <returns></returns>
        public static bool ContainsTerrain(this IEnumerable<TerrainType> terrainTypes, TerrainType checkType)
        {
            foreach (var terrainType in terrainTypes)
            {
                if (terrainType == checkType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the supplied terrain type is aquatic
        /// </summary>
        /// <param name="terrainType"></param>
        /// <returns></returns>
        public static bool IsAquatic(this TerrainType terrainType)
        {
            return terrainType == TerrainType.Water || terrainType == TerrainType.Lake ||
                terrainType == TerrainType.Ocean || terrainType == TerrainType.Coast;
        }

        public static TerrainColor GetTerrainColor(this TerrainType terrainType)
        {
            if (terrainType == TerrainType.Blank)
            {
                return new TerrainColor(0.0f, 0.0f, 0.0f, 1);
            }

            if (terrainType == TerrainType.Land)
            {
                return new TerrainColor(0.1f, 0.8f, 0.2f, 1);
            }

            if (terrainType == TerrainType.Water)
            {
                return new TerrainColor(0.0f, 0.0f, 1.0f, 1);
            }

            if (terrainType == TerrainType.Ocean)
            {
                return new TerrainColor(0.0f, 0.2f, 0.6f, 1);
            }

            if (terrainType == TerrainType.Coast)
            {
                return new TerrainColor(0.2f, 0.4f, 0.8f, 1);
            }

            if (terrainType == TerrainType.Lake)
            {
                return new TerrainColor(0.3f, 0.5f, 0.95f, 1);
            }

            return new TerrainColor(1.0f, 0.0f, 1.0f, 1);
        }

    }
}