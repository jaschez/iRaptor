using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Room
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Coord Position { get; private set; }

    public int ID { get; private set; }

    public List<Entry> Entries { get; private set; }
    public List<Coord> Floor { get; private set; }
    public List<Coord> Loot { get; private set; }
    public List<Coord> Chest { get; private set; }
    public List<Coord> InterestingPlaces { get; private set; }
    public List<List<Coord>> Enemies { get; private set; }
    public List<int> LinkedRooms { get; private set; }
    public List<int> LinkedCorridors { get; private set; }
    public RoomType Type { get; private set; }

    public Room(RoomType type, int width, int height, Coord position, int id, List<Entry> entries, List<Coord> floor,
        List<Coord> loot, List<Coord> chest, List<Coord> interestingPlaces, List<List<Coord>> enemies,
        List<int> linkedRooms, List<int> linkedCorridors)
    {
        Width = width;
        Height = height;
        Position = position;
        ID = id;
        Entries = entries;
        Floor = floor;
        Loot = loot;
        Chest = chest;
        InterestingPlaces = interestingPlaces;
        Enemies = enemies;
        LinkedRooms = linkedRooms;
        LinkedCorridors = linkedCorridors;
        Type = type;
    }

    public struct Entry
    {
        public Coord coord;
        public EntryType type;

        public int LinkedCorridor;

        public Entry(Coord c, EntryType e, int linkedCorridor)
        {
            coord = c;
            type = e;
            LinkedCorridor = linkedCorridor;
        }
    }

    public enum EntryType
    {
        Horizontal,
        Vertical,
        Central
    }

    public enum RoomType
    {
        Normal
    }
}
