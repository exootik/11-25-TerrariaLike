using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; }

    [Header("References")]
    public Camera MainCamera;
    public GameObject Player;
    public GameObject Pointer;
    public InfiniteWorld World;
    public TMP_Text Debug_World;
    public TMP_InputField SeedInputField;
    VTools.RandomService.RandomService random;
    public int Seed;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void RegisterWorld(InfiniteWorld world)
    {
        World = world;
        Debug.Log("World registered: " + world.name);
    }
    public void RegisterDebugText(TMP_Text text)
    {
        Debug_World = text;
        Debug.Log("DebugText registered:");
    }
    public void RegisterPlayer(GameObject player)
    {
        Player = player;
        Debug.Log("Player registered: " + player.name);
    }

    public void LoadScene(string sceneName)
    {
        Seed = ReadInput();
        Debug.Log("Seed utilisé : " + Seed);

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene '" + sceneName + "' not found or not added to build settings.");
        }
    }

    public int ReadInput()
    {
        string userText = SeedInputField.text;

        if (int.TryParse(userText, out int result))
        {
            return result;
        }
        else if (!string.IsNullOrEmpty(userText))
        {
            int hash = userText.GetHashCode();
            Debug.LogWarning("Texte non numérique. Seed basé sur hash : " + hash);
            return Mathf.Abs(hash); // pour rester dans la plage
        }
        else
        {
            Debug.LogWarning("Champ vide. Seed aléatoire utilisé.");
            return Random.Range(0, 20000);
        }
    }

    private bool debugVisible = true;

    private void LateUpdate()
    {
        if (Debug_World != null)
        {
            if(debugVisible == true)
            {
                Debug_World.text = "Seed : " + World.worldSeed + "\n"
                         + "Player : " + Player.transform.position.x + "," + Player.transform.position.y + "\n";
                         //+ "Biomes : " + World.GetBiomeName(Player.transform.position.x) + "\n";
            }

            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                debugVisible = !debugVisible;
                Debug_World.gameObject.SetActive(debugVisible);
            }
        }
        
    }
}