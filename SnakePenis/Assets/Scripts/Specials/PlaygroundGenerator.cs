using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public enum TileOrientation
{
    West = -1, North = 10, East = 1, South = -10, Undefined = 0
}

public struct TilePosition
{
    public int x;
    public int z;

    public static TilePosition operator + (TilePosition left, TilePosition right)
    {
        return new TilePosition { x = left.x + right.x, z = left.z + right.z };
    }

    public static TilePosition operator - (TilePosition left, TilePosition right)
    {
        return new TilePosition { x = left.x - right.x, z = left.z - right.z };
    }

    public static bool operator == (TilePosition left, TilePosition right)
    {
        return left.x == right.x && left.z == right.z;
    }

    public override bool Equals(object o)
    {
        if (o == null)
        {
            return false;
        }
        TilePosition t = (TilePosition)o;
        return x == t.x && z == t.z;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ (z + 1);
        }
    }

    public static bool operator !=(TilePosition left, TilePosition right)
    {
        return left.x != right.x || left.z != right.z;
    }

    public TilePosition AddOrientation(TileOrientation orientation)
    {
        switch (orientation)
        {
            case TileOrientation.East:
                return this + new TilePosition() { x = 1, z = 0 };
            case TileOrientation.West:
                return this + new TilePosition() { x = -1, z = 0 };
            case TileOrientation.North:
                return this + new TilePosition() { x = 0, z = 1 };
            case TileOrientation.South:
                return this + new TilePosition() { x = 0, z = -1 };
            default:
                return this;
        }
    }

    public override string ToString()
    {
        return "{x : " + x.ToString() + ", z : " + z.ToString() + "}";
    }

    public static TilePosition Zero;
}

public struct TileSize
{
    public float Width;
}

[System.Serializable]
public class Tile
{
    private bool isPlaced = false;
    public bool IsPlaced()
    {
        return isPlaced;
    }
    public GameObject TileGroundObj;
    public List<TileOrientation> Orientations;
    public List<TileOrientation> FreeOrientations
    {
        get
        {
            return Orientations.FindAll(o => this.IsOrientationAlreadyBusy(o));
        }
    }
    public TilePosition Position;

    public string Name
    {
        get
        {
            string name = "Tile";
            if (Orientations.Count > 0)
            {
                foreach (TileOrientation o in Orientations)
                {
                    name += o.ToString();
                }
            } else
            {
                name += "_";
            }
            return name;
        }
    }

    private SpawnArea spawnArea;

    public SpawnArea SpawnArea
    {
        get
        {
            if (spawnArea == null)
            {
                InitSpawnArea();
            }
            return spawnArea;
        }
    }


    public void InitSpawnArea()
    {
        if (spawnArea == null)
        {
            spawnArea = new SpawnArea();
        }
        if (spawnArea.Vertices == null)
        {
            spawnArea.Vertices = new List<Vector2>();
        } else
        {
            spawnArea.Vertices.Clear();
        }
        foreach (TileOrientation o in Orientations)
        {
            AddVerticesAtOrientation(o);
        }
        if (Orientations.Count == 1)
        {
            AddVerticesAtOrientation(GetOppositeTileOrientation(Orientations[0]));
        } else if ((Orientations.Count == 2 && Orientations[0] ==  GetOppositeTileOrientation(Orientations[1])) == false)
        {
            AddMiddleVertices();
        }

        foreach (Vector2 v in spawnArea.Vertices)
        {
            Debug.DrawRay(this.GetRealPosition() + new Vector3(v.x, 0, v.y), Vector3.up * 100, Color.red, 20);
        }
    }

