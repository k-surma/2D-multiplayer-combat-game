using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal; // przechowuje dane wej�ciowe dla ruchu w poziomie
    private float speed = 8f; // pr�dko�� poruszania postaci
    private float jumpingPower = 16f; // si�a skoku
    private int jumpCount; // licznik skok�w
    private int maxJumps = 2; // maksymalna liczba skok�w

    [SerializeField] private Rigidbody2D rb; // komponent Rigidbody2D do sterowania fizyk�
    [SerializeField] private Transform groundCheck; // pozycja, kt�ra sprawdza, czy posta� stoi na ziemi
    [SerializeField] private LayerMask groundLayer; // warstwa definiuj�ca ziemi�

    private NetworkVariable<bool> isFacingRightNetwork = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // zmienna sieciowa do synchronizacji kierunku postaci

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ustawienie pocz�tkowego kierunku po za�adowaniu obiektu
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

        // odbieranie danych wej�ciowych dla ruchu poziomego
        horizontal = Input.GetAxisRaw("Horizontal");

        // obs�uga skok�w
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // ustawienie pr�dko�ci pionowej
            jumpCount++;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // zmniejszenie pr�dko�ci skoku po puszczeniu klawisza
        }

        // sprawdzenie kierunku patrzenia
        Flip();

        // reset licznika skok�w po dotkni�ciu ziemi
        if (IsGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // ustawienie pr�dko�ci ruchu w poziomie
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        // sprawdzanie, czy posta� stoi na ziemi
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private void Flip()
    {
        // sprawdzenie, czy trzeba zmieni� kierunek patrzenia
        if ((isFacingRightNetwork.Value && horizontal < 0f) || (!isFacingRightNetwork.Value && horizontal > 0f))
        {
            SetFacingDirectionServerRpc(!isFacingRightNetwork.Value); // wys�anie nowego kierunku do serwera
        }
    }

    private void ApplyFacingDirection(bool facingRight)
    {
        // zmiana kierunku patrzenia na podstawie warto�ci facingRight
        Vector3 localScale = transform.localScale;
        localScale.x = facingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    [ServerRpc]
    private void SetFacingDirectionServerRpc(bool facingRight)
    {
        // aktualizacja warto�ci zmiennej sieciowej
        isFacingRightNetwork.Value = facingRight;
    }
}
