using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldParameters", menuName = "World/Biomes")]
public class WorldParameters : ScriptableObject
{
    [Header("Environment")]
    [Range(0f, 1f)] public float fogDensity = 0.02f;

    [Header("Spawning")]
    public int maxEnemies = 20;
    public float spawnRatePerMinute = 6f;

    [Header("Bloc")]
    public List<TileandHeight> Worldlayers;

    [Header("Cave")]
    public int CaveDeep = 10;
    [Range(0f, 1f)] public float CaveHeight;

    [Header("Relief")]
    [Range(0, 0.5f)] public float Frequency = 0.1f;
    [Range(0, 1)] public float Persistance = 0.1f;
    [Range(0, 3)] public int Octaves = 1;
    public int Amplitude = 1;
    public float Lacunarity = 1f;
    public FastNoiseLite.NoiseType type;
    public List<OreDefinition> possibleOres = new List<OreDefinition>();

    [Header("Flore")]
    public List<FloreDefinition> possibleFlores = new List<FloreDefinition>();

    [Header("Paralax")]
    public List<Sprite> ParalaxImages;
    public float sizeX;
    public Color TopColor;
    public Color BottomColor;

    private FastNoiseLite HeighMax;

    void OnEnable()
    {
        int seed;
        if (Game_Manager.Instance)
            seed = Game_Manager.Instance.Seed;
        else seed = 1234;
        HeighMax = new FastNoiseLite(seed);
        HeighMax.SetNoiseType(type);
        HeighMax.SetFrequency(Frequency);
        HeighMax.SetFractalGain(Persistance);
        HeighMax.SetFractalOctaves(Octaves);
        HeighMax.SetFractalLacunarity(Lacunarity);
    }

    public FastNoiseLite GetBiomeNoise()
    {
        return HeighMax;
    }
}

[System.Serializable]
public class TileandHeight
{
    public int height; 
    public TileBase tile;
}

[System.Serializable]
public class OreDefinition
{
    public string name;
    public TileBase oreTile;          // La tuile du minerai
    public int minVeinSize = 2;       // Nombre min de minerais par filon
    public int maxVeinSize = 6;       // Nombre max
    public float spawnChance = 0.01f; // Probabilité par bloc
    public float minSpawnHeight = -100; // Hauteur minimum où le minerai peut apparaître
    public float maxSpawnHeight = 0;  // Hauteur max (facultatif)
}

[System.Serializable]
public class FloreDefinition
{
    public float spawnChance = 0.01f;
    public GameObject floreobject;
    public int uniqueID;
}