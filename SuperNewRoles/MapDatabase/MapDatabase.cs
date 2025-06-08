using System.Collections.Generic;
using System.IO;
using AmongUs.GameOptions;
using SuperNewRoles.MapDatabase.Database;
using UnityEngine;

// https://github.com/Dolly1016/Nebula-Public/blob/master/NebulaPluginNova/Map/MapData.csより

namespace SuperNewRoles.MapDatabase;

public abstract class MapDatabase
{
    abstract protected Vector2[] MapArea { get; }
    abstract protected Vector2[] NonMapArea { get; }
    abstract protected SystemTypes[] SabotageTypes { get; }

    public SystemTypes[] GetSabotageSystemTypes() => SabotageTypes;
    public bool CheckMapArea(Vector2 position)
    {
        int num = Physics2D.OverlapCircleNonAlloc(position, 0.1f, PhysicsHelpers.colliderHits, Constants.ShipAndAllObjectsMask);
        if (num > 0 && num <= PhysicsHelpers.colliderHits.Length)
        {
            for (int i = 0; i < num; i++)
            {
                if (!PhysicsHelpers.colliderHits[i].isTrigger) return false;
            }
        }

        return CheckMapAreaInternal(position);
    }

    public bool CheckMapAreaInternal(Vector2 position)
    {
        Vector2 vector;
        float magnitude;

        foreach (Vector2 p in NonMapArea)
        {
            vector = p - position;
            magnitude = vector.magnitude;
            if (magnitude > 6.0f) continue;

            if (!PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndAllObjectsMask)) return false;
        }

        foreach (Vector2 p in MapArea)
        {
            vector = p - position;
            magnitude = vector.magnitude;
            if (magnitude > 12.0f) continue;

            if (!PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndAllObjectsMask)) return true;
        }

        return false;
    }

    public int CheckMapAreaDebug(Vector2 position)
    {
        Vector2 vector;
        float magnitude;
        int count = 0;

        foreach (Vector2 p in NonMapArea)
        {
            vector = p - position;
            magnitude = vector.magnitude;
            if (magnitude > 6.0f) continue;

            if (!PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndAllObjectsMask)) return 0;
        }

        foreach (Vector2 p in MapArea)
        {
            vector = p - position;
            magnitude = vector.magnitude;
            if (magnitude > 12.0f) continue;

            if (!PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndAllObjectsMask)) count++;
        }

        return count;
    }

    private static Texture2D CreateReadableTexture(Texture texture, int margin = 0)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);
        Texture2D readableTexture2D = null;
        try
        {
            Graphics.Blit(texture, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            readableTexture2D = new Texture2D(texture.width + margin * 2, texture.height + margin * 2);
            readableTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), margin, margin);
            readableTexture2D.Apply();
            RenderTexture.active = previous;
        }
        finally
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        return readableTexture2D;
    }
    public Texture2D OutputMap(Vector2 center, Vector2 size, float resolution = 10f)
    {
        int x1, y1, x2, y2;
        x1 = (int)((center.x - size.x * 0.5f) * resolution);
        y1 = (int)((center.y - size.y * 0.5f) * resolution);
        x2 = (int)((center.x + size.x * 0.5f) * resolution);
        y2 = (int)((center.y + size.y * 0.5f) * resolution);
        int temp;
        if (x1 > x2)
        {
            temp = x1;
            x1 = x2;
            x2 = temp;
        }
        if (y1 > y2)
        {
            temp = y1;
            y1 = y2;
            y2 = temp;
        }

        Color color = new Color(40 / 255f, 40 / 255f, 40 / 255f);
        var texture = new Texture2D(x2 - x1, y2 - y1, TextureFormat.RGB24, false);

        int num;
        int r = 0;
        for (int y = y1; y < y2; y++)
        {
            for (int x = x1; x < x2; x++)
            {
                num = CheckMapAreaDebug(new Vector2(((float)x) / resolution, ((float)y) / resolution));
                texture.SetPixel(x - x1, y - y1, (num == 0) ? color : new Color((num > 1 ? 100 : 0) / 255f, (150 + (num * 5)) / 255f, 0));
                if (num > 0) r++;
            }
        }

        texture.Apply();

        return CreateReadableTexture(texture);
    }

    static private MapDatabase[] AllMapData = new MapDatabase[] { new SkeldData(), new MiraData(), new PolusData(), null!, new AirshipData(), new FungleData() };
    static public MapDatabase GetCurrentMapData() => AllMapData.Length > currentMapId ? AllMapData[currentMapId] : null!;
    static int currentMapId => GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId);
}
