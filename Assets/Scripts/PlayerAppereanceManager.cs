// zarz�dzanie wygl�dem gracza w grze 2D

using Unity.Netcode;
using UnityEngine;

public class PlayerAppearanceManager : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // komponent renderuj�cy grafik� gracza
    [SerializeField] private Sprite[] sprites;             // tablica mo�liwych wygl�d�w (sprite'�w)

    private NetworkVariable<int> appearanceIndex = new NetworkVariable<int>(0); // zmienna sieciowa do synchronizacji wygl�du

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ustaw wygl�d w zale�no�ci od tego, czy to host, czy klient
        if (IsOwner)
        {
            int index = NetworkManager.Singleton.IsHost ? 0 : 1; // host dostaje index 0, klient - 1
            Debug.Log($"[PlayerAppearanceManager] Ustawianie wygl�du na index {index}");
            SetAppearanceServerRpc(index); // wysy�anie do serwera informacji o wybranym wygl�dzie
        }

        // subskrypcja zmiany wygl�du
        appearanceIndex.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"[PlayerAppearanceManager] Wygl�d zmieniony: oldValue={oldValue}, newValue={newValue}");
            ApplyAppearance(newValue); // zastosowanie nowego wygl�du
        };

        // zastosowanie pocz�tkowego wygl�du po pojawieniu si� obiektu
        ApplyAppearance(appearanceIndex.Value);
    }

    private void ApplyAppearance(int index)
    {
        // sprawdzanie, czy index jest poprawny i czy spriteRenderer istnieje
        if (spriteRenderer != null && sprites.Length > index)
        {
            Debug.Log($"[PlayerAppearanceManager] Stosowanie wygl�du dla indexu {index}");
            spriteRenderer.sprite = sprites[index]; // ustawianie odpowiedniego sprite'a
        }
        else
        {
            Debug.LogWarning($"[PlayerAppearanceManager] Nieprawid�owy index {index} lub tablica sprite'�w jest pusta!");
        }
    }

    [ServerRpc]
    private void SetAppearanceServerRpc(int index)
    {
        Debug.Log($"[PlayerAppearanceManager] ServerRpc: ustawianie indexu wygl�du na {index}");
        appearanceIndex.Value = index; // ustawienie zmiennej sieciowej

        // natychmiastowe zastosowanie wygl�du, je�li to host
        if (IsHost)
        {
            ApplyAppearance(index);
        }
    }
}
