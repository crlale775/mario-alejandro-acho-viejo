using UnityEngine;

/* Nivel 1-4: castillo de Bowser de diseno original. Fosos de lava, barras de
   fuego, puente sobre lava y el jefe final defendiendo el hacha. */
public static class LevelCastle
{
    public const float Length = 202f;
    public const float CameraHeight = 5.5f;

    public static void Build(LevelBuilder b)
    {
        b.length = Length;

        // techo del castillo
        b.RowSprite("CastleBrick", 0, 201, 9);

        // suelo con fosos de lava
        b.GroundSprite("CastleGround", 0, 35, 0);
        b.LavaPool(36, 40, -1);
        b.GroundSprite("CastleGround", 41, 71, 0);
        b.LavaPool(72, 77, -1);
        b.GroundSprite("CastleGround", 78, 103, 0);
        b.LavaPool(104, 109, -1);
        b.GroundSprite("CastleGround", 110, 151, 0);

        // arena final: lava bajo el puente y plataforma del hacha
        b.LavaPool(152, 173, -1);
        b.GroundSprite("CastleGround", 174, 201, 0);

        // ------------------------- decoracion ---------------------------
        for (int x = 4; x < 196; x += 14)
        {
            b.SpriteObject("CastleWindow", x + 0.5f, 5.5f, -7);
            b.SpriteObject("Chain", x + 4.5f, 7.5f, -6);
        }

        int[] torches = { 8, 22, 44, 60, 82, 96, 122, 140, 158, 186 };

        foreach (int x in torches) {
            b.AnimatedDecoration(x, 3.5f, new[] { "Torch1", "Torch2" }, 0.12f, -3);
        }

        // columnas decorativas del fondo (sin colision)
        for (int x = 12; x < 150; x += 28) {
            b.SpriteObject("CastleBrick", x + 0.5f, 5f, -8, false, true, 1f, 8f);
        }

        // --------------------------- peligros ---------------------------
        b.FireBar(58, 6, 7, 115f);
        b.FireBar(96, 6, 7, -100f);
        b.FireBar(136, 7, 6, 130f);

        // --------------------------- enemigos ---------------------------
        b.Koopa(20, 0);
        b.Goomba(30, 0);
        b.Koopa(50, 0);
        b.Goomba(64, 0);
        b.Goomba(90, 0);
        b.Koopa(100, 0);
        b.Goomba(120, 0);
        b.Koopa(132, 0);
        b.Goomba(145, 0);

        // bloques de ayuda
        b.BrickRow(24, 28, 4);
        b.Mystery(26, 4, "MagicMushroom");
        b.BrickRow(86, 90, 4);
        b.Mystery(88, 4, "BlockCoin");
        b.Mystery(146, 4, "MagicMushroom");
        b.CoinRow(60, 66, 6);
        b.CoinRow(112, 118, 5);

        // ------------------------- puente y jefe ------------------------
        BridgeCollapse bridge = b.Bridge();

        for (int x = 152; x <= 173; x++) {
            b.BridgePlank(x, 0, bridge);
        }

        BowserBoss bowser = b.Bowser(166, 0, 156f, 171f);
        bridge.bowser = bowser;

        b.Axe(180, 0, bridge);

        b.DeathBarrier(-5f);

        b.SpawnPlayer(3, 0);
    }

}
