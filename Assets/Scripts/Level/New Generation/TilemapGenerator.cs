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

    List<List<RoomNode>> debugRooms;

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

    public void LoadLevel(List<List<RoomNode>> composites)
    {
        debugRooms = composites;

        tilemap.ClearAllTiles();

        //Capa de bloques de cueva

        int space = -1000;

        foreach (List <RoomNode> comp in composites)
        {
            space += 200;
            foreach (RoomNode room in comp)
            {
                Coord roomPosition = room.Position;

                for (int i = 0; i < room.Width; i++)
                {
                    for (int j = 0; j < room.Height; j++)
                    {

                        Vector3Int tilePosition = new Vector3Int(roomPosition.x + i, roomPosition.y - j, 1);
                        TileBase tile = Tiles[room.TileMap[i, j]];

                        tilemap.SetTile(tilePosition, tile);
                    }
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

        List<RoomNode> evaluated = new List<RoomNode>();

        if (debugRooms != null)
        {
            foreach (List<RoomNode> comp in debugRooms)
            {
                foreach (RoomNode room in comp)
                {
                    Vector3 a = new Vector3(room.Position.x + room.Width / 2, room.Position.y - room.Height / 2, 1) * tileSize;

                    Gizmos.color = Color.cyan;
                    for (int i = 0; i < room.ID;i++) {
                        Gizmos.DrawSphere(a+Vector3.right*i*3, 10);
                    }
                    foreach (RoomNode child in room.Neighbours)
                    {
                        if (!evaluated.Contains(child))
                        {
                            Vector3 b = new Vector3(child.Position.x + child.Width / 2, child.Position.y - child.Height / 2, 1) * tileSize;
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(a, b);
                        }
                    }

                    evaluated.Add(room);
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