﻿using System.Collections;
using System.Collections.Generic;

public class Node<T>
{
    public List<T> Neighbours { get; protected set; }

    public RoomType Type { get; private set; }

    public int ID { get; private set; }
    public int Depth { get; private set; } = -1;

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
    public RoomNode(int ID) : base(ID) { }

}
