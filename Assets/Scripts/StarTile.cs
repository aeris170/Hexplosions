using UnityEngine;

public class StarTile : AbstractTile
{

    public override void Hexplode()
    {
        score.Increment(20);
        Destroy(gameObject);
    }
}