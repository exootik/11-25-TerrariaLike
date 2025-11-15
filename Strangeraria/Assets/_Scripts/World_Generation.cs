//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using VTools.RandomService;

public class InfiniteWorld : MonoBehaviour
{
    [Header("Player Parameters")]
    public Transform player;
    public int viewDistanceInChunks = 3;
    public int chunkSize = 32;

    [Header("World Parameters")]
    public int WorldDeep = 512;
    public int WorldFloor = 0;
    public Tilemap chunkTilemapPrefab;
    public Tilemap chunkBackgroundTilemapPrefab;
    public int worldSeed = 1234;

    [Header("Physics")]
    public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    public float delayBetweenTiles = 0.1f;

    [Header("Time")]
    [Tooltip("Durée d'un cycle jour/nuit en secondes")]
    public float DayDuration = 1200f;
    [Range(0, 1)] public float SunPercentagevsMoonPercentage;

    [Header("Mineral Noise")]
    [Range(0, 0.5f)] public float Frequency = 0.1f;
    [Range(0, 3)] public int Octaves = 1;
    public float lacunarity = 1;
    public FastNoiseLite.NoiseType noiseType;
    public TileBase caveTile;
    public TileBase BackgroundTile;

    [Header("ENNEMIS")]
    public List<GameObject> Ennemis;
    public int spawnTimermin;
    public int spawnTimermax;
    private Coroutine spawnRoutine;
    private int difficultyGame = 1;

    [System.Serializable]
    public class BiomeDefinition
    {
        public string name;
        public WorldParameters parameters;
        public Vector2Int BiomeLength;
    }

    [Header("Biomes disponibles")]
    public List<BiomeDefinition> possibleBiomes = new List<BiomeDefinition>();

    private class BiomeSegment
    {
        public float startX;
        public float endX;
        public WorldParameters biome;
    }

    private List<BiomeSegment> biomeSegments = new List<BiomeSegment>();
    private Dictionary<Vector2Int, Tilemap> chunks = new Dictionary<Vector2Int, Tilemap>();
    private Dictionary<Vector2Int, Tilemap> chunksBackground = new Dictionary<Vector2Int, Tilemap>();
    private Dictionary<Vector2Int, List<GameObject>> chunkFloras = new Dictionary<Vector2Int, List<GameObject>>();
    private Dictionary<int, List<GameObject>> globalFlorasByX = new Dictionary<int, List<GameObject>>();

    private Vector2Int lastPlayerChunk;
    private WorldSaveManager.WorldSaveData worldSave;

