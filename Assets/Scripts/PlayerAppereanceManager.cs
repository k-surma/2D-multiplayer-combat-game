using Unity.Netcode;
using UnityEngine;

public class PlayerAppearanceManager : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // For 2D games
    [SerializeField] private Sprite[] sprites;             // Array of sprites for different appearances

    private NetworkVariable<int> appearanceIndex = new NetworkVariable<int>(0);

    private void Start()
    {
        if (IsOwner)
        {
            // Assign appearance based on whether this is the host or client
            int index = NetworkManager.Singleton.IsHost ? 0 : 1;
            Debug.Log($"[PlayerAppearanceManager] Setting appearance index to {index}");
            SetAppearanceServerRpc(index);
        }

        // Subscribe to changes in appearanceIndex
        appearanceIndex.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"[PlayerAppearanceManager] OnValueChanged triggered: oldValue={oldValue}, newValue={newValue}");
            ApplyAppearance(newValue);
        };
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Apply the initial appearance when the player spawns
        ApplyAppearance(appearanceIndex.Value);
    }

    private void ApplyAppearance(int index)
    {
        if (spriteRenderer != null && sprites.Length > index)
        {
            Debug.Log($"[PlayerAppearanceManager] Applying appearance for index {index}");
            spriteRenderer.sprite = sprites[index];
        }
        else
        {
            Debug.LogWarning($"[PlayerAppearanceManager] Invalid index {index} or sprites array is empty!");
        }
    }

    [ServerRpc]
    private void SetAppearanceServerRpc(int index)
    {
        Debug.Log($"[PlayerAppearanceManager] ServerRpc called to set appearance index to {index}");
        appearanceIndex.Value = index;

        // Immediately apply the appearance on the host
        if (IsHost)
        {
            ApplyAppearance(index);
        }
    }
}
