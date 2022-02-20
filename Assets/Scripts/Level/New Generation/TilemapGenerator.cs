using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TilemapGenerator : MonoBehaviour
{
    static TilemapGenerator instance;

    public Tilemap tilemap;

    public TileSource[] TileSources;

    Dictionary<TileType, TileBase> Tiles;

    List<RoomNode> debugRooms;

    private void Awake()
    {
        Tiles = new Dictionary<TileType, TileBase>();

        foreach (TileSource source in TileSources)
        {
            if (!Tiles.ContainsKey(source.Type))
            {
                Tiles.Add(source.Type, source.TileResource);
            }
        }

        if (instance == null)
        {
            instance = this;
        }
    }

    public void LoadLevel(List<RoomNode> rooms)
    {
        debugRooms = rooms;

        tilemap.ClearAllTiles();

        //Capa de bloques de cueva
        foreach (RoomNode room in rooms)
        {
            Coord roomPosition = room.Position;

            for (int i = 0; i < room.Width; i++)
            {
                for (int j = 0; j < room.Height; j++)
                {
                    
                    Vector3Int tilePosition = new Vector3Int(roomPosition.x + i, roomPosition.y + j, 1);
                    TileBase tile = Tiles[room.TileMap[i, j]];

                    tilemap.SetTile(tilePosition, tile);
                }
            }
        }
    }

    public static TilemapGenerator GetInstance()
    {
        return instance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        int tileSize = 16;



        Gizmos.color = Color.red;

        //UNIONES ENTRE HABITACIONES

        if (debugRooms != null)
        {
            foreach (RoomNode room in debugRooms)
            {
                Vector3 a = new Vector3(room.Position.x + room.Width/2, room.Position.y + room.Height / 2, 1) * tileSize;
                foreach (RoomNode child in room.Neighbours)
                {
                    Vector3 b = new Vector3(child.Position.x + child.Width / 2, child.Position.y - child.Height / 2, 1) * tileSize;

                    Gizmos.DrawLine(a, b);
                }
            }
        }

        Gizmos.color = Color.cyan;
    }

    [System.Serializable]
    public struct TileSource
    {
        public TileBase TileResource;
        public TileType Type;

        public TileSource(TileBase tileResource, TileType tileType)
        {
            TileResource = tileResource;
            Type = tileType;
        }
    }
}