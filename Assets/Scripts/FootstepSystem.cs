using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FootstepSystem : MonoBehaviour
{
    [Header("Footstep Settings")]
    [Tooltip("Intervalo de tempo entre passos (em segundos).")]
    public float footstepInterval = 0.5f;
    [Tooltip("Velocidade mínima para considerar que o jogador está se movendo.")]
    public float speedThreshold = 0.1f;
    [Tooltip("Distância para checar se o jogador está no chão.")]
    public float groundCheckDistance = 1.1f;

    [Header("Audio Settings")]
    [Tooltip("Fonte de áudio para reproduzir os passos.")]
    public AudioSource footstepAudioSource;
    [Tooltip("Clipes de áudio dos passos.")]
    public AudioClip[] footstepClips;

    private Rigidbody rb;
    private float footstepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void Update()
    {
        // Verifica se o jogador está se movendo e está no chão
        if (IsMoving() && IsGrounded())
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    // Verifica se a velocidade do Rigidbody ultrapassa o limite definido
    bool IsMoving()
    {
        return rb.linearVelocity.magnitude > speedThreshold;
    }

    // Usa um raycast para checar se o jogador está no chão
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    // Reproduz um som de passo aleatório da lista
    void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0 && footstepAudioSource != null)
        {
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[index];
            footstepAudioSource.PlayOneShot(clip);
        }
    }
}
