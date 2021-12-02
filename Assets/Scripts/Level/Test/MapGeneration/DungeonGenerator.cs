﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonGenerator
{

    int scaleFactor = 3;

    int width, height;

    List<Room> rooms;
    List<Corridor> corridors;
    List<Room> failedRooms;

    Coord[] directions = { new Coord(1, 0), new Coord(-1, 0), new Coord(0, 1), new Coord(0, -1) };

    int[,] dungeonMap;

    int minRooms = 10;
    int maxRooms = 12;

    int minRoomSize = 8;
    int maxRoomSize = 10;

    int maxLoot = 3;

    int tileSize = 16;

    public bool randomSeed = true;

    public int seed = 0;

    public MapInfo CreateMap()
    {

        int randomValue = Random.Range(int.MinValue, int.MaxValue);

        if (seed == 0)
        {
            seed = randomValue;
        }

        Random.InitState(seed);

        GenerateMap();
        SetMapBounds();

        GenerateTiles();

        Room[] roomsByUniformity;
        Room firstRoom = rooms[0];
        
        List<Coord> loots = new List<Coord>();
        List<Coord> enemies = new List<Coord>();
        Coord player;
        Coord exit;

        int bossRoom = rooms.Count - 1;

        //Place loot points
        roomsByUniformity = rooms.OrderBy(room => room.interestingPoints.Count).ToArray();   //Order by uniformity

        int placedLoot = 0;

        for (int i = 0; placedLoot < maxLoot && i < roomsByUniformity.Length; i++)
        {
            Room interestingRoom = roomsByUniformity[i];

            if (interestingRoom.GetId() != 0) {

                foreach (Coord lootCoord in interestingRoom.lootPoints)
                {
                    loots.Add(RoomToWorldCoord(lootCoord, interestingRoom));
                }

                placedLoot++;
            }
        }

        //Place Player Coord

        player = RoomToWorldCoord(firstRoom.startPoints[1], firstRoom);

        //Place Exit Coord

        exit = RoomToWorldCoord(rooms[bossRoom].floorCoords[Random.Range(0, rooms[bossRoom].floorCoords.Count)], rooms[bossRoom]);

        //Place enemy Coords

        foreach (Room room in rooms)
        {
            foreach (Coord c in room.enemyCoords)
            {
                enemies.Add(RoomToWorldCoord(c, room));
            }
        }

        MapInfo mapInfo = new MapInfo
        {
            width = width,
            height = height,
            tileSize = tileSize,
            rooms = rooms,
            corridors = corridors,
            mapSeed = seed,
            map = dungeonMap,
            lootCoords = loots,
            PlayerCoord = player,
            enemyCoords = enemies,
            ExitPos = exit
        };


        return mapInfo;
    }

    void GenerateMap()
    {

        rooms = new List<Room>();
        corridors = new List<Corridor>();
        failedRooms = new List<Room>();

        int roomNum = Random.Range(minRooms, maxRooms + 1);
        int sameDir = 0;

        Room lastRoom = null;

        Coord lastDir = new Coord();

        for (int i = 0; i < roomNum - 1; i++)
        {

            int roomWidth;
            int roomHeight;

            //Calculate room dimensions

            if (i == 0)
            {
                roomWidth = Random.Range(4, 6);
                roomHeight = Random.Range(4, 6);
            }
            else
            {
                roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
                roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            }

            //Calculate direction

            Coord dir;

            do
            {
                dir = directions[ChooseDirection()];

                if (!lastDir.Equals(dir))
                {
                    sameDir = 0;
                }
            } while ((dir.x == -lastDir.x && dir.y == -lastDir.y) || sameDir > 2);

            if (lastDir.Equals(dir))
            {
                sameDir++;
            }

            Room generated = GenerateRoom(dir, roomWidth, roomHeight, lastRoom, i, true);

            //If room is new and not created prevoiusly
            if (generated.GetId() == i)
            {
                lastDir = dir;
            }
            else
            {
                lastDir = new Coord();
                i--;
            }

            lastRoom = generated;
        }

        //TODO: Obtener la cueva de mayor distancia respecto al inicio

        int largestIndex = 0;
        int largestDistance = 0;

        for (int index = 0; index < rooms.Count; index++)
        {

            int dist = rooms[index].distanceFromStart;

            if(dist > largestDistance)
            {
                largestDistance = dist;

                largestIndex = index;
            }
        }

        Room bossRoom = null;

        while (bossRoom == null)
        {

            Coord direction = directions[ChooseDirection()];

            int width = Random.Range(10, 15);
            int height = Random.Range(10, 15);

            bossRoom = GenerateRoom(direction, width, height, rooms[largestIndex], roomNum - 1, false, .85f);
            //Y si nunca encuentra el sitio apropiado?
        }

        foreach (Room r in rooms)
        {
            r.CreateRoom();
        }
    }

    Room GetNearestOverlappingRoom(Room evaluated)
    {

        Room nearest = null;

        float nearestDistance = int.MaxValue;

        foreach (Room room in rooms)
        {
            if (room.GetId() >= 0)
            {
                if (evaluated.OverlapsRoom(room))
                {
                    //Get distance between rooms
                    Coord A = new Coord((evaluated.left + evaluated.right) / 2, (evaluated.top + evaluated.bottom) / 2);
                    Coord B = new Coord((room.left + room.right) / 2, (room.top + room.bottom) / 2);

                    Coord result = new Coord(A.x - B.x, A.y - B.y);

                    float distance = result.x * result.x + result.y * result.y;

                    if (distance < nearestDistance)
                    {
                        nearest = room;
                        nearestDistance = distance;
                    }
                }
            }
        }

        return nearest;
    }

    Room GenerateRoom(Coord dir, int roomWidth, int roomHeight, Room lastRoom, int id, bool linkIfOverlaps, float fillPerc = -1)
    {
        //Calculate spacing between rooms

        Coord roomPos = CalculateRoomPosition(dir, roomWidth, roomHeight, lastRoom);

        Room newRoom = new Room(roomPos, roomWidth, roomHeight, scaleFactor, id, fillPerc);
        Room overlapped = GetNearestOverlappingRoom(newRoom);

        if (overlapped == null)
        {

            //Link one room to the other
            if (lastRoom != null)
            {
                newRoom.distanceFromStart = lastRoom.distanceFromStart + 1;

                LinkRooms(newRoom, lastRoom);
            }

            //newRoom.CreateRoom(); Debug purposes

            rooms.Add(newRoom);

            return newRoom;
        }
        else
        {
            if (linkIfOverlaps) {

                //No se crea nueva habitacion, pero si una nueva conexion
                //Siempre y cuando no exista la conexion en ningun sentido, se crea

                //Link one room to the other
                if (!overlapped.IsLinkedToRoomIndex(lastRoom.GetId()) && overlapped.GetId() >= 0)
                {

                    //Actualizar distancias

                    if (overlapped.distanceFromStart != lastRoom.distanceFromStart)
                    {
                        int nextDist = Mathf.Min(overlapped.distanceFromStart, lastRoom.distanceFromStart) + 1;

                        if (overlapped.distanceFromStart > lastRoom.distanceFromStart)
                        {
                            overlapped.distanceFromStart = nextDist;
                        }
                        else
                        {
                            lastRoom.distanceFromStart = nextDist;
                        }
                    }

                    LinkRooms(overlapped, lastRoom);
                }

                /*newRoom.SetId(-i);
                failedRooms.Add(newRoom);*/

                return overlapped;
            }

            return null;
        }
    }

    Coord CalculateRoomPosition(Coord dir, int roomWidth, int roomHeight, Room lastRoom)
    {
        int stepX;
        int stepY;

        if (dir.x != 0)
        {
            stepX = dir.x * Random.Range(3, 5);
        }
        else
        {
            stepX = Random.Range(-5, 5);
        }

        if (dir.y != 0)
        {
            stepY = dir.y * Random.Range(3, 5);
        }
        else
        {
            stepY = Random.Range(-5, 5);
        }

        int roomPosX = 0;
        int roomPosY = 0;

        if (lastRoom != null)
        {
            int startX;
            int startY;

            if (dir.x > 0)
            {
                startX = lastRoom.right;
            }
            else if (dir.x < 0)
            {
                startX = lastRoom.left - roomWidth * scaleFactor;
            }
            else
            {
                startX = lastRoom.left;
            }

            if (dir.y > 0)
            {
                startY = lastRoom.top + roomHeight * scaleFactor;
            }
            else if (dir.y < 0)
            {
                startY = lastRoom.bottom;
            }
            else
            {
                startY = lastRoom.top;
            }

            roomPosX = startX + stepX;
            roomPosY = startY + stepY;
        }

        return new Coord(roomPosX, roomPosY);
    }

    int ChooseDirection()
    {
        int p = Random.Range(0, 100);

        if (p > 75)
        {
            return 0;
        }
        else if (p > 50)
        {
            return 1;
        }
        else if (p > 25)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    void LinkRooms(Room roomA, Room roomB)
    {
        roomA.LinkToRoomIndex(roomB.GetId());
        roomB.LinkToRoomIndex(roomA.GetId());

        Coord pointA = new Coord();
        Coord pointB = new Coord();

        Room.EntryType typeA;
        Room.EntryType typeB;

        //Generate corridor points between rooms

        if (roomA.OverlapsValueX(roomB.left, roomB.right))
        {
            typeA = Room.EntryType.Horizontal;
            typeB = Room.EntryType.Horizontal;

            int overlapMinX = Mathf.Max(roomA.left, roomB.left);
            int overlapMaxX = Mathf.Min(roomA.right - 1, roomB.right - 1);

            pointA.x = Random.Range(overlapMinX, overlapMaxX);

            if (roomA.top > roomB.top)
            {
                pointA.y = roomA.bottom + 1;
                pointB.y = roomB.top;
            }
            else
            {
                pointA.y = roomA.top;
                pointB.y = roomB.bottom + 1;
            }

            int differenceY = Mathf.Abs(pointA.y - pointB.y);
            int xRanged = Random.Range(pointA.x - differenceY, pointA.x + differenceY);

            pointA.x = Mathf.Clamp(pointA.x, roomA.left + 2, roomA.right - 2);
            pointB.x = Mathf.Clamp(xRanged, roomB.left + 2, roomB.right - 2);

        }
        else if (roomA.OverlapsValueY(roomB.bottom, roomB.top))
        {
            typeA = Room.EntryType.Vertical;
            typeB = Room.EntryType.Vertical;

            int overlapMinY = Mathf.Max(roomA.bottom + 1, roomB.bottom + 1);
            int overlapMaxY = Mathf.Min(roomA.top, roomB.top);

            pointA.y = Random.Range(overlapMinY, overlapMaxY);

            if (roomA.left > roomB.left)
            {
                pointA.x = roomA.left;
                pointB.x = roomB.right - 1;
            }
            else
            {
                pointA.x = roomA.right - 1;
                pointB.x = roomB.left;
            }

            int differenceX = Mathf.Abs(pointA.x - pointB.x);
            int yRanged = Random.Range(pointA.y - differenceX, pointA.y + differenceX);

            pointA.y = Mathf.Clamp(pointA.y, roomA.bottom + 2, roomA.top - 2);
            pointB.y = Mathf.Clamp(yRanged, roomB.bottom + 2, roomB.top - 2);

        }
        else
        {

            //Cómo decidimos si se conecta por arriba o por abajo???

            if (Random.Range(0, 10) > 5)
            {
                typeA = Room.EntryType.Vertical;
                typeB = Room.EntryType.Horizontal;

                //Esquina con forma 1
                if (roomA.left > roomB.left)
                {
                    pointA.x = roomA.left;
                    pointB.x = roomB.right - 4;
                }
                else
                {
                    pointA.x = roomA.right - 1;
                    pointB.x = roomB.left + 4;
                }

                //pointB.x = Random.Range(roomB.left, roomB.right);

                if (roomB.top > roomA.top)
                {
                    pointA.y = roomA.top - 4;
                    pointB.y = roomB.bottom + 1;
                }
                else
                {
                    pointA.y = roomA.bottom + 4;
                    pointB.y = roomB.top;
                }

                //pointA.y = Random.Range(roomA.top, roomA.bottom);
            }
            else
            {
                typeA = Room.EntryType.Horizontal;
                typeB = Room.EntryType.Vertical;

                //Esquina con forma 2
                if (roomB.left > roomA.left)
                {
                    pointA.x = roomA.right - 4;
                    pointB.x = roomB.left;
                }
                else
                {
                    pointA.x = roomA.left + 4;
                    pointB.x = roomB.right - 1;
                }

                //pointA.x = Random.Range(roomA.left, roomA.right);

                if (roomA.top > roomB.top)
                {
                    pointA.y = roomA.bottom + 1;
                    pointB.y = roomB.top - 4;
                }
                else
                {
                    pointA.y = roomA.top;
                    pointB.y = roomB.bottom + 4;
                }

                //pointB.y = Random.Range(roomB.top, roomB.bottom);
            }
        }

        roomA.AddEntry(pointA, typeA, corridors.Count);
        roomB.AddEntry(pointB, typeB, corridors.Count);

        Corridor newCorridor = new Corridor(pointA, pointB);
        newCorridor.Generate();

        corridors.Add(newCorridor);
    }

    void SetMapBounds()
    {
        int rightCaveIndex = -1;
        int bottomCaveIndex = -1;

        int leftierValue = int.MaxValue;
        int rightierValue = int.MinValue;
        int upperValue = int.MinValue;
        int deeperValue = int.MaxValue;

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];

            if (room.left < leftierValue)
            {
                leftierValue = room.left;
            }

            if (room.top > upperValue)
            {
                upperValue = room.top;
            }

            if (room.bottom < deeperValue)
            {
                deeperValue = room.bottom;
                bottomCaveIndex = i;
            }

            if (room.right > rightierValue)
            {
                rightierValue = room.right;
                rightCaveIndex = i;
            }
        }

        foreach (Room room in rooms)
        {
            Coord roomPos = room.GetWorldPosition();

            roomPos.x -= leftierValue;
            roomPos.y -= upperValue;

            room.SetWorldPosition(roomPos);
        }

        foreach (Corridor c in corridors)
        {
            c.ShiftCorridor(-leftierValue, -upperValue);
        }

        //DEBUG
        foreach (Room room in failedRooms)
        {
            Coord roomPos = room.GetWorldPosition();

            roomPos.x -= leftierValue;
            roomPos.y -= upperValue;

            room.SetWorldPosition(roomPos);
        }

        width = rooms[rightCaveIndex].right;
        height = Mathf.Abs(rooms[bottomCaveIndex].bottom);
    }

    void GenerateTiles()
    {
        dungeonMap = new int[width, height];

        foreach (Room r in rooms)
        {
            int w = r.gameWidth;
            int h = r.gameHeight;

            Coord worldPos = r.GetWorldPosition();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (r.gameMap[i, j] == 0)
                    {
                        dungeonMap[worldPos.x + i, Mathf.Abs(worldPos.y) + j] = 1;
                    }
                    else
                    {
                        if (r.IsEdge(i, j))
                        {
                            dungeonMap[worldPos.x + i, Mathf.Abs(worldPos.y) + j] = 2;
                        }
                    }
                }
            }
        }

        foreach (Corridor corridor in corridors)
        {
            foreach (Coord c in corridor.Coords)
            {
                if (c.x >= 0 && Mathf.Abs(c.y) >= 0 && c.x < width && Mathf.Abs(c.y) < height)
                {
                    dungeonMap[c.x, Mathf.Abs(c.y)] = 1;
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (dungeonMap[i, j] == 0)
                {
                    if (IsEdge(i, j, 1))
                    {
                        dungeonMap[i, j] = 2;
                    }
                }
                else if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    dungeonMap[i, j] = 2;
                }
            }
        }
    }

    bool IsEdge(int x, int y, int searchingTile)
    {
        for (int xOff = x - 1; xOff <= x + 1; xOff++)
        {
            for (int yOff = y - 1; yOff <= y + 1; yOff++)
            {
                if (xOff >= 0 && xOff < width && yOff >= 0 && yOff < height)
                {
                    if ((xOff == x || yOff == y) && !(xOff == x && yOff == y))
                    {
                        if (dungeonMap[xOff, yOff] == searchingTile)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (dungeonMap[x, y] == 1)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    Coord RoomToWorldCoord(Coord local, Room room)
    {
        Coord world = new Coord(local.x, local.y);

        world.x += room.GetWorldPosition().x;
        world.y -= room.GetWorldPosition().y;

        return world;
    }
}

public struct MapInfo
{
    public int width;
    public int height;

    public int tileSize;

    public int[,] map;

    public int mapSeed;

    public Coord PlayerCoord;
    public Coord ExitPos;

    public List<Room> rooms;
    public List<Corridor> corridors;

    public List<Coord> enemyCoords;
    public List<Coord> lootCoords;
    public List<Coord> eggCoords;
}

public struct Coord
{
    public int x, y;

    public Coord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
