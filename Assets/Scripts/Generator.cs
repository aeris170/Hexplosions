using UnityEngine;

public class Generator : MonoBehaviour
{
    public GameObject Board;
    public GameObject[] Hexes;

    [Range(3, 30)]
    public int GridWidth = 8;

    [Range(3, 30)]
    public int GridHeight = 9;

    private Score score;

    void Start()
    {
        score = GameObject.Find("Score").GetComponent<Score>();
        GenerateAll();
    }

    #region Hex Generator Functions
    void GenerateAll()
    {
        GameBoard gb = Instantiate(Board, transform).GetComponent<GameBoard>();
        gb.Allocate(GridWidth, GridHeight);

        Vector3 hexSize = Hexes[0].GetComponent<SpriteRenderer>().bounds.size;
        float hexWidth = hexSize.x + .15f;
        float hexHeight = hexSize.y + .15f;

        CalculateScale(gb);

        Vector3 center = new Vector3(-hexWidth * .75f * (GridWidth - 1) / 2, -hexHeight * (GridHeight - 1) / 2 - Camera.main.orthographicSize * .3f);

        float yOffsetBetweenColumns;

        for (int xx = 0; xx < GridWidth; ++xx)
        {
            yOffsetBetweenColumns = xx % 2 == 0 ? 0 : hexHeight / 2;
            for (int yy = 0; yy < GridHeight; ++yy)
            {

                GameObject hexagon = InstantiateHex();
                hexagon.transform.position = new Vector3(xx * hexWidth * .75f, yy * hexHeight - yOffsetBetweenColumns, 0f);
                hexagon.transform.Translate(center);
                hexagon.transform.localScale = new Vector3(gb.ScaleFactor, gb.ScaleFactor, gb.ScaleFactor);
                hexagon.transform.position = Vector3.Scale(hexagon.transform.position, hexagon.transform.localScale);
                gb.SetTile(xx, yy, hexagon);
            }
        }
        gb.EndInitialGenerationPhase();
    }

    public void GenerateNewHex(int i, int j, Vector3 position, GameBoard gb)
    {
        GameObject hexagon = InstantiateHex();
        hexagon.transform.position = position;
        hexagon.transform.localScale = new Vector3(gb.ScaleFactor, gb.ScaleFactor, gb.ScaleFactor);
        gb.SetTile(i, j, hexagon);
    }
    #endregion

    #region Mutual Accessors/Mutators to GameBoard
    private GameObject InstantiateHex()
    {
        GameObject hexagon;
        int rngResult = UnityEngine.Random.Range(0, 100);
        if (rngResult < 5 && score.GetScoreAsNumber() > 1000)
        {
            hexagon = Instantiate(Hexes[2]);
        }
        else if (rngResult < 10)
        {
            hexagon = Instantiate(Hexes[1]);
        }
        else
        {
            hexagon = Instantiate(Hexes[0]);
        }
        hexagon.transform.parent = transform;
        return hexagon;
    }

    private void CalculateScale(GameBoard gb)
    {
        Vector3 rt = Hexes[0].GetComponent<SpriteRenderer>().sprite.rect.size;

        float width = rt.x + 30;
        float height = rt.y + 30;

        float xLength = (2 * GridWidth - (GridWidth / 2)) * width;
        float yLength = (GridHeight + .5f) * height * .75f;

        //proportionality constant
        float minRequiredProportionaly = Mathf.Min((1080 - 100) / xLength, (1920 - 150) * 9 / 10 / yLength);

        gb.ScaleFactor = minRequiredProportionaly;
    }
    #endregion
}
