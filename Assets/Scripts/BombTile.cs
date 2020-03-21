using UnityEngine;

public class BombTile : AbstractTile
{
    public Sprite[] sprites;

    private int turnsLeftToKaboom = 8;

    public override void Start()
    {
        spriteRenderer.sprite = sprites[turnsLeftToKaboom];
    }

    public bool IsKaboom()
    {
        return turnsLeftToKaboom < 1;
    }

    public void CountDown()
    {
        --turnsLeftToKaboom;
        if (turnsLeftToKaboom < 0)
        {
            return;
        }
        spriteRenderer.sprite = sprites[turnsLeftToKaboom];
    }

    public override void Hexplode()
    {
        score.Increment(5);
        Destroy(gameObject);
    }
}