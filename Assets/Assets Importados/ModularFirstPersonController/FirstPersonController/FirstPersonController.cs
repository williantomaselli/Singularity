using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirstPersonController : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    [Tooltip("FOV padrão da câmera")]
    public float fov = 60f;
    [Tooltip("Inverte o eixo vertical do mouse")]
    public bool invertCamera = false;
    [Tooltip("Permite ou não rotacionar a câmera com o mouse")]
    public bool cameraCanMove = true;
    [Tooltip("Sensibilidade do mouse")]
    public float mouseSensitivity = 2f;
    [Tooltip("Ângulo máximo (pra cima e pra baixo) que a câmera pode rotacionar")]
    public float maxLookAngle = 50f;

    [Header("Cursor / Crosshair")]
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    [Header("Movimentação")]
    [Tooltip("Permite ou não movimentar o jogador")]
    public bool playerCanMove = true;
    [Tooltip("Velocidade de caminhada")]
    public float walkSpeed = 5f;
    [Tooltip("Controle de aceleração máxima ao usar AddForce")]
    public float maxVelocityChange = 10f;

    [Header("Pulo")]
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public Transform joint;          // Objeto que se move (câmera ou um pivot)
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internos
    private Rigidbody rb;
    private Image crosshairObject;
    private float yaw = 0f;
    private float pitch = 0f;
    private bool isWalking = false;
    private bool isGrounded = false;
    private Vector3 jointOriginalPos;
    private Vector3 originalScale;
    private float timer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Se existir um Image em algum filho, associamos para usar como crosshair
        crosshairObject = GetComponentInChildren<Image>();

        // Configura FOV inicial
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = fov;
        }

        // Armazena escala original (caso seja usada em outras lógicas)
        originalScale = transform.localScale;

        // Armazena posição original do joint para o Head Bob
        if (joint != null)
        {
            jointOriginalPos = joint.localPosition;
        }
    }

    void Start()
    {
        // Trava cursor se necessário
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Configura crosshair
        if (crosshair && crosshairObject != null)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else if (crosshairObject != null)
        {
            crosshairObject.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Rotação da Câmera (Mouse Look)
        if (cameraCanMove && playerCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw = transform.localEulerAngles.y + mouseX;
            pitch += (invertCamera ? mouseY : -mouseY);
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0f, yaw, 0f);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        // Verifica se está no chão
        CheckGround();

        // Pulo
        if (enableJump && isGrounded && Input.GetKeyDown(jumpKey))
        {
            Jump();
        }

        // Head Bob
        if (enableHeadBob && joint != null)
        {
            HeadBob();
        }
    }

    void FixedUpdate()
    {
        if (!playerCanMove) return;

        // Lê input de movimento
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 targetVelocity = new Vector3(inputX, 0, inputZ);

        // Checa se está andando
        isWalking = (targetVelocity.magnitude > 0.01f && isGrounded);

        // Converte para direção local
        targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

        // Calcula diferença de velocidade
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);

        // Limita aceleração
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0f;

        // Aplica força
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CheckGround()
    {
        // Raycast para checar se está no chão
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        float distance = 0.75f;

        // Ray para baixo
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            timer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Sin(timer) * bobAmount.x;
            float bobY = Mathf.Sin(timer) * bobAmount.y;
            float bobZ = Mathf.Sin(timer) * bobAmount.z;

            joint.localPosition = new Vector3(
                jointOriginalPos.x + bobX,
                jointOriginalPos.y + bobY,
                jointOriginalPos.z + bobZ
            );
        }
        else
        {
            // Reseta o bob quando para de andar
            timer = 0f;
            float lerpSpeed = Time.deltaTime * bobSpeed;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, lerpSpeed);
        }
    }
}
