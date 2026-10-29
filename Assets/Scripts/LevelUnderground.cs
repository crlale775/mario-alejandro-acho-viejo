/* Nivel 1-2: caverna subterranea construida como replica del clasico
   nivel subterraneo de Super Mario Bros. */
public static class LevelUnderground
{
    public const float Length = 216f;
    public const float CameraHeight = 6.5f;

    public static void Build(LevelBuilder b)
    {
        b.length = Length;

        const string floor = "UndergroundBlock";
        const string brick = "UndergroundBrick";

        // techo de la caverna
        b.Row(brick, 0, 215, 10);
        b.Row(brick, 0, 215, 11);

        // suelo por tramos, con tres fosas
        b.Ground(floor, 0, 52, 0);
        b.Ground(floor, 58, 96, 0);
        b.Ground(floor, 102, 150, 0);
        b.Ground(floor, 156, 215, 0);

        // pared de entrada
        b.Column(floor, 0, 0, 9);

        // ---------------------------- tramo 1 ----------------------------
        b.CoinRow(6, 9, 4, true);
        b.BrickRow(12, 18, 4);
        b.Mystery(14, 4, "MagicMushroom");
        b.Mystery(16, 4, "BlockCoin");
        b.Goomba(20, 0);
        b.Pipe(24, 0, 3);
        b.CoinRow(28, 31, 6, true);
        b.BrickRow(34, 38, 6);
        b.Goomba(35, 0);
        b.Koopa(44, 0);
        b.CoinRow(46, 49, 3, true);

        // ---------------------------- tramo 2 ----------------------------
        for (int i = 0; i < 4; i++) {
            for (int y = 1; y <= i + 1; y++) {
                b.Tile(brick, 58 + i, y);
            }
        }

        b.Goomba(66, 0);
        b.BrickRow(66, 72, 5);
        b.Mystery(69, 5, "BlockCoin");
        b.CoinRow(67, 71, 6, true);
        b.Koopa(76, 0);
        b.Pipe(80, 0, 4);
        b.BrickRow(84, 90, 6);
        b.CoinRow(85, 89, 7, true);
        b.Goomba(88, 0);
        b.CoinRow(92, 95, 3, true);

        // ---------------------------- tramo 3 ----------------------------
        for (int i = 0; i < 4; i++) {
            for (int y = 1; y <= 4 - i; y++) {
                b.Tile(brick, 102 + i, y);
            }
        }

        b.BrickRow(110, 114, 5);
        b.CoinRow(110, 114, 6, true);
        b.Goomba(118, 0);
        b.Goomba(124, 0);
        b.Koopa(130, 0);
        b.BrickRow(135, 140, 4);
        b.Mystery(136, 4, "Starman");
        b.Mystery(138, 4, "BlockCoin");
        b.CoinRow(142, 147, 5, true);
        b.Goomba(146, 0);

        // ---------------------------- tramo final -------------------------
        b.BrickRow(160, 165, 6);
        b.CoinRow(161, 164, 7, true);
        b.Koopa(166, 0);
        b.Goomba(172, 0);
        b.Pipe(180, 0, 2);
        b.CoinRow(186, 189, 4, true);
        b.Mystery(192, 4, "BlockCoin");

        // tuberia de salida hacia el nivel acuatico
        b.ExitPipe(202, 0, 4);

        // la camara se mantiene arriba, el vacio de las fosas mata
        b.DeathBarrier(-4f);

        b.SpawnPlayer(3, 0);
    }

}
