using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public float ScaleFactor { get; set; }

    private GameObject[,] tiles;
    private GameObject[,] tilesReadyToHexplode;

    private Tuple<int, int, GameObject>[] selectedTiles = new Tuple<int, int, GameObject>[3];
    private bool areSelectedTilesRotating;
    private bool areSelectedTilesRotatingClockwise;
    private bool rotationJustStopped;

    private IDictionary<Tuple<int, int>, Vector2> indexToWorldCoordinate = new Dictionary<Tuple<int, int>, Vector2>();
    private bool initialGenerationHasEnded;

    public void Update()
    {
        MoveDownAirborneHexes();
        CreateNewHexesForEmptyCells();
        if (!AllTilesAreStationary())
        {
            return;
        }
        RotateSelectedTilesInGrandArray();
        MarkHexplosions();
        HexplodeMarkedHexes();
        LetRotateOrStopRotatingHexes();
        CountDownBombs();
        CheckIfGameIsOver();
    }

    #region Hex Selection and Movement Related Functions
    //I'm starting to wonder if I should've
    //used a graph instead of a 2D array :)
    public void SelectTrioAround(Vector2 touchPoint)
    {
        if (!selectedTiles.Contains(null))
        {
            selectedTiles[0].Item3.GetComponent<AbstractTile>().DeSelect();
            selectedTiles[1].Item3.GetComponent<AbstractTile>().DeSelect();
            selectedTiles[2].Item3.GetComponent<AbstractTile>().DeSelect();
            selectedTiles[0] = null;
            selectedTiles[1] = null;
            selectedTiles[2] = null;
            areSelectedTilesRotatingClockwise = false;
        }

        Dictionary<Tuple<int, int>, float> distancesToTouchPoint = new Dictionary<Tuple<int, int>, float>();
        foreach (KeyValuePair<Tuple<int, int>, Vector2> entry in indexToWorldCoordinate)
        {
            distancesToTouchPoint[entry.Key] = Vector2.Distance(entry.Value, touchPoint);
        }
        distancesToTouchPoint = distancesToTouchPoint.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        if (distancesToTouchPoint.ElementAt(0).Value > .5)
        {
            //touched too far :)
            return;
        }
        Tuple<int, int> current = distancesToTouchPoint.ElementAt(0).Key;

        int i = current.Item1;
        int j = current.Item2;

        int topIndex = j < tiles.GetLength(1) ? j + 1 : -1;
        int bottomIndex = j > 0 ? j - 1 : -1;
        int rightIndex = i < tiles.GetLength(0) ? i + 1 : -1;
        int leftIndex = i > 0 ? i - 1 : -1;

        GameObject topHex = GetTile(i, topIndex);
        GameObject bottomHex = GetTile(i, bottomIndex);
        GameObject rightHex = GetTile(rightIndex, j);
        GameObject leftHex = GetTile(leftIndex, j);

        //Either bottom or top right/left depending on i
        GameObject rightHex2, leftHex2;
        if(i % 2 == 0)
        {
            rightHex2 = GetTile(rightIndex, topIndex);
            leftHex2 = GetTile(leftIndex, topIndex);
        }
        else
        {
            rightHex2 = GetTile(rightIndex, bottomIndex);
            leftHex2 = GetTile(leftIndex, bottomIndex);
        }
        selectedTiles[0] = Tuple.Create(i, j, GetTile(i, j));

        Dictionary<GameObject, float> distances = new Dictionary<GameObject, float>();
        if(topHex != null)  distances[topHex] = Vector2.Distance(topHex.transform.position, touchPoint);
        if(bottomHex != null) distances[bottomHex] = Vector2.Distance(bottomHex.transform.position, touchPoint);
        if(rightHex != null) distances[rightHex] = Vector2.Distance(rightHex.transform.position, touchPoint);
        if(leftHex != null) distances[leftHex] = Vector2.Distance(leftHex.transform.position, touchPoint);
        if(rightHex2 != null) distances[rightHex2] = Vector2.Distance(rightHex2.transform.position, touchPoint);
        if(leftHex2 != null) distances[leftHex2] = Vector2.Distance(leftHex2.transform.position, touchPoint);
        distances = distances.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        Tuple<int, int> index = indexToWorldCoordinate.FirstOrDefault(x => x.Value.Equals(distances.ElementAt(0).Key.transform.position)).Key;
        selectedTiles[1] = Tuple.Create(index.Item1, index.Item2, distances.ElementAt(0).Key);
        index = indexToWorldCoordinate.FirstOrDefault(x => x.Value.Equals(distances.ElementAt(1).Key.transform.position)).Key;
        selectedTiles[2] = Tuple.Create(index.Item1, index.Item2, distances.ElementAt(1).Key);

        selectedTiles = selectedTiles.OrderByDescending(x=>x.Item3.transform.position.y).ToArray();

        selectedTiles[0].Item3.GetComponent<AbstractTile>().Select();
        selectedTiles[1].Item3.GetComponent<AbstractTile>().Select();
        selectedTiles[2].Item3.GetComponent<AbstractTile>().Select();
    }

    public void TurnCW()
    {
        if (!selectedTiles.Contains(null))
        {
            Vector2 center = new Vector2((selectedTiles[0].Item3.transform.position.x + selectedTiles[1].Item3.transform.position.x + selectedTiles[2].Item3.transform.position.x) / 3, (selectedTiles[0].Item3.transform.position.y + selectedTiles[1].Item3.transform.position.y + selectedTiles[2].Item3.transform.position.y) / 3);
            selectedTiles[0].Item3.GetComponent<AbstractTile>().RotateCWAround(center);
            selectedTiles[1].Item3.GetComponent<AbstractTile>().RotateCWAround(center);
            selectedTiles[2].Item3.GetComponent<AbstractTile>().RotateCWAround(center);
            areSelectedTilesRotating = true;
            areSelectedTilesRotatingClockwise = true;
        }
    }

    public void TurnCCW()
    {
        if (!selectedTiles.Contains(null))
        {
            Vector2 center = new Vector2((selectedTiles[0].Item3.transform.position.x + selectedTiles[1].Item3.transform.position.x + selectedTiles[2].Item3.transform.position.x) / 3, (selectedTiles[0].Item3.transform.position.y + selectedTiles[1].Item3.transform.position.y + selectedTiles[2].Item3.transform.position.y) / 3);
            selectedTiles[0].Item3.GetComponent<AbstractTile>().RotateCCWAround(center);
            selectedTiles[1].Item3.GetComponent<AbstractTile>().RotateCCWAround(center);
            selectedTiles[2].Item3.GetComponent<AbstractTile>().RotateCCWAround(center);
            areSelectedTilesRotating = true;
            areSelectedTilesRotatingClockwise = false;
        }
    }

    public void RotationStopped()
    {
        if (areSelectedTilesRotating)
        {
            RotateSelectedTilesInGrandArray();
            areSelectedTilesRotating = false;
        }
        rotationJustStopped = true;
    }

    #endregion

    #region Hex Storage Related Functions
    public void Allocate(int w, int h)
    {
        tiles = new GameObject[w, h];
        tilesReadyToHexplode = new GameObject[w, h];
    }

    public GameObject GetTile(int i, int j)
    {
        if(i < 0 || j < 0 || i > tiles.GetLength(0) - 1 || j > tiles.GetLength(1) - 1)
        {
            return null;
        }
        return tiles[i, j];
    }

    public void SetTile(int i, int j, GameObject o)
    {
        if (i < 0 || j < 0 || i > tiles.GetLength(0) - 1 || j > tiles.GetLength(1) - 1)
        {
            return;
        }
        tiles[i, j] = o;
        if(!initialGenerationHasEnded)
        {
            indexToWorldCoordinate[Tuple.Create(i, j)] = o.transform.position;
        }
    }

    private void SetMemory(int i, int j, GameObject o)
    {
        if (i < 0 || j < 0 || i > tilesReadyToHexplode.GetLength(0) - 1 || j > tilesReadyToHexplode.GetLength(1) - 1)
        {
            return;
        }
        tilesReadyToHexplode[i, j] = o;
    }

    public void EndInitialGenerationPhase()
    {
        initialGenerationHasEnded = true;
    }
    #endregion

    #region Game Board Related Logic
    private bool AllTilesAreStationary()
    {
        for (int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                if(tiles[i, j].GetComponent<AbstractTile>().IsFalling() || tiles[i, j].GetComponent<AbstractTile>().IsRotating())
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void MoveDownAirborneHexes() {
        for (int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                if (tiles[i, j] == null)
                {
                    int k = j;
                    while (k < tiles.GetLength(1) && tiles[i, k] == null)
                    {
                        ++k;
                    }
                    if (k < tiles.GetLength(1))
                    {
                        tiles[i, j] = tiles[i, k];
                        tiles[i, k] = null;
                        tiles[i, j].GetComponent<AbstractTile>().FallTo(indexToWorldCoordinate[Tuple.Create(i, j)]);
                    }
                }
            }
        }
    }

    private void CreateNewHexesForEmptyCells()
    {
        for (int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                if (tiles[i, j] == null)
                {
                    GetComponentInParent<Generator>().GenerateNewHex(i, j, indexToWorldCoordinate[Tuple.Create(i, j)] + CalculateHexSpawnOffset(AbstractTile.ACCELERATION, AbstractTile.FALL_TIME), this);
                    tiles[i, j].GetComponent<AbstractTile>().FallTo(indexToWorldCoordinate[Tuple.Create(i, j)]);
                }
            }
        }
    }

    private void RotateSelectedTilesInGrandArray()
    {
        if (!selectedTiles.Contains(null) && areSelectedTilesRotating)
        {
            GameObject zero = tiles[selectedTiles[0].Item1, selectedTiles[0].Item2];
            GameObject one = tiles[selectedTiles[1].Item1, selectedTiles[1].Item2];
            GameObject two = tiles[selectedTiles[2].Item1, selectedTiles[2].Item2];
            if (areSelectedTilesRotatingClockwise)
            {
                if (selectedTiles[0].Item1 < selectedTiles[1].Item1)
                {
                    tiles[selectedTiles[0].Item1, selectedTiles[0].Item2] = one;
                    tiles[selectedTiles[1].Item1, selectedTiles[1].Item2] = two;
                    tiles[selectedTiles[2].Item1, selectedTiles[2].Item2] = zero;
                }
                else
                {
                    tiles[selectedTiles[0].Item1, selectedTiles[0].Item2] = two;
                    tiles[selectedTiles[1].Item1, selectedTiles[1].Item2] = zero;
                    tiles[selectedTiles[2].Item1, selectedTiles[2].Item2] = one;
                }
            }
            else
            {
                if (selectedTiles[0].Item1 < selectedTiles[1].Item1)
                {
                    tiles[selectedTiles[0].Item1, selectedTiles[0].Item2] = two;
                    tiles[selectedTiles[1].Item1, selectedTiles[1].Item2] = zero;
                    tiles[selectedTiles[2].Item1, selectedTiles[2].Item2] = one;
                }
                else
                {
                    tiles[selectedTiles[0].Item1, selectedTiles[0].Item2] = one;
                    tiles[selectedTiles[1].Item1, selectedTiles[1].Item2] = two;
                    tiles[selectedTiles[2].Item1, selectedTiles[2].Item2] = zero;
                }
            }
        }
    }

    //Wear your safety goggles!
    //Assigned to DOA, refactor into a graph from a 2D array ;)
    private void MarkHexplosions()
    {
        for (int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                SetMemory(i, j, null);

                int topIndex = j < tiles.GetLength(1) ? j + 1 : -1;
                int bottomIndex = j > 0 ? j - 1 : -1;
                int rightIndex = i < tiles.GetLength(0) ? i + 1 : -1;
                int leftIndex = i > 0 ? i - 1 : -1;

                GameObject topLeft = GetTile(leftIndex, topIndex);
                GameObject topRight = GetTile(rightIndex, topIndex);
                GameObject bottomLeft = GetTile(leftIndex, bottomIndex);
                GameObject bottomRight = GetTile(rightIndex, bottomIndex);
                GameObject top = GetTile(i, topIndex);
                GameObject bottom = GetTile(i, bottomIndex);
                GameObject left = GetTile(leftIndex, j);
                GameObject right = GetTile(rightIndex, j);
                GameObject current = GetTile(i, j);

                Color? topLeftColor = topLeft?.GetComponent<SpriteRenderer>().color;
                Color? topRightColor = topRight?.GetComponent<SpriteRenderer>().color;
                Color? bottomRightColor = bottomRight?.GetComponent<SpriteRenderer>().color;
                Color? bottomLeftColor = bottomLeft?.GetComponent<SpriteRenderer>().color;
                Color? topColor = top?.GetComponent<SpriteRenderer>().color;
                Color? rightColor = right?.GetComponent<SpriteRenderer>().color;
                Color? bottomColor = bottom?.GetComponent<SpriteRenderer>().color;
                Color? leftColor = left?.GetComponent<SpriteRenderer>().color;
                Color? currentColor = current?.GetComponent<SpriteRenderer>().color;

                topLeftColor = topLeftColor.HasValue ? topLeftColor : new Color(0.1f, 0.1f, 0.1f);
                topRightColor = topRightColor.HasValue ? topRightColor : new Color(0.1f, 0.2f, 0.1f);
                bottomRightColor = bottomRightColor.HasValue ? bottomRightColor : new Color(0.1f, 0.3f, 0.1f);
                bottomLeftColor = bottomLeftColor.HasValue ? bottomLeftColor : new Color(0.1f, 0.4f, 0.1f);
                topColor = topColor.HasValue ? topColor : new Color(0.1f, 0.5f, 0.1f);
                rightColor = rightColor.HasValue ? rightColor : new Color(0.1f, 0.6f, 0.1f);
                bottomColor = bottomColor.HasValue ? bottomColor : new Color(0.1f, 0.7f, 0.1f);
                leftColor = leftColor.HasValue ? leftColor : new Color(0.1f, 0.8f, 0.1f);
                currentColor = currentColor.HasValue ? currentColor : new Color(0.1f, 0.9f, 0.1f);

                if (i % 2 == 0) {
                    if (currentColor.Equals(topColor))
                    {
                        if (currentColor.Equals(topRightColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(i, topIndex, top);
                            SetMemory(rightIndex, topIndex, topRight);
                        }
                        if (currentColor.Equals(topLeftColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(i, topIndex, top);
                            SetMemory(leftIndex, topIndex, topLeft);
                        }
                    }

                    if (currentColor.Equals(rightColor))
                    {
                        if (currentColor.Equals(topRightColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(rightIndex, j, right);
                            SetMemory(rightIndex, topIndex, topRight);
                        }
                        if (currentColor.Equals(bottomColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(rightIndex, j, right);
                            SetMemory(i, bottomIndex, bottom);
                        }
                    }

                    if (currentColor.Equals(leftColor))
                    {
                        if (currentColor.Equals(topLeftColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(leftIndex, j, left);
                            SetMemory(leftIndex, topIndex, topLeft);
                        }
                        if (currentColor.Equals(bottomColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(leftIndex, j, left);
                            SetMemory(i, bottomIndex, bottom);
                        }
                    }
                }
                else
                {
                    if (currentColor.Equals(topColor))
                    {
                        if (currentColor.Equals(rightColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(i, topIndex, top);
                            SetMemory(rightIndex, j, right);
                        }
                        if (currentColor.Equals(leftColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(i, topIndex, top);
                            SetMemory(leftIndex, j, left);
                        }
                    }

                    if (currentColor.Equals(bottomRightColor))
                    {
                        if (currentColor.Equals(rightColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(rightIndex, bottomIndex, bottomRight);
                            SetMemory(rightIndex, j, right);
                        }
                        if (currentColor.Equals(bottomColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(rightIndex, bottomIndex, bottomRight);
                            SetMemory(i, bottomIndex, bottom);
                        }
                    }

                    if (currentColor.Equals(bottomLeftColor))
                    {
                        if (currentColor.Equals(leftColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(leftIndex, bottomIndex, bottomLeft);
                            SetMemory(leftIndex, j, left);
                        }
                        if (currentColor.Equals(bottomColor))
                        {
                            SetMemory(i, j, current);
                            SetMemory(leftIndex, bottomIndex, bottomLeft);
                            SetMemory(i, bottomIndex, bottom);
                        }
                    }
                }
            }
        }
    }

    private void HexplodeMarkedHexes()
    {
        for (int i = 0; i < tilesReadyToHexplode.GetLength(0); ++i)
        {
            for (int j = 0; j < tilesReadyToHexplode.GetLength(1); ++j)
            {
                if (tilesReadyToHexplode[i, j] != null)
                {
                    if (selectedTiles[0] != null && tiles[i, j].Equals(selectedTiles[0].Item3))
                    {
                        areSelectedTilesRotating = false;
                        selectedTiles[0].Item3.GetComponent<AbstractTile>().StopRotating();
                        selectedTiles[0] = null;
                        areSelectedTilesRotatingClockwise = false;
                    }
                    if (selectedTiles[1] != null && tiles[i, j].Equals(selectedTiles[1].Item3))
                    {
                        areSelectedTilesRotating = false;
                        selectedTiles[1].Item3.GetComponent<AbstractTile>().StopRotating();
                        selectedTiles[1] = null;
                        areSelectedTilesRotatingClockwise = false;
                    }
                    if (selectedTiles[2] != null && tiles[i, j].Equals(selectedTiles[2].Item3))
                    {
                        areSelectedTilesRotating = false;
                        selectedTiles[2].Item3.GetComponent<AbstractTile>().StopRotating();
                        selectedTiles[2] = null;
                        areSelectedTilesRotatingClockwise = false;
                    }
                    tilesReadyToHexplode[i, j].GetComponent<AbstractTile>().Hexplode();
                    tiles[i, j] = null;
                }
            }
        }
    }

    private void LetRotateOrStopRotatingHexes()
    {
        if (!selectedTiles.Contains(null) && areSelectedTilesRotating)
        {
            if (areSelectedTilesRotatingClockwise)
            {
                TurnCW();
            }
            else
            {
                TurnCCW();
            }
        }
        else if (rotationJustStopped)
        {
            rotationJustStopped = false;
            if (selectedTiles[0] != null)
            {
                selectedTiles[0].Item3.GetComponent<AbstractTile>().DeSelect();
                selectedTiles[0].Item3.GetComponent<AbstractTile>().StopRotating();
                selectedTiles[0].Item3.GetComponent<AbstractTile>().FallTo(null);
                selectedTiles[0] = null;
            }
            if (selectedTiles[1] != null)
            {
                selectedTiles[1].Item3.GetComponent<AbstractTile>().DeSelect();
                selectedTiles[1].Item3.GetComponent<AbstractTile>().StopRotating();
                selectedTiles[1].Item3.GetComponent<AbstractTile>().FallTo(null);
                selectedTiles[1] = null;
            }
            if (selectedTiles[2] != null)
            {
                selectedTiles[2].Item3.GetComponent<AbstractTile>().DeSelect();
                selectedTiles[2].Item3.GetComponent<AbstractTile>().StopRotating();
                selectedTiles[2].Item3.GetComponent<AbstractTile>().FallTo(null);
                selectedTiles[2] = null;
            }
        }
    }

    private void CountDownBombs()
    {
        bool didAnyOfTheHexesHexplode = false;
        List<BombTile> bombs = new List<BombTile>();
        for(int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                if(tiles[i, j] == null)
                {
                    didAnyOfTheHexesHexplode = true;
                }
                else
                {
                    if (tiles[i, j].TryGetComponent<BombTile>(out BombTile b))
                    {
                        bombs.Add(b);
                    }
                }
            }
        }
        if(didAnyOfTheHexesHexplode)
        {
            bombs.ForEach(bomb => bomb.CountDown());
        }
    }

    private void CheckIfGameIsOver()
    {
        for (int i = 0; i < tiles.GetLength(0); ++i)
        {
            for (int j = 0; j < tiles.GetLength(1); ++j)
            {
                if (tiles[i, j] != null && tiles[i, j].TryGetComponent<BombTile>(out BombTile b))
                {
                    if(b.IsKaboom())
                    {
                        //gameover ;)
                    }
                }
            }
        }
        //Also, check for no available moves!
        //Frankly, I don't know how to do this
        //other than using the brute force approach :(
    }
    #endregion

    #region Private Methods
    private Vector2 CalculateHexSpawnOffset(float a, float t)
    {
        return new Vector2(0, a * t * t * .5f);
    }
    #endregion
}
