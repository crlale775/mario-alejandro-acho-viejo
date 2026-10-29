using UnityEngine;

/* Nivel 1-3: fondo marino con nado, corales, algas y peces enemigos. */
public static class LevelWater
{
    public const float Length = 192f;
    public const float CameraHeight = 6.5f;

    private static int FloorTop(int x)
    {
        if (x >= 24 && x <= 40) return 1;
        if (x >= 78 && x <= 96) return 2;
        if (x >= 130 && x <= 146) return 1;
        return 0;
    }

    public static void Build(LevelBuilder b)
    {
        b.length = Length;

        // fondo marino
        b.GroundSprite("SeaGround", 0, 191, 0);
        b.GroundSprite("SeaGround", 24, 40, 1);
        b.GroundSprite("SeaGround", 78, 96, 2);
        b.GroundSprite("SeaGround", 130, 146, 1);

        // superficie del agua (decorativa)
        for (int x = -2; x < 192; x++) {
            b.SpriteObject("WaterSurface", x + 0.5f, 11.6f, -6);
        }

        // ---------------------------- decoracion -------------------------
        int[] corals = { 5, 14, 27, 34, 46, 55, 63, 82, 89, 101, 108, 118, 134, 141, 152, 160, 171, 182 };

        for (int i = 0; i < corals.Length; i++)
        {
            int x = corals[i];
            b.SpriteObject(i % 2 == 0 ? "Coral1" : "Coral2", x + 0.5f, FloorTop(x) + 1.5f, -1);
        }

        for (int x = 3; x < 190; x += 7) {
            b.SpriteObject(x % 2 == 0 ? "Seaweed1" : "Seaweed2", x + 0.5f, FloorTop(x) + 1.6f, -2);
        }

        // ---------------------------- plataformas ------------------------
        b.RowSprite("SeaBrick", 10, 16, 4);
        b.CoinRow(10, 16, 5);

        b.RowSprite("SeaBrick", 20, 26, 6);
        b.CoinRow(21, 25, 7);

        b.RowSprite("SeaBrick", 44, 50, 5);
        b.CoinRow(44, 50, 6);

        b.RowSprite("SeaBrick", 56, 62, 7);
        b.CoinRow(57, 61, 8);

        b.RowSprite("SeaBrick", 69, 75, 4);
        b.CoinRow(70, 74, 5);

        b.RowSprite("SeaBrick", 84, 90, 6);
        b.CoinRow(85, 89, 7);

        b.RowSprite("SeaBrick", 100, 106, 5);
        b.CoinRow(100, 106, 6);

        b.RowSprite("SeaBrick", 112, 118, 4);
        b.CoinRow(113, 117, 5);

        b.RowSprite("SeaBrick", 140, 147, 6);
        b.CoinRow(141, 146, 7);

        b.RowSprite("SeaBrick", 150, 156, 4);
        b.CoinRow(151, 155, 5);

        b.RowSprite("SeaBrick", 160, 166, 8);
        b.CoinRow(161, 165, 9);

        // ---------------------------- peces ------------------------------
        b.CheepCheep(18, 7.5f, 6f, 2.2f);
        b.CheepCheep(32, 6f, 5f, 2.6f);
        b.CheepCheep(48, 8.5f, 7f, 2f);
        b.CheepCheep(60, 7f, 5f, 2.4f);
        b.CheepCheep(74, 6.5f, 6f, 2.8f);
        b.CheepCheep(88, 8f, 6f, 2.1f);
        b.CheepCheep(104, 7f, 6f, 2.5f);
        b.CheepCheep(120, 8.5f, 7f, 2.2f);
        b.CheepCheep(136, 6.5f, 5f, 2.6f);
        b.CheepCheep(156, 8f, 6f, 2.3f);
        b.CheepCheep(168, 7f, 5f, 2.7f);

        // burbujas ambientales
        b.Bubbles(0f, Length, 1f, 11f, 26);

        // tuberia de salida hacia el castillo
        b.ExitPipe(182, 0, 3);

        b.SpawnPlayer(4, 0);
    }

}
