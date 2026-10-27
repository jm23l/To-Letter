using UnityEngine;
namespace LetterRun2D
{
    public class CourierVisual2D : MonoBehaviour
    {
        public CourierPlayer player;
        public SpriteRenderer art;
        private Rigidbody2D body;
        private Sprite down, up, side;
        private Vector3 rest;
        private float walk;
        private void Awake()
        {
            body=GetComponent<Rigidbody2D>();rest=art.transform.localPosition;
            down=Resources.Load<Sprite>("Sprites/courier_down");
            up=Resources.Load<Sprite>("Sprites/courier_up");
            side=Resources.Load<Sprite>("Sprites/courier_side");
        }
        private void LateUpdate()
        {
            if (art == null || player == null || body == null) return;
            Vector2 velocity=body.linearVelocity;
            bool moving=player.game.IsPlaying && !player.IsStunned && velocity.sqrMagnitude > 0.02f;
            if (moving)
            {
                if (Mathf.Abs(velocity.x)>Mathf.Abs(velocity.y)) { art.sprite=side;art.flipX=velocity.x<0; }
                else { art.sprite=velocity.y>0?up:down;art.flipX=false; }
                walk+=Time.deltaTime*14;
            }
            art.transform.localPosition=rest+Vector3.up*(moving?Mathf.Sin(walk)*0.035f:0);
            art.color=player.IsStunned?new Color(1,0.22f,0.22f):player.ShieldRemaining>0?new Color(0.65f,0.88f,1):Color.white;
            if(player.GraceRemaining>0) { var c=art.color;c.a=Mathf.Sin(Time.unscaledTime*24)>0?0.5f:1;art.color=c; }
            art.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10);
        }
    }
}
