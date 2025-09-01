using UnityEngine;
using Unity.Netcode;

public class SimplePlayerController : NetworkBehaviour
{
    public NetworkVariable<ulong> PlayerID;

    public ulong PlayerID2;


    public float speed;
    private Animator animator;
    private Rigidbody rb;
    public LayerMask groundLayer;
    public float jumpForce = 5f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;

        float x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        float y = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;

        if (x != 0 || y != 0)
        {
            MovePlayerServerRpc(x, y);
        }
        CheckGroundRpc();

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            AnimatorSetTriggerRpc("Jump");
        }
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }


    [Rpc(SendTo.Server)]
    public void AnimatorSetTriggerRpc(string animationName)
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        animator.SetTrigger(animationName);
    }

    [ServerRpc]
    private void MovePlayerServerRpc(float x, float y)
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + new Vector3(x, 0, y));
        }
    }

    [Rpc(SendTo.Server)]
    public void CheckGroundRpc()
    {
        if (IsGrounded())
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("FreeFall", false);
        }
        else
        {
            animator.SetBool("Grounded", false);
            animator.SetBool("FreeFall", true);

        }
    }
}