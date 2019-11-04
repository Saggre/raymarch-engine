// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using EconSim.Collections;
using EconSim.Geometry;
using EconSim.EMath;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = System.Drawing.Color;
using ComputeShader = EconSim.EMath.ComputeShader;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Terrain
{
    using Math = System.Math;

    public class TerrainChunk
    {
        private FastNoise noise;
        private TerrainPlane terrainPlane;
        private SquareRect bounds;
        private Stopwatch stopwatch;

        private CardinalCollection<TerrainChunk> neighborChunks; // TODO

        // tiles indexed by terrain type
        private Dictionary<Util.TerrainType, List<Tile>> tilesIndexedByTerrain;

        public TerrainChunk(SquareRect bounds, FastNoise noise)
        {
            this.noise = noise;
            this.bounds = bounds;
            Create();
        }

        private void Create()
        {
            Console.WriteLine("Creating a terrain chunk at (" + bounds.X + ", " + bounds.Y + ")");

            stopwatch = new Stopwatch();
            stopwatch.Start();

            tilesIndexedByTerrain = new Dictionary<Util.TerrainType, List<Tile>>();
            Util.ForEachTerrainType(delegate (Util.TerrainType terrainType)
            {
                tilesIndexedByTerrain.Add(terrainType, new List<Tile>());
            });

            //Voronoi voronoi = CreateVoronoi(area);
            terrainPlane = new TerrainPlane(bounds.Size);
            ProfileTime("Vertices creation");

            //AssignTerrainTypes();
            //ProfileTime("Assigning terrain types");

            //CreateLakes();
            //ProfileTime("Creating lakes");

            AssignElevations();
            ProfileTime("Assigning elevations");

            //AssignBiomes(voronoi);
            //ProfileTime("Creating biomes");

            //Dictionary<Util.TerrainType, Texture2D> terrainMaps = GetTerrainMaps();
            ProfileTime("Rendering terrain textures");

        }

        void ProfileTime(string text)
        {
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Debug.WriteLine("(" + ts.Milliseconds + "ms) " + text);
            stopwatch.Reset();
            stopwatch.Start();
        }

        /*Dictionary<Util.TerrainType, Texture2D> GetTerrainMaps()
        {
          CameraCapture cc = CreateCaptureCamera();
          Dictionary<Util.TerrainType, RenderTexture> dictionary = new Dictionary<Util.TerrainType, RenderTexture>();

          Util.ForEachTerrainType(delegate (Util.TerrainType terrainType)
          {
            Mesh mesh = BuildMeshFromTiles(voronoi, GetTilesByTerrainType(terrainType));
            //Debug.Log("VERTS: " + mesh.vertices.Length);
            //GetComponent<MeshFilter>().mesh = mesh;
            //Debug.Break();

            RenderTexture rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.R8);
            //TODO size
            cc.RenderObject(mesh, Helpers.GetDefaultMaterial(), ref rt, 5f);

            dictionary.Add(terrainType, rt);
          });

          return dictionary;
        }*/

        /*CameraCapture CreateCaptureCamera()
        {
          GameObject cameraRig = new GameObject("CaptureCamera", typeof(Camera), typeof(CameraCapture));
          CameraCapture cameraCapture = cameraRig.GetComponent<CameraCapture>();
          cameraCapture.Init();
          return cameraCapture;
        }*/

        /*private List<Vector2> CreateRandomPointList(int numberOfPoints, RectInt area)
        {
          List<Vector2> points = new List<Vector2>();

          for (int i = 0; i < numberOfPoints; i++)
          {
            points.Add(new Vector2(area.x + UnityEngine.Random.Range(0f, area.width), area.y + UnityEngine.Random.Range(0f, area.width)));
          }

          return points;
        }*/

        /// <summary>
        /// Assign elevation to vertices
        /// </summary>
        private void AssignElevations()
        {
            terrainPlane.ForEachVertex((int x, int y, int i) =>
            {
                terrainPlane.Vertices[x, y].Elevation = noise.GetNoise(bounds.X + x, bounds.Y - y);
            });
        }

        /// <summary>
        /// Assign biomes to tiles based on connected vertices moisture and elevation
        /// </summary>
        private void AssignBiomes()
        {
            foreach (Tile tile in terrainPlane.Tiles)
            {
                if (tile.TerrainType.IsAquatic())
                {
                    continue;
                }

                tile.BiomeType = Util.GetBiomeType(tile.GetMoisture(), tile.GetElevation());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="distance"></param>
        /// <param name="terrainTypes"></param>
        /// <returns></returns>
        private Tile GetClosestTileOfType(in Vector2 startPoint, out float distance,
            params Util.TerrainType[] terrainTypes)
        {
            // Contain the start point inside bounds

            int startX = ((int)startPoint.X).Clamp(0, bounds.Size - 1);
            int startY = ((int)startPoint.Y).Clamp(0, bounds.Size - 1);

            Tile closestTile = null;

            terrainPlane.Tiles.ForEachHelical(startX, startY, (Tile tile) =>
            {
                if (closestTile == null && terrainTypes.ContainsTerrain(tile.TerrainType))
                {
                    closestTile = tile;
                }
            });

            distance = closestTile == null ? 0f : startPoint.ManhattanDistance(closestTile.Centroid);
            return closestTile;
        }

        /*
        private Tile GetClosestTileOfType(in Vector2 startPoint, out float distance, params Util.TerrainType[] terrainTypes)
        {
            List<Tile> tiles = GetTilesByTerrainType(terrainTypes);

            distance = 0;
            Tile closestTile = null;

            if (tiles.Count == 0)
            {
                Debug.WriteLine("No tiles of supplied types exist");
                return null;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                float d = TaxicabDistance(tiles[i].Centroid, startPoint);
                if (i == 0 || d < distance)
                {
                    distance = d;
                    closestTile = tiles[i];
                }
            }

            return closestTile;
        }*/

        /// <summary>
        /// Calculate the taxicab distance between two vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float TaxicabDistance(Vector2 a, Vector2 b)
        {
            float da = (a.X - b.X);
            float db = (a.Y - b.Y);
            if (da < 0)
            {
                da = -da;
            }
            if (db < 0)
            {
                db = -db;
            }
            return da + db;
        }

        private void CreateLakes()
        {
            // TODO this is slow
            const int iters = 10;
            float lakeBaseChance = 0.2f;

            int index = 0;
            for (int i = 0; i < iters; i++)
            {
                foreach (Tile landTile in GetTilesByTerrainType(Util.TerrainType.Land))
                {
                    // Neighbors not aquatic except for other lakes
                    HashSet<Util.TerrainType> neighborTerrainTypes = landTile.ConnectedTileTerrainTypes();
                    if (!neighborTerrainTypes.ContainsAnyTerrain(Util.TerrainType.Water, Util.TerrainType.Ocean, Util.TerrainType.Coast))
                    {
                        float lakeChance = lakeBaseChance;

                        // More aquatic vertices increases the probability of having a lake
                        lakeChance *= landTile.AquaticPercentage();

                        // Probability of having a lake next to another is increased
                        if (neighborTerrainTypes.ContainsTerrain(Util.TerrainType.Lake))
                        {
                            float distribution = 0.03f;
                            lakeChance = lakeChance * (1 - distribution) + 1.0f * distribution;
                        }

                        if (noise.GetNoise(index, index) < lakeChance)
                        {
                            SetTileTerrain(landTile, Util.TerrainType.Lake);
                            break;
                        }

                        index++;
                    }
                }
            }


        }

        /// <summary>
        /// Assign terrain types to Tiles and Vertices
        /// </summary>
        private void AssignTerrainTypes()
        {
            float dispersion = 0.5f; // 0.0 - 1.0

            // Internal var
            float noiseMultipler = 1.0f / (1.0f - dispersion);

            // Set edge terrain from noise
            foreach (Vertex vertex in terrainPlane.Vertices)
            {
                // TODO why does it need to be flipped?
                Vector2 vertexFlippedY = new Vector2(vertex.Position.X, bounds.Size - vertex.Position.Y);
                Vector2 normalizedCoord = (vertexFlippedY + new Vector2(bounds.X, bounds.Y)) / bounds.Size;
                Vector2 pos = normalizedCoord * noiseMultipler * 128;
                //noise.SetSeed(DateTime.Now.Millisecond);
                float random = noise.GetSimplex(pos.X, pos.Y);
                if (random > 0.1f)
                {
                    vertex.TerrainType = Util.TerrainType.Land;
                }
                else
                {
                    vertex.TerrainType = Util.TerrainType.Water;
                }
            }

            // First pass, set tile terrain type from edge types
            foreach (Tile tile in terrainPlane.Tiles)
            {
                SetTileTerrain(tile, tile.AquaticPercentage() > 0.5f ? Util.TerrainType.Water : Util.TerrainType.Land);
            }

            // Second pass, classify water tiles
            foreach (Tile tile in terrainPlane.Tiles)
            {
                if (!tile.TerrainType.IsAquatic())
                {
                    continue;
                }

                int aquaticNeighbors = 0;

                foreach (Tile neighbor in tile.Neighbors)
                {
                    if (neighbor.TerrainType.IsAquatic())
                    {
                        aquaticNeighbors++;
                    }
                }

                // If all neighbors are aquatic
                if (aquaticNeighbors == tile.Neighbors.Count())
                {
                    SetTileTerrain(tile, Util.TerrainType.Ocean);
                }
                else
                {
                    SetTileTerrain(tile, Util.TerrainType.Coast);
                }

            }

            Debug.Assert(tilesIndexedByTerrain[Util.TerrainType.Coast].Count != 0);

        }

        /// <summary>
        /// Helper method to set tile terrain and update the indexed list of tiles at the same time
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="terrainType"></param>
        void SetTileTerrain(Tile tile, Util.TerrainType terrainType)
        {
            // Delete old entry
            tilesIndexedByTerrain[tile.TerrainType].Remove(tile);

            // Add new entry
            tile.TerrainType = terrainType;
            tilesIndexedByTerrain[terrainType].Add(tile);
        }

        /// <summary>
        /// Returns a list of tiles by terrain type
        /// </summary>
        /// <param name="terrainType"></param>
        /// <returns></returns>
        List<Tile> GetTilesByTerrainType(params Util.TerrainType[] terrainType)
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Util.TerrainType type in terrainType)
            {
                tiles.AddRange(tilesIndexedByTerrain[type]);
            }
            return tiles;
        }

        /// <summary>
        /// Create a texture with vertex moisture and elevation data
        /// </summary>
        public Texture2D CreateVertexMaps()
        {
            SVertex[] vertexStructs = new SVertex[terrainPlane.Vertices.Length];
            STile[] tileStructs = new STile[terrainPlane.Tiles.Length];

            int i = 0;
            foreach (Vertex vertex in terrainPlane.Vertices)
            {
                vertexStructs[i] = vertex.AsStruct(bounds);
                i++;
            }

            i = 0;
            foreach (Tile tile in terrainPlane.Tiles)
            {
                tileStructs[i] = tile.AsStruct(bounds);
                i++;
            }

            // Set data
            ComputeBuffer vertexBuffer = new ComputeBuffer(vertexStructs.Length, SVertex.Bytes());
            vertexBuffer.SetData(vertexStructs);
            ComputeBuffer tileBuffer = new ComputeBuffer(tileStructs.Length, STile.Bytes());
            tileBuffer.SetData(tileStructs);

            // Render
            ComputeShader computer = new ComputeShader(EconSim.d3dDevice, "Shaders/terrainGeneration.hlsl", "ComputeTerrain");

            // Input texture
            Texture2D computeResource = new Texture2D(EconSim.d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default //?
            });

            computer.SetTexture(computeResource, 0);
            computer.SetComputeBuffer(vertexBuffer, 1);
            computer.SetComputeBuffer(tileBuffer, 2);

            computer.Begin();
            computer.Dispatch(32, 32, 1);

            // Get output
            Texture2D texture = computer.GetTexture(0);

            computer.End();

            return texture;
        }
    }
}