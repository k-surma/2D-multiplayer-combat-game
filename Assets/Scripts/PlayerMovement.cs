using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal; // przechowuje dane wejœciowe dla ruchu w poziomie
    private float speed = 8f; // prêdkoœæ poruszania postaci
    private float jumpingPower = 16f; // si³a skoku
    private int jumpCount; // licznik skoków
    private int maxJumps = 2; // maksymalna liczba skoków

    [SerializeField] private Rigidbody2D rb; // komponent Rigidbody2D do sterowania fizyk¹
    [SerializeField] private Transform groundCheck; // pozycja, która sprawdza, czy postaæ stoi na ziemi
    [SerializeField] private LayerMask groundLayer; // warstwa definiuj¹ca ziemiê

    private NetworkVariable<bool> isFacingRightNetwork = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // zmienna sieciowa do synchronizacji kierunku postaci

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ustawienie pocz¹tkowego kierunku po za³adowaniu obiektu
        ApplyFacingDirection(isFacingRightNetwork.Value);

        // subskrypcja zmiany kierunku patrzenia
        isFacingRightNetwork.OnValueChanged += (oldValue, newValue) =>
        {
            ApplyFacingDirection(newValue); // zastosowanie nowego kierunku
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        // odbieranie danych wejœciowych dla ruchu poziomego
        horizontal = Input.GetAxisRaw("Horizontal");

        // obs³uga skoków
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // ustawienie prêdkoœci pionowej
            jumpCount++;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // zmniejszenie prêdkoœci skoku po puszczeniu klawisza
        }

        // sprawdzenie kierunku patrzenia
        Flip();

        // reset licznika skoków po dotkniêciu ziemi
        if (IsGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // ustawienie prêdkoœci ruchu w poziomie
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        // sprawdzanie, czy postaæ stoi na ziemi
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private void Flip()
    {
        // sprawdzenie, czy trzeba zmieniæ kierunek patrzenia
        if ((isFacingRightNetwork.Value && horizontal < 0f) || (!isFacingRightNetwork.Value && horizontal > 0f))
        {
            SetFacingDirectionServerRpc(!isFacingRightNetwork.Value); // wys³anie nowego kierunku do serwera
        }
    }

    private void ApplyFacingDirection(bool facingRight)
    {
        // zmiana kierunku patrzenia na podstawie wartoœci facingRight
        Vector3 localScale = transform.localScale;
        localScale.x = facingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    [ServerRpc]
    private void SetFacingDirectionServerRpc(bool facingRight)
    {
        // aktualizacja wartoœci zmiennej sieciowej
        isFacingRightNetwork.Value = facingRight;
    }
}
