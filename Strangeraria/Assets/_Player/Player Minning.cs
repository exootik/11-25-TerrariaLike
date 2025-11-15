using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMinning : MonoBehaviour
{
    [Header("World Reference")]
    public InfiniteWorld infiniteWorld;
    public TileBase buildTile;
    public GameObject destroyAnimPrefab;
    public Sprite[] destroyFrames;       
    public float mineTime = 1f;

    [Header("Refs inventory")]
    public PlayerEquip playerEquip;
    public InventoryModel inventory;
    public float DistanceMaxToMouse;

    private GameObject currentAnimGO = null;
    private Vector3 currentTilePos;
    private float elapsed = 0f;
    private bool isMining = false;
    private BreakableTile currentBreakableRef = null;

    void Start()
    {
        if (infiniteWorld == null)
            infiniteWorld = Game_Manager.Instance.World;

        if (playerEquip == null)
            playerEquip = Object.FindFirstObjectByType<PlayerEquip>();

        if (inventory == null)
            inventory = Object.FindFirstObjectByType<InventoryModel>();
    }
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;

        if (Mouse.current.leftButton.isPressed)
        {
            TileBase tile = infiniteWorld.GetTileUnderMouse(mouseWorldPos);

            if (tile is BreakableTile breakable)
            {
                Vector3 tilePos = infiniteWorld.TilePoseUnderMouse(mouseWorldPos);

                if (!isMining)
                {
                    StartMining(tilePos, breakable.breakTime, breakable);
                }
                else if (tilePos != currentTilePos)
                {
                    StopMining();
                    StartMining(tilePos, breakable.breakTime, breakable);
                }

                UpdateMining();
            }
            else
            {
                StopMining();
            }
        }
        else
        {
            StopMining();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (IsMouseTooFar()) { return; }
            if (playerEquip != null && playerEquip.currentEquippedItem != null && playerEquip.currentEquippedItem.itemType == ItemType.Block)
            {
                TileBase tileToPlace = playerEquip.currentEquippedItem.placeTile != null ? playerEquip.currentEquippedItem.placeTile : buildTile;
                bool placed = infiniteWorld.BuildTile(mouseWorldPos, tileToPlace);

                if (placed)
                {
                    int equippedIndex = playerEquip.equippedHotbarIndex;
                    if (equippedIndex >= 0 && inventory != null)
                    {
                        bool removed = inventory.RemoveOneAt(equippedIndex);
                        if (removed)
                        {
                            var s = inventory.GetSlot(equippedIndex);
                            if (s.IsEmpty)
                                playerEquip.Unequip();
                        }
                    }
                }
            }
            else
            {
                // PAS DE BLOC A POSER
                Debug.Log("No block equipped to place");
            }
        }
    }

    void StartMining(Vector3 tilePos, float breakTime, BreakableTile breakable)
    {
        if (IsMouseTooFar()) { return; }
        currentTilePos = tilePos;
        elapsed = 0f;
        isMining = true;
        mineTime = breakTime;
        currentBreakableRef = breakable;

        currentAnimGO = Instantiate(destroyAnimPrefab, tilePos + new Vector3(0, 0, -2), Quaternion.identity);
        SpriteRenderer sr = currentAnimGO.GetComponent<SpriteRenderer>();
        sr.sprite = destroyFrames[0]; 
    }

    void UpdateMining()
    {
        if (!isMining || currentAnimGO == null) return;

        elapsed += Time.deltaTime;

        float toolMultiplier = 1f;

        if (playerEquip != null && playerEquip.currentEquippedItem != null)
        {
            var item = playerEquip.currentEquippedItem;

            if (item.itemType == ItemType.Weapon && item.mineSpeedMultiplier > 0f)
            {
                toolMultiplier = item.mineSpeedMultiplier;
            }
        }
        float requiredTime = Mathf.Max(0.0001f, mineTime / toolMultiplier);

        float progress = Mathf.Clamp01(elapsed / requiredTime);

        int frameIndex = Mathf.FloorToInt(progress * destroyFrames.Length);
        frameIndex = Mathf.Clamp(frameIndex, 0, destroyFrames.Length - 1);
        currentAnimGO.GetComponent<SpriteRenderer>().sprite = destroyFrames[frameIndex];

        if (elapsed >= requiredTime)
        {
            infiniteWorld.MineTile(currentTilePos, BreakableTile.ToolType.Axe);

            // SPAWN BLOCK
            if (currentBreakableRef != null && currentBreakableRef.dropPrefab != null)
            {
                Debug.Log("Spawn Block");
                Vector3 spawnPos = currentTilePos;
                GameObject go = Instantiate(currentBreakableRef.dropPrefab, spawnPos, Quaternion.identity);
                var wi = go.GetComponent<WorldItem>();
                if (wi != null)
                {
                    //wi.item = currentBreakableRef.dropPrefab;
                    wi.amount = Mathf.Max(1, currentBreakableRef.dropAmount);
                }
                else
                {
                    Debug.LogError("[playerMinning] currentBreakableRef.dropPrefab missing");
                }
            }

            Destroy(currentAnimGO);
            isMining = false;
            currentBreakableRef = null;
        }
    }

    void StopMining()
    {
        if (currentAnimGO != null)
            Destroy(currentAnimGO);

        isMining = false;
        elapsed = 0f;
        currentTilePos = Vector3.zero;
        currentBreakableRef = null;
    }

    public bool IsMouseTooFar()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        float distance = Vector3.Distance(transform.position, mouseWorldPos);
        return distance > DistanceMaxToMouse;
    }

}
