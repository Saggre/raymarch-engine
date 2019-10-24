// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using EconSim.Geometry;
using EconSim.Math;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = System.Drawing.Color;
using ComputeShader = EconSim.Math.ComputeShader;
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
      stopwatch = new Stopwatch();
      stopwatch.Start();

      tilesIndexedByTerrain = new Dictionary<Util.TerrainType, List<Tile>>();
      Util.ForEachTerrainType(delegate (Util.TerrainType terrainType)
      {
        tilesIndexedByTerrain.Add(terrainType, new List<Tile>());
      });

      //Voronoi voronoi = CreateVoronoi(area);
      terrainPlane = new TerrainPlane(bounds.Size);
      ProfileTime("Voronoi creation");

      AssignTerrainTypes();
      ProfileTime("Assigning terrain types");

      CreateLakes();
      ProfileTime("Creating lakes");

      AssignElevationsAndMoisture();
      ProfileTime("Assigning elevations");

      //AssignBiomes(voronoi);
      //ProfileTime("Creating biomes");

      //Dictionary<Util.TerrainType, Texture2D> terrainMaps = GetTerrainMaps();
      ProfileTime("Rendering terrain textures");

      //terrainMaps[Util.TerrainType.Coast].SaveRenderTextureToFile("Coast");

      /*_vertexComputeShaderIo = new ComputeShaderIO(vertexComputeShader, 1024);
      RenderTexture vertexMaps = CreateVertexMaps(voronoi);
      ProfileTime("Creating vertex maps");

      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube.transform.localScale = new Vector3(10, 0.01f, 10);
      cube.transform.position = new Vector3(0, -0.5f, 0);
      cube.GetComponent<Renderer>().material.mainTexture = vertexMaps;
      cube.SetActive(false);

      DisplayTerrain(voronoi, terrainMaps, vertexMaps);*/

    }

    /*void DisplayTerrain(Dictionary<Util.TerrainType, Texture2D> terrainMaps, Texture2D vertexMaps)
    {
      Material terrainMaterial = Resources.Load<Material>("Materials/Terrain");

      if (terrainMaterial == null)
      {
        Debug.LogError("Could not find material");
      }

      // Build quad
      GameObject terrainObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
      terrainObject.transform.LookAt(Vector3.up * -1);
      terrainObject.transform.position = new Vector3(voronoi.Bounds.x, 0, voronoi.Bounds.y);
      terrainObject.transform.localScale = new Vector3(voronoi.Bounds.width, voronoi.Bounds.width, voronoi.Bounds.width);

      terrainObject.GetComponent<Renderer>().sharedMaterial = terrainMaterial;

      //terrainMaterial.SetTexture("_Land", terrainMaps[Util.TerrainType.Land]);
      terrainObject.GetComponent<Renderer>().material.SetTexture("_VertexMaps", vertexMaps);
    }*/

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
    /// Assign elevations and moisture to Tiles and Vertices
    /// </summary>
    private void AssignElevationsAndMoisture()
    {
      float coastFreshWaterPercentage = 0.35f;
      float lakeFreshWaterPercentage = 0.9f;

      // Set vertex elevations as distance from coast
      // Set moisture as distance from fresh water
      foreach (var vertex in terrainPlane.Vertices)
      {
        List<Util.TerrainType> connectedTypes = vertex.ConnectedTileTerrainTypes();

        if (GetClosestTileOfType(vertex.Position, out var distanceFromCoastline, Util.TerrainType.Coast) == null)
        {
          // Default value
          distanceFromCoastline = bounds.Size;
        }

        if (GetClosestTileOfType(vertex.Position, out var distanceFromLake, Util.TerrainType.Lake) == null)
        {
          // Default value
          distanceFromLake = bounds.Size;
        }

        // TODO is this necessary?
        // Normalize values
        distanceFromLake *= 512.0f / bounds.Size;
        distanceFromCoastline *= 512.0f / bounds.Size;

        var distanceFromFreshwater = Math.Min(distanceFromLake * (1 - lakeFreshWaterPercentage), distanceFromCoastline * (1 - coastFreshWaterPercentage));

        vertex.Moisture = (float)Math.Pow(0.92f, distanceFromFreshwater);

        // 0 is at sea level
        if (connectedTypes.Contains(Util.TerrainType.Ocean))
        {
          vertex.Elevation = -1 + (float)Math.Pow(0.99f, distanceFromCoastline);
        }
        else
        {
          vertex.Elevation = 1 - (float)Math.Pow(0.99f, distanceFromCoastline);
        }
      }

    }

    /// <summary>
    /// Assign biomes to tiles based on connected vertices moisture and elevation
    /// </summary>
    private void AssignBiomes()
    {
      foreach (var tile in terrainPlane.Tiles)
      {
        if (tile.TerrainType.IsAquatic())
        {
          continue;
        }

        tile.BiomeType = Util.GetBiomeType(tile.GetMoisture(), tile.GetElevation());
      }
    }

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

      for (var i = 0; i < tiles.Count; i++)
      {
        float d = Vector2.Distance(tiles[i].Centroid, startPoint);
        if (i == 0 || d < distance)
        {
          distance = d;
          closestTile = tiles[i];
        }
      }

      return closestTile;
    }

    private void CreateLakes()
    {
      // TODO this is slow
      const int iters = 10;
      var lakeBaseChance = 0.2f;

      int index = 0;
      for (int i = 0; i < iters; i++)
      {
        foreach (var landTile in GetTilesByTerrainType(Util.TerrainType.Land))
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
    /// <param name="voronoi"></param>
    private void AssignTerrainTypes()
    {
      float dispersion = 0.5f; // 0.0 - 1.0


      // Internal var
      float noiseMultipler = 1.0f / (1.0f - dispersion);

      // Set edge terrain from noise
      foreach (var vertex in terrainPlane.Vertices)
      {
        Vector2 normalizedCoord = vertex.Position / bounds.Size;
        //Debug.Log(normalizedCoord.ToString("F5"));
        Vector2 pos = normalizedCoord * noiseMultipler * 128;
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
      foreach (var tile in terrainPlane.Tiles)
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

        foreach (var neighbor in tile.Neighbors)
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
      foreach (var type in terrainType)
      {
        tiles.AddRange(tilesIndexedByTerrain[type]);
      }
      return tiles;
    }

    /// <summary>
    /// Builds a mesh from the supplied tiles, scaled down by _bounds.width and scaled up by scale
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    /*Mesh BuildMeshFromTiles(Voronoi voronoi, IEnumerable<Tile> tiles, float scale = 10f)
    {
      if (voronoi.Bounds.width < Mathf.Epsilon)
      {
        Debug.Log("Invalid bounds size");
      }

      float voronoiScaleMultipler = scale / voronoi.Bounds.width;

      Mesh mesh = new Mesh();

      List<Vector3> vertices = new List<Vector3>();
      List<int> indices = new List<int>();
      List<Color> colors = new List<Color>();
      List<Vector3> normals = new List<Vector3>();

      foreach (var tile in tiles)
      {
        Color tileColor = tile.Terrain.GetTerrainColor();

        int tileBase = vertices.Count;
        vertices.Add(new Vector3(tile.x * voronoiScaleMultipler - scale * 0.5f, 0, tile.y * voronoiScaleMultipler - scale * 0.5f));
        int regionBase = tileBase + 1;

        List<Vertex> region = tile.Region(voronoi.Bounds);

        for (int i = 0; i < region.Count; i++)
        {
          vertices.Add(new Vector3(region[i].x * voronoiScaleMultipler - scale * 0.5f, 0, region[i].y * voronoiScaleMultipler - scale * 0.5f));

          int[] triangleIndices = new int[]
          {
            tileBase,
            regionBase + ((i + 1) % region.Count),
            regionBase + i
          };

          indices.AddRange(triangleIndices);
        }

        for (int i = 0; i < region.Count + 1; i++)
        {
          colors.Add(tileColor);
          normals.Add(Vector3.up);
        }
      }

      mesh.vertices = vertices.ToArray();
      mesh.triangles = indices.ToArray();
      mesh.colors = colors.ToArray();
      mesh.normals = normals.ToArray();
      mesh.name = "Voronoi";

      return mesh;
    }*/

    /// <summary>
    /// Create a texture with vertex moisture and elevation data
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="result"></param>
    public byte[] CreateVertexMaps()
    {
      SVertex[] vertexStructs = new SVertex[terrainPlane.Vertices.Length];
      STile[] tileStructs = new STile[terrainPlane.Tiles.Length];

      var i = 0;
      foreach (var vertex in terrainPlane.Vertices)
      {
        vertexStructs[i] = vertex.AsStruct(bounds);
        i++;
      }

      i = 0;
      foreach (var tile in terrainPlane.Tiles)
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
      ComputeShader computer = new ComputeShader(EconSim.d3dDevice, "Shader/terrainGeneration.hlsl", "ComputeTerrain");

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
        SampleDescription = { Count = 1, Quality = 0 }
      });

      computer.SetTexture(computeResource, 0);
      computer.SetComputeBuffer(vertexBuffer, 1);
      computer.SetComputeBuffer(tileBuffer, 2);

      computer.Begin();
      computer.Dispatch(32, 32, 1);

      // Get output
      byte[] texture = computer.GetTexture(0);

      computer.End();

      return texture;
    }
  }
}