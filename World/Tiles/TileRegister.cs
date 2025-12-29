using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MineGameB.World.Tiles;
public class TileRegister {
    Dictionary<int, Tile> archive = [];
    public int Count() => archive.Count;

    int next = 0;
    public Tile Register(Tile t, int? id=null) {
        int finalId = id ?? next++;
        archive.Add(finalId, t.SetId(finalId));
        return GetTileById(finalId);
    }

    public Tile GetTileById(int id) {
        return archive[id];
    }
    public Tile GetTileByName(string name) {
        foreach (var tile in archive.Values) {
            if (tile.Name == name)
                return tile;
        }
        return null;
    }
    public int GetIdByName(string name) {
        foreach (var kvp in archive) {
            if (kvp.Value.Name == name)
                return kvp.Key;
        }
        return -1;
    }
    public string GetNameById(int id) {
        return archive[id].Name;
    }
}