    void AddVerticesAtOrientation(TileOrientation o)
    {
        float x = Size.Width / 6 * 0.8f;
        float y = Size.Width / 2 * 0.8f;
        switch (o)
        {
            case TileOrientation.North:
                spawnArea.Vertices.Add(new Vector2 { x = -x, y = y });
                spawnArea.Vertices.Add(new Vector2 { x = x, y = y });
                break;
            case TileOrientation.South:
                spawnArea.Vertices.Add(new Vector2 { x = -x, y = -y });
                spawnArea.Vertices.Add(new Vector2 { x = x, y = -y });
                break;
            case TileOrientation.East:
                spawnArea.Vertices.Add(new Vector2 { x = y, y = x });
                spawnArea.Vertices.Add(new Vector2 { x = y, y = -x });
                break;
            case TileOrientation.West:
                spawnArea.Vertices.Add(new Vector2 { x = -y, y = x });
                spawnArea.Vertices.Add(new Vector2 { x = -y, y = -x });
                break;
        }
    }

    void AddMiddleVertices()
    {
        float side = Size.Width / 6 * 0.8f;
        spawnArea.Vertices.Add(new Vector2 { x = side, y = side });
        spawnArea.Vertices.Add(new Vector2 { x = -side, y = -side });
        spawnArea.Vertices.Add(new Vector2 { x = -side, y = side });
        spawnArea.Vertices.Add(new Vector2 { x = side, y = -side });
    }

    public static float Height = -3f;
    [SerializeField] public static TileSize Size = new TileSize() { Width = 190 };
    public static Vector3 startingTilePosition = Vector3.zero + Vector3.up * Height;
    private List<Tile> ConnectedTiles;

    public Tile(Tile t)
    {
        this.TileGroundObj = Object.Instantiate(t.TileGroundObj);
        this.Orientations = new List<TileOrientation>();
        foreach (TileOrientation o in t.Orientations)
        {
            this.Orientations.Add(o);
        }
        this.ConnectedTiles = new List<Tile>();
        this.TileGroundObj.name = this.Name;
    }

    public void SetGameObjectName(string newName)
    {
        this.TileGroundObj.name = newName;
    }

    public string GetGameObjectName()
    {
        return this.TileGroundObj.name;
    }

    public Tile(Tile t, TilePosition Position) : this(t)
    {
        t.Position = Position;
        t.TileGroundObj.transform.position = ConvertTilePositionToRealPosition(Position);
    }

    /// <summary>
    /// Get all the orientations of the otherTile that can be connected to this tile.
    /// </summary>
    /// <param name="otherTile"></param>
    /// <returns></returns>
    public List<TileOrientation> GetAvailableOrientationsToConnect(Tile otherTile)
    {
        List<TileOrientation> availableOrientations = new List<TileOrientation>();
        foreach (TileOrientation to in this.Orientations)
        {
            foreach (TileOrientation otherTo in otherTile.Orientations)
            {
                if (AreOrientationsConnectable(to, otherTo))
                {
                    if (IsOrientationAlreadyBusy(to) == false)
                    {
                        // Debug.Log($"Tile {this.Name} can connect to orientation '{otherTo}' of tile {otherTile.Name}.");
                        availableOrientations.Add(otherTo);
                    }
                }
            }
        }

        return availableOrientations;
    }

