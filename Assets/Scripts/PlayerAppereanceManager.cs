// zarz¹dzanie wygl¹dem gracza w grze 2D

using Unity.Netcode;
using UnityEngine;

public class PlayerAppearanceManager : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // komponent renderuj¹cy grafikê gracza
    [SerializeField] private Sprite[] sprites;             // tablica mo¿liwych wygl¹dów (sprite'ów)

    private NetworkVariable<int> appearanceIndex = new NetworkVariable<int>(0); // zmienna sieciowa do synchronizacji wygl¹du

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ustaw wygl¹d w zale¿noœci od tego, czy to host, czy klient
        if (IsOwner)
        {
            int index = NetworkManager.Singleton.IsHost ? 0 : 1; // host dostaje index 0, klient - 1
            Debug.Log($"[PlayerAppearanceManager] Ustawianie wygl¹du na index {index}");
            SetAppearanceServerRpc(index); // wysy³anie do serwera informacji o wybranym wygl¹dzie
        }

        // subskrypcja zmiany wygl¹du
        appearanceIndex.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"[PlayerAppearanceManager] Wygl¹d zmieniony: oldValue={oldValue}, newValue={newValue}");
            ApplyAppearance(newValue); // zastosowanie nowego wygl¹du
        };

        // zastosowanie pocz¹tkowego wygl¹du po pojawieniu siê obiektu
        ApplyAppearance(appearanceIndex.Value);
    }

    private void ApplyAppearance(int index)
    {
        // sprawdzanie, czy index jest poprawny i czy spriteRenderer istnieje
        if (spriteRenderer != null && sprites.Length > index)
        {
            Debug.Log($"[PlayerAppearanceManager] Stosowanie wygl¹du dla indexu {index}");
            spriteRenderer.sprite = sprites[index]; // ustawianie odpowiedniego sprite'a
        }
        else
        {
            Debug.LogWarning($"[PlayerAppearanceManager] Nieprawid³owy index {index} lub tablica sprite'ów jest pusta!");
        }
    }

    [ServerRpc]
    private void SetAppearanceServerRpc(int index)
    {
        Debug.Log($"[PlayerAppearanceManager] ServerRpc: ustawianie indexu wygl¹du na {index}");
        appearanceIndex.Value = index; // ustawienie zmiennej sieciowej

        // natychmiastowe zastosowanie wygl¹du, jeœli to host
        if (IsHost)
        {
            ApplyAppearance(index);
        }
    }
}
