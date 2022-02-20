using System.Collections;
using System.Collections.Generic;

public class Node<T>
{
    public List<T> Neighbours { get; protected set; }

    public RoomType Type { get; protected set; }

    public int ID { get; private set; }
    public int Depth { get; protected set; } = -1;

    public Node(int ID)
    {
        this.ID = ID;

        Neighbours = new List<T>();
        Type = RoomType.Null;
    }

    public void AddNeighbour(T neighbour)
    {
        if (!Neighbours.Contains(neighbour))
        {
            Neighbours.Add(neighbour);
        }
    }

    public void SetRoomType(RoomType Type)
    {
        this.Type = Type;
    }

    public void SetDepth(int Depth)
    {
        this.Depth = Depth;
    }
}

class UnrootedNode : Node<UnrootedNode>
{
    public UnrootedNode(int ID) : base(ID) { }
}

public class RootedNode : Node<RootedNode>
{
    public List<RootedNode> Childs
    {
        get { return Neighbours; }
        private set { Neighbours = value; }
    }
    public RootedNode Parent { get; private set; }

    public RootedNode(RootedNode Parent, int ID) : base(ID)
    {
        this.Parent = Parent;

        Childs = new List<RootedNode>();
    }

    public void AddChild(RootedNode child)
    {
        AddNeighbour(child);
    }

    public void RemoveChild(RootedNode child)
    {
        Neighbours.Remove(child);
    }

    public void SetParent(RootedNode Parent)
    {
        this.Parent = Parent;
    }
}

public class RoomNode : Node<RoomNode>
{

    public int Width { get; private set; }
    public int Height { get; private set; }

    public int Top { get; private set; }
    public int Bottom { get; private set; }
    public int Left { get; private set; }
    public int Right { get; private set; }

    public TileType[,] TileMap { get; private set; }

    //public List<Entry> Entries { get; private set; }
    public List<Coord> Floor { get; private set; }
    public List<Coord> Loot { get; private set; }
    public List<Coord> Chest { get; private set; }
    public List<Coord> InterestingPlaces { get; private set; }
    public List<List<Coord>> Enemies { get; private set; }

    public Coord Position { get; private set; }

    public RoomNode(int ID, RoomType Type) : base(ID)
    {
        this.Type = Type;
    }

    public RoomNode(RootedNode node) : base(node.ID)
    {
        Type = node.Type;
        Depth = node.Depth;
    }

    public void SetDimensions(int width, int height)
    {
        Width = width;
        Height = height;

        SetWorldPosition(new Coord(0, 0));
    }

    public void Generate(TileType[,] tileMap, List<Coord> floor
        /*,List<Coord> loot, List<Coord> chest,
        List<Coord> interestingPlaces, List<List<Coord>> enemies*/)
    {
        TileMap = tileMap;

        Floor = floor;

        SetWorldPosition(new Coord(0, 0));

        /*Loot = loot;
        Chest = chest;
        InterestingPlaces = interestingPlaces;
        Enemies = enemies;*/
    }

    public void SetWorldPosition(Coord position)
    {
        Position = position;

        Left = position.x;
        Right = position.x + Width;
        Top = position.y;
        Bottom = position.y - Height;
    }

    public bool OverlapsRoom(RoomNode another)
    {
        return OverlapsValueX(another.Left, another.Right) && OverlapsValueY(another.Bottom, another.Top);
    }

    bool OverlapsValueX(int min, int max)
    {
        return Right > min && max > Left;
    }

    bool OverlapsValueY(int min, int max)
    {
        return Top > min && max > Bottom;
    }
}
