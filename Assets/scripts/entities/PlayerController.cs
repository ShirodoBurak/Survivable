using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour {
    public GameObject arrow;
    private float horizontal;
    private float speed = 24f;
    private float jumpingPower = 64f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    public WorldHandler wh;

    void Update() {
        horizontal = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Jump") && IsGrounded()) {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0f) {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
        // Flip the sprite
        this.GetComponent<SpriteRenderer>().flipX = Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 0.5f;
        // Make arrow rotate towards the mouse on screen.
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        arrow.transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
        if(Input.GetMouseButton(0)) {
            Vector3Int pos = mouseToTileMapPos();
            wh.tileCache.tiles.TryGetValue(pos, out TileBase o);
            //if(o != null) {
                breakTile(pos);
                breakTile(pos + Vector3Int.right);
                breakTile(pos + Vector3Int.left);

                breakTile(pos + Vector3Int.up);
                breakTile(pos + Vector3Int.up + Vector3Int.right);
                breakTile(pos + Vector3Int.up + Vector3Int.left);

                breakTile(pos + Vector3Int.down);
                breakTile(pos + Vector3Int.down + Vector3Int.right);
                breakTile(pos + Vector3Int.down + Vector3Int.left);
            //}
        }
    }
    private void FixedUpdate() {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }
    public float size = 2.05f;
    private bool IsGrounded() {
        return Physics2D.OverlapCircle(groundCheck.position, size, groundLayer);
    }
    void breakTile(Vector3Int pos) {
        wh.tileCache.tiles[pos] = null;
        wh.mainTileMap.SetTile(pos, null);
        TileBase getDecorationTile = wh.decorationTileMap.GetTile(pos+Vector3Int.up);
        if(getDecorationTile != null) {
            wh.decorationTileMap.SetTile(pos + Vector3Int.up, null);
        } else if(getDecorationTile == wh.Tile("treestump")) {
            RemoveTree(pos + Vector3Int.up);
        }
    }
    private void RemoveTree(Vector3Int treepos) {
        int step = 0;
        while(wh.decorationTileMap.GetTile(treepos + new Vector3Int(0,step,0)) != null) {
            wh.decorationTileMap.SetTile(treepos + new Vector3Int(0, step, 0), null);
            step++;
        }
    }
    Vector3Int mouseToTileMapPos() {
        Vector2 mouseToWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3Int((int)mouseToWorld.x, (int)mouseToWorld.y, 0);
    }
}
