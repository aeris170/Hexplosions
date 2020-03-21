using UnityEngine;

public class PlainTile : AbstractTile
{

    public override void Hexplode()
    {
        score.Increment(5);
        Destroy(gameObject);
    }
}