    void Start()
    {
        if (Game_Manager.Instance != null)
            worldSeed = Game_Manager.Instance.Seed;

        WorldSaveManager.Init(worldSeed);
        worldSave = WorldSaveManager.CurrentWorld;

        lastPlayerChunk = GetChunkCoordFromPosition(player.position);

        GenerateBiomeSegments(-500000, 500000);
        GenerateChunksAroundPlayer();

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.RegisterWorld(this);
            Game_Manager.Instance.Player.transform.position = new Vector3(1, GetFloorfromX(1) + 1, 0);
        }
        else
        {
            Debug.LogWarning("GameManager not found when trying to register Player.");
        }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        Vector2Int playerChunk = GetChunkCoordFromPosition(player.position);
        if (playerChunk != lastPlayerChunk)
        {
            lastPlayerChunk = playerChunk;
            GenerateChunksAroundPlayer();
        }
    }
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnTimermin, spawnTimermax);
            yield return new WaitForSeconds(wait);

            float leftOrRight = (Random.value < 0.5f) ? -1f : 1f;

            if (player == null || Ennemis == null) continue;

            Vector3 spawnPos;
            if (leftOrRight == -1f)
            {
                float spawnPosX = player.position.x + 10;
                float spawnPosY = GetFloorfromX(player.position.x + 10);
                spawnPos = new Vector3(spawnPosX, spawnPosY);
            }
            else
            {
                float spawnPosX = player.position.x - 10;
                float spawnPosY = GetFloorfromX(player.position.x + 10);
                spawnPos = new Vector3(spawnPosX, spawnPosY);
            }

            int enemi = Random.Range(0, Ennemis.Count);
            Instantiate(Ennemis[enemi], spawnPos, Quaternion.identity);
        }
    }

    public void IncreaseDifficultyOfMap()
    {
        difficultyGame++;

        if (difficultyGame == 2)
        {
            spawnTimermin = 10;
            spawnTimermax = 20;
        }
        if (difficultyGame == 3)
        {
            spawnTimermin = 5;
            spawnTimermax = 15;
        }
        if (difficultyGame == 4)
        {
            spawnTimermin = 3;
            spawnTimermax = 6;
        }
        if (difficultyGame >= 5)
        {
            spawnTimermin = 1;
            spawnTimermax = 2;
        }
    }

    private void GenerateBiomeSegments(float worldStartX, float worldEndX)
    {
        biomeSegments.Clear();
        float currentX = worldStartX;

        System.Random rng = new System.Random(worldSeed);

        while (currentX < worldEndX)
        {
            BiomeDefinition biomeDef = possibleBiomes[rng.Next(possibleBiomes.Count)];

            float length = rng.Next(biomeDef.BiomeLength.x, biomeDef.BiomeLength.y + 1);

            BiomeSegment segment = new BiomeSegment
            {
                startX = currentX,
                endX = currentX + length,
                biome = biomeDef.parameters
            };

            biomeSegments.Add(segment);
            currentX += length;
        }
    }

    public WorldParameters GetBiomeFromX(float worldX)
    {
        foreach (var segment in biomeSegments)
        {
            if (worldX >= segment.startX && worldX < segment.endX)
                return segment.biome;
        }

        return possibleBiomes.Count > 0 ? possibleBiomes[0].parameters : null;
    }

    public string GetBiomeName(float worldX)
    {
        foreach (var segment in biomeSegments)
        {
            if (worldX >= segment.startX && worldX < segment.endX)
                return segment.biome.name;
        }

        return possibleBiomes.Count > 0 ? possibleBiomes[0].name : null;
    }

    Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int y = Mathf.FloorToInt(position.y / chunkSize);
        return new Vector2Int(x, y);
    }

    Vector3Int GetLocalTilePosition(Vector3 worldPos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        Vector3Int localPos = new Vector3Int(
            Mathf.FloorToInt(worldPos.x) - chunkCoord.x * chunkSize,
            Mathf.FloorToInt(worldPos.y) - chunkCoord.y * chunkSize,
            0
        );
        return localPos;
    }

    void GenerateChunksAroundPlayer()
    {
        int horizontalRadius = viewDistanceInChunks;
        int verticalRadius = viewDistanceInChunks * 2; // + large verticalement pour stabiliser les arbres
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        // --- Calcul des chunks nécessaires autour du joueur ---
        for (int x = -horizontalRadius; x <= horizontalRadius; x++)
        {
            for (int y = -verticalRadius; y <= verticalRadius; y++)
            {
                Vector2Int chunkCoord = lastPlayerChunk + new Vector2Int(x, y);
                neededChunks.Add(chunkCoord);

                // Génération du chunk si nécessaire
                if (!chunks.ContainsKey(chunkCoord))
                {
                    Tilemap tilemap = GenerateChunk(chunkCoord);

                    var savedData = WorldSaveManager.LoadChunk(chunkCoord);
                    if (savedData != null)
                    {
                        foreach (var kvp in savedData.modifiedTiles)
                        {
                            Vector3Int localPos = kvp.Key;
                            string tileName = kvp.Value;
                            TileBase tile = tileName != "null" ? FindTileByName(tileName) : null;
                            tilemap.SetTile(localPos, tile);
                        }
                    }
                }
            }
        }

        // --- Suppression des chunks trop éloignés ---
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;

            if (!neededChunks.Contains(coord))
            {
                // Supprime le chunk principal
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);

                // Supprime l’arrière-plan
                if (chunksBackground.TryGetValue(coord, out var background))
                    if (background != null)
                        Destroy(background.gameObject);

                // Supprime la flore associée
                DestroyAllFloraInChunk(coord);

                chunksToRemove.Add(coord);
            }
        }

        // Nettoyage des dictionnaires
        foreach (var coord in chunksToRemove)
        {
            chunks.Remove(coord);
            chunksBackground.Remove(coord);
        }
    }

    Tilemap GenerateChunk(Vector2Int chunkCoord)
    {
        Tilemap tilemap = Instantiate(chunkTilemapPrefab, transform);
        Tilemap tilemapBackground = Instantiate(chunkBackgroundTilemapPrefab, transform);

        tilemap.transform.position = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0);
        tilemapBackground.transform.position = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 1);

        chunks[chunkCoord] = tilemap;
        chunksBackground[chunkCoord] = tilemapBackground;

        FastNoiseLite caveNoise = new FastNoiseLite(worldSeed);
        caveNoise.SetNoiseType(noiseType);
        caveNoise.SetFrequency(Frequency);
        caveNoise.SetFractalOctaves(Octaves);
        caveNoise.SetFractalLacunarity(lacunarity);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float worldX = chunkCoord.x * chunkSize + x;
                float worldY = chunkCoord.y * chunkSize + y;
                WorldParameters biome = GetBiomeFromX(worldX);
                if (biome == null) continue;

                float rawNoise = biome.GetBiomeNoise().GetNoise(worldX, 0);
                float caveValue = caveNoise.GetNoise(worldX, worldY);
                TileBase tile = GetTileForHeight(rawNoise, worldY, biome, caveTile);

                float normalizedValue = (rawNoise + 1f) * 0.5f;
                float surfaceHeight = WorldFloor + normalizedValue * biome.Amplitude * 2f;

                if (worldY > surfaceHeight || worldY < -WorldDeep) continue;
                tilemapBackground.SetTile(new Vector3Int(x, y, 0), BackgroundTile);
                if (tile == caveTile && caveValue > biome.CaveHeight) continue;
                if (tile != null) tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        GenerateOresInChunk(chunkCoord, tilemap);
        GenerateFloreOnChunk(chunkCoord, tilemap);

        return tilemap;
    }

    TileBase GetTileForHeight(float value, float worldY, WorldParameters biome, TileBase caveTile)
    {
        float normalizedValue = (value + 1f) * 0.5f;
        float surfaceTop = WorldFloor + normalizedValue * biome.Amplitude * 2f;
        float currentHeight = surfaceTop;

        foreach (var layer in biome.Worldlayers)
        {
            float nextLimit = currentHeight - layer.height;
            if (worldY <= currentHeight && worldY > nextLimit)
            {
                return layer.tile;
            }
            currentHeight = nextLimit;
        }

        return caveTile;
    }

    void GenerateFloreOnChunk(Vector2Int chunkCoord, Tilemap tilemap)
    {
        if (!chunkFloras.ContainsKey(chunkCoord))
            chunkFloras[chunkCoord] = new List<GameObject>();

        var savedData = WorldSaveManager.LoadChunk(chunkCoord)
                        ?? new WorldSaveManager.ChunkData { coord = chunkCoord };

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = chunkCoord.x * chunkSize + x;

            // Si des arbres existent déjà, réactive-les
            if (globalFlorasByX.TryGetValue(worldX, out var existingFloras))
            {
                foreach (var flora in existingFloras)
                    if (flora != null)
                        flora.SetActive(true);

                // Ajoute seulement ceux qui ne sont pas déjà dans chunkFloras
                chunkFloras[chunkCoord].AddRange(existingFloras
                    .Where(f => !chunkFloras[chunkCoord].Contains(f)));

                continue;
            }

            WorldParameters biome = GetBiomeFromX(worldX);
            if (biome == null || biome.possibleFlores.Count == 0)
                continue;

            float surfaceY = GetFloorfromX(worldX);
            List<GameObject> spawnedFlorasAtX = new List<GameObject>();

            foreach (var floraDef in biome.possibleFlores)
            {
                // Seed stable basé sur uniqueID
                int floraSeed = worldSeed + worldX * 1000 + floraDef.uniqueID;

                if (savedData.destroyedFloraSeeds.Contains(floraSeed))
                    continue;

                System.Random localRng = new System.Random(floraSeed);
                if (localRng.NextDouble() < floraDef.spawnChance)
                {
                    Vector3 spawnPos = new Vector3(worldX + 0.5f, surfaceY + 1f, 0f);
                    GameObject flora = Instantiate(floraDef.floreobject, spawnPos, Quaternion.identity, this.transform);
                    flora.name = $"{floraDef.floreobject.name}_{worldX}";

                    if (flora.TryGetComponent<ISeededFlora>(out var seeded))
                        seeded.ApplySeed(floraSeed);

                    spawnedFlorasAtX.Add(flora);
                }
            }

            if (spawnedFlorasAtX.Count > 0)
                globalFlorasByX[worldX] = spawnedFlorasAtX;

            chunkFloras[chunkCoord].AddRange(spawnedFlorasAtX);
        }
    }

    void DestroyAllFloraInChunk(Vector2Int chunkCoord)
    {
        if (!chunkFloras.ContainsKey(chunkCoord))
            return;

        foreach (var flora in chunkFloras[chunkCoord])
        {
            if (flora != null)
            {
                float distance = Vector3.Distance(flora.transform.position, player.position);
                if (distance > chunkSize * (viewDistanceInChunks*1.3)) // ou un rayon personnalisé
                    flora.SetActive(false);
                else
                    flora.SetActive(true);
            }
        }
    }


    public void MarkFloraDestroyed(Vector3 worldPos, int floraSeed)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        var data = WorldSaveManager.LoadChunk(chunkCoord) ?? new WorldSaveManager.ChunkData { coord = chunkCoord };

        data.destroyedFloraSeeds.Add(floraSeed);
        WorldSaveManager.SaveChunk(chunkCoord, data);

        // Supprime de la liste en mémoire
        if (chunkFloras.ContainsKey(chunkCoord))
            chunkFloras[chunkCoord].RemoveAll(f => f == null || f.transform.position == worldPos);

        // Supprime aussi du dictionnaire global pour ne pas réapparaître
        int worldX = Mathf.FloorToInt(worldPos.x);
        if (globalFlorasByX.ContainsKey(worldX))
        {
            globalFlorasByX.Remove(worldX);
        }
    }

    public void DestroyFlora(Vector3 worldPos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        var data = WorldSaveManager.LoadChunk(chunkCoord) ?? new WorldSaveManager.ChunkData { coord = chunkCoord };

        int worldX = Mathf.FloorToInt(worldPos.x);
        WorldParameters biome = GetBiomeFromX(worldX);
        if (biome == null) return;

        foreach (var floraDef in biome.possibleFlores)
        {
            int floraSeed = worldSeed + worldX * 1000 + floraDef.uniqueID;
            data.destroyedFloraSeeds.Add(floraSeed);

            // Désactive l'arbre en mémoire
            if (globalFlorasByX.TryGetValue(worldX, out var florasAtX))
            {
                foreach (var flora in florasAtX)
                    if (flora != null)
                        flora.SetActive(false);
            }

            if (chunkFloras.ContainsKey(chunkCoord))
                chunkFloras[chunkCoord].RemoveAll(f => f == null || Mathf.FloorToInt(f.transform.position.x) == worldX);
        }

        WorldSaveManager.SaveChunk(chunkCoord, data);
    }


    public float GetFloorfromX(float X)
    {
        WorldParameters biome = GetBiomeFromX(X);
        if (biome != null)
        {
            float rawNoise = biome.GetBiomeNoise().GetNoise(X, 0);
            float normalizedValue = (rawNoise + 1f) * 0.5f;
            float surfaceHeight = WorldFloor + normalizedValue * biome.Amplitude * 2f;
            surfaceHeight = Mathf.FloorToInt(surfaceHeight);
            return surfaceHeight + 0.5f;
        }
        return 0f;
    }

    private TileBase FindTileByName(string name)
    {
        foreach (var biomeDef in possibleBiomes)
        {
            foreach (var layer in biomeDef.parameters.Worldlayers)
            {
                if (layer.tile != null && layer.tile.name == name)
                    return layer.tile;
            }

            foreach (var ore in biomeDef.parameters.possibleOres)
            {
                if (ore.oreTile != null && ore.oreTile.name == name)
                    return ore.oreTile;
            }
        }

        if (caveTile != null && caveTile.name == name) return caveTile;
        if (BackgroundTile != null && BackgroundTile.name == name) return BackgroundTile;

        return null;
    }

    public Vector3 TilePoseUnderMouse(Vector3 worldPos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        if (!chunks.ContainsKey(chunkCoord))
            return Vector3.zero;

        Tilemap tilemap = chunks[chunkCoord];
        Vector3Int localPos = GetLocalTilePosition(worldPos);

        if (!tilemap.HasTile(localPos))
            return Vector3.zero;

        return tilemap.CellToWorld(localPos) + tilemap.tileAnchor;
    }
    public TileBase GetTileUnderMouse(Vector3 worldPos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);

        if (!chunks.ContainsKey(chunkCoord))
            return null;

        Tilemap tilemap = chunks[chunkCoord];
        Vector3Int localPos = GetLocalTilePosition(worldPos);

        return tilemap.GetTile(localPos);
    }

    public bool HasTileAtPosition(Vector3 worldPos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);

        if (!chunks.ContainsKey(chunkCoord))
            return false;

        Tilemap tilemap = chunks[chunkCoord];
        Vector3Int localPos = GetLocalTilePosition(worldPos);

        return tilemap.HasTile(localPos);
    }

    public void MineTile(Vector3 worldPos, BreakableTile.ToolType toolUsed)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        if (!chunks.ContainsKey(chunkCoord)) return;

        Tilemap tilemap = chunks[chunkCoord];
        Vector3Int localPos = GetLocalTilePosition(worldPos);
        TileBase tile = tilemap.GetTile(localPos);

        if (tile is BreakableTile breakableTile && breakableTile.isBreakable)
        {
            if (breakableTile.requiredTool != BreakableTile.ToolType.None &&
                breakableTile.requiredTool != toolUsed)
            {
                Debug.Log("Outil incorrect pour miner cette tile.");
                return;
            }

            tilemap.SetTile(localPos, null);

            var data = WorldSaveManager.LoadChunk(chunkCoord) ?? new WorldSaveManager.ChunkData { coord = chunkCoord };
            data.modifiedTiles[localPos] = "null";
            WorldSaveManager.SaveChunk(chunkCoord, data);
        }
    }

    public bool BuildTile(Vector3 worldPos, TileBase tileToBuild)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(worldPos);
        if (!chunks.ContainsKey(chunkCoord)) return false;

        Tilemap tilemap = chunks[chunkCoord];
        Vector3Int localPos = GetLocalTilePosition(worldPos);

        if (tilemap.GetTile(localPos) != null)
        {
            return false;
        }

        //for (int x = -1; x <= 1; x++)
        //{
        //    for (int y = -1; y <= 1; y++)
        //    {
        //        if (x == 0 && y == 0)
        //            continue;

        //        Vector3Int checkPos = new Vector3Int(localPos.x + x, localPos.y + y, localPos.z);
        //        Vector2Int neighborChunkCoord = chunkCoord;
        //        Vector3Int neighborLocalPos = checkPos;

        //        // Vérifie si la position sort du chunk actuel
        //        if (checkPos.x < 0)
        //        {
        //            neighborChunkCoord.x -= 1;
        //            neighborLocalPos.x += chunkSize;
        //        }
        //        else if (checkPos.x >= chunkSize)
        //        {
        //            neighborChunkCoord.x += 1;
        //            neighborLocalPos.x -= chunkSize;
        //        }

        //        if (checkPos.y < 0)
        //        {
        //            neighborChunkCoord.y -= 1;
        //            neighborLocalPos.y += chunkSize;
        //        }
        //        else if (checkPos.y >= chunkSize)
        //        {
        //            neighborChunkCoord.y += 1;
        //            neighborLocalPos.y -= chunkSize;
        //        }

        //        // Récupère le chunk voisin
        //        Tilemap neighborTilemap =GetWorldPositionTilemap(neighborLocalPos);
        //        if (neighborTilemap == null)
        //            continue;

        //        TileBase neighborTile = neighborTilemap.GetTile(neighborLocalPos);
        //        if (neighborTile != null)
        //        {
        //            var data = WorldSaveManager.LoadChunk(chunkCoord) ?? new WorldSaveManager.ChunkData { coord = chunkCoord };
        //            data.modifiedTiles[localPos] = tileToBuild != null ? tileToBuild.name : "null";
        //            WorldSaveManager.SaveChunk(chunkCoord, data);

        //            tilemap.SetTile(localPos, tileToBuild);
        //            return true;
        //        }
        //    }
        //}

        var data = WorldSaveManager.LoadChunk(chunkCoord) ?? new WorldSaveManager.ChunkData { coord = chunkCoord };
        data.modifiedTiles[localPos] = tileToBuild != null ? tileToBuild.name : "null";
        WorldSaveManager.SaveChunk(chunkCoord, data);

        tilemap.SetTile(localPos, tileToBuild);
        return true;
    }

    public Tilemap GetWorldPositionTilemap(Vector3 pos)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(pos);
        Tilemap tilemap = chunks[chunkCoord];
        return tilemap;
    }
    void GenerateOresInChunk(Vector2Int chunkCoord, Tilemap tilemap)
    {
        System.Random rng = new System.Random(worldSeed + chunkCoord.x * 73856093 + chunkCoord.y * 19349663);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float worldX = chunkCoord.x * chunkSize + x;
                float worldY = chunkCoord.y * chunkSize + y;

                WorldParameters biome = GetBiomeFromX(worldX);
                if (biome == null || biome.possibleOres.Count == 0)
                    continue;

                foreach (var ore in biome.possibleOres)
                {
                    if (worldY > ore.maxSpawnHeight || worldY < ore.minSpawnHeight)
                        continue;

                    if (rng.NextDouble() < ore.spawnChance)
                    {
                        int veinSize = rng.Next(ore.minVeinSize, ore.maxVeinSize + 1);
                        CreateOreVein(tilemap, new Vector3Int(x, y, 0), ore, veinSize, rng);
                    }
                }
            }
        }
    }
    void CreateOreVein(Tilemap tilemap, Vector3Int start, OreDefinition ore, int veinSize, System.Random rng)
    {
        Vector3Int current = start;

        for (int i = 0; i < veinSize; i++)
        {
            if (!tilemap.HasTile(current)) continue;

            tilemap.SetTile(current, ore.oreTile);

            current += new Vector3Int(
                rng.Next(-1, 2),
                rng.Next(-1, 2),
                0
            );
        }
    }
}
