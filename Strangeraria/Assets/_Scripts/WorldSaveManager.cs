using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WorldSaveManager
{
    private static string saveFolder;
    private static int worldSeed;
    public static WorldSaveData CurrentWorld { get; private set; }

    public static void Init(int seed)
    {
        worldSeed = seed;
        saveFolder = Path.Combine(Application.persistentDataPath, "World_" + seed);
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        CurrentWorld = new WorldSaveData { seed = seed, chunks = new Dictionary<Vector2Int, ChunkData>() };
    }

    #region World & Chunk Data

    [Serializable]
    public class WorldSaveData
    {
        public int seed;
        public Dictionary<Vector2Int, ChunkData> chunks = new();
    }

    [Serializable]
    public class ChunkData
    {
        public Vector2Int coord;
        public Dictionary<Vector3Int, string> modifiedTiles = new();
        public HashSet<int> destroyedFloraSeeds = new(); // ← ajoute cette ligne
    }


    #endregion

    #region Chunk Save/Load

    private static string GetChunkPath(Vector2Int coord)
    {
        return Path.Combine(saveFolder, $"chunk_{coord.x}_{coord.y}.json");
    }

    public static void SaveChunk(Vector2Int coord, ChunkData data)
    {
        // Assure que CurrentWorld contient ce chunk
        CurrentWorld.chunks[coord] = data;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetChunkPath(coord), json);
    }

    public static ChunkData LoadChunk(Vector2Int coord)
    {
        if (CurrentWorld.chunks.TryGetValue(coord, out ChunkData chunk))
            return chunk;

        string path = GetChunkPath(coord);
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        ChunkData loaded = JsonUtility.FromJson<ChunkData>(json);
        CurrentWorld.chunks[coord] = loaded;
        return loaded;
    }

    #endregion
}