    public bool IsOrientationAlreadyBusy(TileOrientation to)
    {
        if (this.Orientations.Contains(to) == false)
        {
            return false;
        }
        if (ConnectedTiles == null)
        {
            return false;
        }
        TilePosition positionToCheck = GetPositionOfTileAtOrientation(to);
        foreach (Tile connectedTile in ConnectedTiles)
        {
            if (connectedTile.Position == positionToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public TilePosition GetPositionOfTileAtOrientation(TileOrientation orientation)
    {
        return Position.AddOrientation(orientation);
    }

    public static TileOrientation GetOppositeTileOrientation(TileOrientation tileOrientation)
    {
        return (TileOrientation)(-1*(int)tileOrientation);
    }

    public static bool AreOrientationsConnectable(TileOrientation o1, TileOrientation o2)
    {
        return (o1 == TileOrientation.East && o2 == TileOrientation.West) ||
            (o1 == TileOrientation.West && o2 == TileOrientation.East) ||
            (o1 == TileOrientation.North && o2 == TileOrientation.South) ||
            (o1 == TileOrientation.South && o2 == TileOrientation.North);
    }

    public bool IsTileConnectable(Tile t)
    {
        return t.GetAvailableOrientationsToConnect(this).Count > 0;
    }

    public static Vector3 ConvertTilePositionToRealPosition(TilePosition tilePosition)
    {
        Vector3 startingTilePosition = Vector3.zero + Vector3.up * Height;
        return startingTilePosition + Vector3.right * tilePosition.x * Size.Width + Vector3.forward * tilePosition.z * Size.Width;
    }

    public Vector3 GetRealPosition()
    {
        return ConvertTilePositionToRealPosition(this.Position);
    }

    public bool IsClosingTile()
    {
        return this.Orientations.Count == 1;
    }

    public Tile ConnectToTile(Tile newTile)
    {
        List<TileOrientation> AvailableOtherTileOrientations = this.GetAvailableOrientationsToConnect(newTile);
        if (AvailableOtherTileOrientations.Count == 0)
        {
            Debug.LogError("Can't connect tiles");
            newTile.Destroy();
            return null;
        }
        TileOrientation chosenOtherOrientation;
        chosenOtherOrientation = AvailableOtherTileOrientations[Random.Range(0, AvailableOtherTileOrientations.Count)];
        // chosenOtherOrientation = AvailableOtherTileOrientations[Random.Range(0, AvailableOtherTileOrientations.Count)];
        TileOrientation chosenOrientation = GetOppositeTileOrientation(chosenOtherOrientation);
        newTile.Place(this.GetPositionOfTileAtOrientation(chosenOrientation));
        newTile.ConnectedTiles.Add(this);
        this.ConnectedTiles.Add(newTile);
        return newTile;
    }

    public Tile ConnectToTileAtOrientation(Tile newTile, TileOrientation previousTileOrientation)
    {
        if (previousTileOrientation == TileOrientation.Undefined)
        {
            return ConnectToTile(newTile);
        }
        if (this.HasOrientation(previousTileOrientation) == false)
        {
            Debug.LogError($"Can't connect tiles. Orientation ''{previousTileOrientation}'' is not in originating tile");
            newTile.Destroy();
            return null;
        }
        List<TileOrientation> AvailableOtherTileOrientations = this.GetAvailableOrientationsToConnect(newTile);
        if (AvailableOtherTileOrientations.Count == 0 || 
            AvailableOtherTileOrientations.Contains(GetOppositeTileOrientation(previousTileOrientation)) == false
            )
        {
            Debug.LogError("Can't connect tiles");
            newTile.Destroy();
            return null;
        }
        TileOrientation chosenOtherOrientation;
        chosenOtherOrientation = GetOppositeTileOrientation(previousTileOrientation);
        // chosenOtherOrientation = AvailableOtherTileOrientations[Random.Range(0, AvailableOtherTileOrientations.Count)];
        TileOrientation chosenOrientation = previousTileOrientation;
        newTile.Place(this.GetPositionOfTileAtOrientation(chosenOrientation));
        newTile.ConnectedTiles.Add(this);
        this.ConnectedTiles.Add(newTile);
        return newTile;
    }

    /// <summary>
    /// Get all possible positions if this tile was connected to the tileToConnect one
    /// </summary>
    /// <param name="tileToConnect"> the already present tile to connect the new one</param>
    /// <returns></returns>
    public List<TilePosition> GetAllPossiblePositionIfConnectingToTile(Tile tileToConnect)
    {
        List<TileOrientation> AvailableOtherTileOrientations = this.GetAvailableOrientationsToConnect(tileToConnect);
        List<TilePosition> positions = new List<TilePosition>();
        foreach (TileOrientation o in AvailableOtherTileOrientations)
        {
            positions.Add(tileToConnect.GetPositionOfTileAtOrientation(o));
        }
        return positions;
    }

    public void Destroy()
    {
        Object.Destroy(this.TileGroundObj);
        this.Orientations.Clear();
    }

    public void Place(TilePosition tilePosition)
    {
        this.Position = tilePosition;
        this.TileGroundObj.transform.position = this.GetRealPosition();
        this.isPlaced = true;
    }

    public override string ToString()
    {
        return this.Name;
    }

    public bool CanConnectToOrientation(TileOrientation directionToConnect)
    {
        return this.Orientations.Contains(GetOppositeTileOrientation(directionToConnect));
    }

    public bool IsBranchingTile()
    {
        return Orientations.Count > 2;
    }

    public TileOrientation GetOrientationFromOtherTilePosition(TilePosition otherTilePosition)
    {
        List<TileOrientation> orientations = new List<TileOrientation>() { TileOrientation.East, TileOrientation.West, TileOrientation.South, TileOrientation.North };
        foreach (TileOrientation o in orientations)
        {
            if (this.Position.AddOrientation(o) == otherTilePosition)
            {
                return o;
            }
        }
        return TileOrientation.Undefined;
    }

    public bool HasOrientation(TileOrientation o)
    {
        return this.Orientations.Contains(o);
    }

    public bool IsFullyConnected()
    {
        if (this.IsPlaced() == false)
        {
            return false;
        }
        foreach (TileOrientation o in Orientations)
        {
            if (ConnectedTiles.Exists(t => t.Position == this.Position.AddOrientation(o)) == false)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsOrientationFree(TileOrientation o)
    {
        return IsOrientationAlreadyBusy(o) == false;
    }

    /// <summary>
    /// Set two already placed tiles to be connected, in case they're not
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    public static void ConnectTwoAlreadyPlacedTiles(Tile t1, Tile t2)
    {
        t1.ConnectedTiles.Add(t2);
        t2.ConnectedTiles.Add(t1);
    }

    public List<GameObject> GetWalls()
    {
        TileData tileData = TileGroundObj.GetComponent<TileData>();
        if (tileData)
        {
            return tileData.Walls;
        }
        return new List<GameObject>();
    }

    public void SetBoardTexture(Texture texture)
    {
        TileData td = TileGroundObj.GetComponent<TileData>();
        if (td)
        {
            foreach (GameObject ground in td.Ground)
            {
                ground.GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
    }
}

public class PlaygroundGenerator : MonoBehaviour
{
    public Tile InitialTile;
    public List<Tile> Tiles;

    public List<Tile> GeneratedTiles;

    public int minTilesCount = 2, maxTilesCount = 10, maxSubpathCount = 2;
    private int subPathCount = 0;

    public int seed = 0;
    [SerializeField] private bool GenerateRandomSeed = false;

    bool CheckIfTilesListIsConsistent()
    {
        foreach(Tile t1 in Tiles)
        {
            foreach (Tile t2 in Tiles)
            {
                if (t1.IsTileConnectable(t2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void GeneratePlaygroundRecursive(int minTiles, int maxTiles, Tile startTile, TileOrientation startingOrientation = TileOrientation.Undefined)
    {
        Tile currentTile = startTile;

        Tile previousTile;
        int placedTiles = 1, iterations = 1;
        bool isComplete = false;
        while (!isComplete && iterations < 100)
        {
            bool includeClosingTile = placedTiles > minTiles;
            bool forceClosingTile = placedTiles >= maxTiles;
            Tile selectedTile;
            if (placedTiles == 1)
            {
                // if you force the direction to connect the tile
                selectedTile = GetRandomConnectableTile(currentTile, includeClosingTile, forceClosingTile, startingOrientation);
            } else
            {
                selectedTile = GetRandomConnectableTile(currentTile, includeClosingTile, forceClosingTile);
            }
            Debug.Log($"Selected Tile {selectedTile.Name}");
            if (selectedTile != null)
            {
                previousTile = currentTile;
                currentTile = new Tile(selectedTile);
                Tile placedTile;
                if (placedTiles == 1)
                {
                    placedTile = previousTile.ConnectToTileAtOrientation(currentTile, startingOrientation);
                } else
                {
                    placedTile = previousTile.ConnectToTile(currentTile);
                }
                if (placedTile != null)
                {
                    placedTile.SetGameObjectName(placedTile.Name + "_" + GeneratedTiles.Count);
                    Debug.Log($"PLACED {placedTile.GetGameObjectName()} at position {placedTile.Position}");
                    if (GeneratedTiles.Exists(t => t.Position == placedTile.Position))
                    {
                        Debug.LogError($"Tile {placedTile.GetGameObjectName()} has the same position as {GeneratedTiles.Find(t => t.Position == placedTile.Position).GetGameObjectName()}. Fix it!");
                    }
                    GeneratedTiles.Add(placedTile);
                    placedTiles++;

                    // If you connect to a free spot of a branching tile, the branching tile won't appear in the
                    // ConnectedTiles list. To avoid this, check around if there are unkonwn connected tiles
                    CheckAroundTileForUnknownConnectedTiles(placedTile);

                    if (placedTile.IsClosingTile())
                    {
                        Debug.Log($"Placed closing tile {placedTile.Name}. Terminating subpath!");
                        break;
                    }

                    if (placedTile.IsFullyConnected())
                    {
                        Debug.Log($"Placed fully connected tile {placedTile.Name}. Terminating subpath!");
                        break;
                    }

                    if (placedTile.IsBranchingTile())
                    {
                        Debug.Log($"Tile {placedTile.GetGameObjectName()} is BRANCHING");
                        subPathCount += placedTile.Orientations.Count - 1;
                        foreach (TileOrientation newOrientation in placedTile.Orientations)
                        {
                            if (placedTile.IsOrientationAlreadyBusy(newOrientation) == false && IsPositionFree(placedTile.Position.AddOrientation(newOrientation)))
                            {
                                Debug.Log($"Starting new path from {placedTile.GetGameObjectName()}. Direction ''{newOrientation}''");
                                GeneratePlaygroundRecursive(
                                    Mathf.Clamp(minTiles - GeneratedTiles.Count, 0, minTiles - GeneratedTiles.Count),
                                    Mathf.Clamp(maxTiles - GeneratedTiles.Count, 1, maxTiles - GeneratedTiles.Count),
                                    placedTile,
                                    newOrientation
                                    );
                            }
                        }
                        break;
                    }
                }
                if (GeneratedTiles.Count > 10 * maxTiles)
                {
                    Debug.LogError("Placed too many tiles. Abort.");
                    break;
                }
            }
            iterations++;
        }
        Debug.Log($"Closing path started with {startTile.GetGameObjectName()} at orientation {startingOrientation}");
    }

    private void CheckAroundTileForUnknownConnectedTiles(Tile placedTile)
    {
        foreach (TileOrientation o in placedTile.Orientations)
        {
            if (placedTile.IsOrientationAlreadyBusy(o))
            {
                continue;
            }
            Tile nearTile = GetTileAtPosition(placedTile.Position.AddOrientation(o));
            if (nearTile != null)
            {
                TileOrientation oppositeOrientation = Tile.GetOppositeTileOrientation(o);
                if (nearTile.Orientations.Contains(oppositeOrientation) && nearTile.IsOrientationFree(oppositeOrientation))
                {
                    Tile.ConnectTwoAlreadyPlacedTiles(placedTile, nearTile);
                }
            }
        }
    }

    private Tile GetTileAtPosition(TilePosition position)
    {
        return GeneratedTiles.Find(t => t.Position == position);
    }

    private bool IsPositionFree(TilePosition tilePosition)
    {
        if (GeneratedTiles == null)
        {
            return true;
        }
        return GeneratedTiles.Exists(t => t.Position == tilePosition) == false;
    }

    public void GeneratePlayground()
    {
        if (Tiles.Count == 0)
        {
            Debug.LogError("No tiles to create playground");
            return;
        }
        //if (CheckIfTilesListIsConsistent() == false)
        //{
        //    Debug.LogError("None of these tiles can be connected");
        //    return;
        //}

        // Init spawn areas on template tiles
        foreach (Tile t in Tiles)
        {
            t.InitSpawnArea();
        }

        subPathCount = 0;
        Tile startTile = PlaceInitialTile();
        // Set seed
        if (GenerateRandomSeed)
        {
            seed = Random.seed;
        }
        Random.InitState(seed);
        //Start To Generate Dungeon
        if (GeneratedTiles == null)
        {
            GeneratedTiles = new List<Tile>();
        }

        GeneratePlaygroundRecursive(minTilesCount, maxTilesCount, startTile);

    }

    private Tile GetRandomConnectableTile(Tile tileToConnect, bool includeClosingTile, bool forceClosingTile, TileOrientation directionToConnect = TileOrientation.Undefined)
    {
        List<Tile> connectableTiles = new List<Tile>();
        foreach(Tile t in Tiles)
        {
            if (t.IsTileConnectable(tileToConnect))
            {

                if (directionToConnect != TileOrientation.Undefined && t.CanConnectToOrientation(directionToConnect) == false)
                {
                    Debug.Log($"Discarding {t} because the forced direction to connect is {directionToConnect}");
                    continue;
                }
                connectableTiles.Add(t);
            }
        }

        // We have ALL the tiles that can be connected basing on orientations only. Now let's filter considering the environment

        List<Tile> filteredTiles = new List<Tile>();

        foreach (Tile ctile in connectableTiles)
        {
            bool cTileIsOk = false;
            // Get all the possiblePosition of the connectable tile
            List<TilePosition> CTilePossiblePositions = ctile.GetAllPossiblePositionIfConnectingToTile(tileToConnect);
            if (directionToConnect != TileOrientation.Undefined)
            {
                CTilePossiblePositions = CTilePossiblePositions.FindAll(p => p == tileToConnect.Position.AddOrientation(directionToConnect));
            }
            foreach (TilePosition ctilePosition in CTilePossiblePositions)
            {
                // if the possible position is not free => discard tile
                if (IsPositionFree(ctilePosition) == false)
                {
                    continue;
                }

                // if the position next to the orientation aren't free (except for originating tile) => discard tile
                if (ArePositionsAtOrientationsFree(ctile, ctilePosition, originatingTile: tileToConnect) == false)
                {
                    continue;
                }


                // If the tile would BLOCK a branching tile => must be discarded
                // Ignore if the branching tile IS the originating tile :)
                if (DoesTileBlockABranchingTileIfPlacedAtPosition(ctile, ctilePosition, originatingTile: tileToConnect))
                {
                    continue;
                }
                // Here we speculate on the SUBSEQUENT tile, whether the previous examined is placed
                foreach (TileOrientation ctileOrientation in ctile.Orientations)
                {
                    if (ctilePosition.AddOrientation(ctileOrientation) == tileToConnect.Position)
                    {
                        continue;
                    }

                    TilePosition positionToCheck = ctilePosition.AddOrientation(ctileOrientation);

                    // If the tile could continue without being blocked by a present tile
                    if (IsPositionFree(positionToCheck))
                    {
                        cTileIsOk = true;
                        break;
                    }
                    
                    // If the SUBSEQUENT tile could be attached to an already present tile (clever!)
                    if (IsTileConnectableToAnotherPresentTileFromPosition(ctile, GeneratedTiles.Find(til => til.Position == positionToCheck), ctilePosition))
                    {
                        cTileIsOk = true;
                        break;
                    }
                }
                if (ctile.Orientations.Count == 1)
                {
                    // This means that the tile is a closing tile, so it won't lead to a new attached tile.
                    cTileIsOk = true;
                    break;
                }
                if (cTileIsOk)
                {
                    break;
                }
            }
            if (cTileIsOk)
            {
                filteredTiles.Add(ctile);
            }
        }
        connectableTiles = filteredTiles;

        // Filter closing tiles if possible
        if (connectableTiles.Exists(t => t.IsClosingTile() == false))
        {
            filteredTiles = new List<Tile>();
            foreach (Tile ctile in connectableTiles)
            {
                if (includeClosingTile == false && ctile.IsClosingTile())
                {
                    Debug.Log($"Discarding {ctile} as it's a closing tile");
                    continue;
                }
                filteredTiles.Add(ctile);
            }
            connectableTiles = filteredTiles;
        }

        // Filter branch tiles that shouldn't be placed if possible
        if (connectableTiles.Exists(t => t.IsBranchingTile() == false))
        {
            filteredTiles = new List<Tile>();
            foreach (Tile ctile in connectableTiles)
            {
                if (ctile.IsBranchingTile() && subPathCount >= maxSubpathCount)
                {
                    Debug.Log($"Discarding {ctile} as it's a branching tile and subpath is already {subPathCount} >= max value {maxSubpathCount}");
                    continue;
                }
                filteredTiles.Add(ctile);
            }
            connectableTiles = filteredTiles;
        }


        if (connectableTiles.Count == 0)
        {
            Debug.LogError($"No tiles found to be connected to {tileToConnect.Name}");
            return null;
        }

        // If you're forcing to get a tile from a specific direction
        if (directionToConnect != TileOrientation.Undefined)
        {
            //return tile to connect at the directionToConnect only
            connectableTiles = connectableTiles.FindAll(til => til.CanConnectToOrientation(directionToConnect));
        }

        // If your forcing to get closing tiles 
        if (forceClosingTile)
        {
            Debug.Log("Trying to force to get a closing tile!");
            List<Tile> closingTiles = connectableTiles.FindAll(til => til.IsClosingTile());
            if (closingTiles != null && closingTiles.Count > 0)
            {
                connectableTiles = closingTiles;
            }
        }
        return connectableTiles[Random.Range(0, connectableTiles.Count)];
    }

    private bool ArePositionsAtOrientationsFree(Tile tile, TilePosition tilePosition, Tile originatingTile)
    {
        foreach (TileOrientation o in tile.Orientations)
        {
            TilePosition positionToCheck = tilePosition.AddOrientation(o);
            Tile foundTile = GetTileAtPosition(positionToCheck);
            if (foundTile != null && foundTile != originatingTile && IsTileConnectableToAnotherPresentTileFromPosition(tile, foundTile, tilePosition) == false)
            {
                return false;
            }
        }
        return true;
    }

    private bool DoesTileBlockABranchingTileIfPlacedAtPosition(Tile tile, TilePosition position, Tile originatingTile)
    {
        List<TileOrientation> orientations = new List<TileOrientation>() { TileOrientation.East, TileOrientation.West, TileOrientation.South, TileOrientation.North };
        foreach (TileOrientation o in orientations)
        {
            Tile foundTile = GeneratedTiles.Find(t => t.Position == position.AddOrientation(o));
            if (foundTile != null && foundTile != originatingTile && foundTile.IsBranchingTile()) // We found a branching tile nearby
            {
                TileOrientation foundTileOrientation = Tile.GetOppositeTileOrientation(o);
                if (foundTile.HasOrientation(foundTileOrientation) && foundTile.IsOrientationAlreadyBusy(foundTileOrientation) == false) // The branching tile has a free spot
                {
                    if (IsTileConnectableToAnotherPresentTileFromPosition(tile, foundTile, position) == false)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
        
    }

    private bool IsTileConnectableToAnotherPresentTileFromPosition(Tile selectedTile, Tile presentTile, TilePosition selectedPosition)
    {
        TileOrientation presentTileOrientation = presentTile.GetOrientationFromOtherTilePosition(selectedPosition);

        // check: the present tile has the right orientation and it's not busy
        if (presentTile.Orientations.Contains(presentTileOrientation) == false || presentTile.IsOrientationAlreadyBusy(presentTileOrientation))
        {
            return false;
        }

        // check: the tile to add has the opposite orientation
        if (selectedTile.Orientations.Contains(Tile.GetOppositeTileOrientation(presentTileOrientation)) == false)
        {
            return false;
        }

        Debug.Log($"Tile {selectedTile} is connectable to already present {presentTile.GetGameObjectName()}!");
        return true;
    }

    private Tile PlaceInitialTile()
    {
        Tile t = new Tile(InitialTile);
        t.Place(new TilePosition() { x = 0, z = 0 });
        GeneratedTiles.Add(t);
        return t;
        
    }

    public void DestroyPlayground()
    {
        if (GeneratedTiles == null)
        {
            return;
        }
        foreach (Tile t in GeneratedTiles)
        {
            t.Destroy();
        }
        GeneratedTiles.Clear();
    }

    public List<Tile> GetGeneratedTiles()
    {
        return GeneratedTiles;
    }
}
