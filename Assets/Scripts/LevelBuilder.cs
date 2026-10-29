using System.Collections.Generic;
using UnityEngine;

/* Herramientas para construir niveles con bloques de una unidad.
   Cada tile (tx, ty) ocupa el cuadrado [tx, tx+1] x [ty, ty+1]. */
public class LevelBuilder
{
    public readonly Transform root;
    public float length = 200f;
    public Transform player { get; private set; }
    public PlayerMovement playerMovement { get; private set; }

    private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public LevelBuilder(string name)
    {
        root = new GameObject(name).transform;
    }

    /* ------------------------------ recursos ------------------------------ */

    public Sprite LoadSprite(string name)
    {
        if (sprites.TryGetValue(name, out Sprite sprite) && sprite != null) {
            return sprite;
        }

        sprite = Resources.Load<Sprite>($"Sprites/{name}");

        if (sprite == null) {
            Debug.LogWarning($"Sprite no encontrado: {name}");
        }

        sprites[name] = sprite;
        return sprite;
    }

    public GameObject LoadPrefab(string name)
    {
        if (prefabs.TryGetValue(name, out GameObject prefab) && prefab != null) {
            return prefab;
        }

        prefab = Resources.Load<GameObject>($"Prefabs/{name}");

        if (prefab == null) {
            Debug.LogWarning($"Prefab no encontrado: {name}");
        }

        prefabs[name] = prefab;
        return prefab;
    }

    public static Vector3 Center(int tx, int ty)
    {
        return new Vector3(tx + 0.5f, ty + 0.5f, 0f);
    }

    /* ------------------------------- bloques ------------------------------ */

    public GameObject Spawn(string prefabName, float x, float y)
    {
        GameObject prefab = LoadPrefab(prefabName);

        if (prefab == null) {
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, root);
        instance.name = prefabName;
        return instance;
    }

    public GameObject Tile(string prefabName, int tx, int ty)
    {
        return Spawn(prefabName, tx + 0.5f, ty + 0.5f);
    }

    public void Row(string prefabName, int x0, int x1, int ty)
    {
        for (int tx = x0; tx <= x1; tx++) {
            Tile(prefabName, tx, ty);
        }
    }

    public void Column(string prefabName, int tx, int y0, int y1)
    {
        for (int ty = y0; ty <= y1; ty++) {
            Tile(prefabName, tx, ty);
        }
    }

    public void Fill(string prefabName, int x0, int x1, int y0, int y1)
    {
        for (int ty = y0; ty <= y1; ty++) {
            Row(prefabName, x0, x1, ty);
        }
    }

    /* Suelo macizo: la fila superior mas las filas de relleno hacia abajo. */
    public void Ground(string prefabName, int x0, int x1, int topY, int depth = 3)
    {
        for (int i = 0; i < depth; i++) {
            Row(prefabName, x0, x1, topY - i);
        }
    }

    /* Bloques de un tile creados a partir de un sprite nuevo (castillo, fondo marino). */
    public GameObject TileSprite(string spriteName, int tx, int ty, bool solid = true, int sortingOrder = 0)
    {
        GameObject go = new GameObject(spriteName);
        go.transform.SetParent(root);
        go.transform.position = new Vector3(tx + 0.5f, ty + 0.5f, 0f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite(spriteName);
        renderer.sortingOrder = sortingOrder;

        if (solid)
        {
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }

        return go;
    }

    public void RowSprite(string spriteName, int x0, int x1, int ty, bool solid = true, int sortingOrder = 0)
    {
        for (int tx = x0; tx <= x1; tx++) {
            TileSprite(spriteName, tx, ty, solid, sortingOrder);
        }
    }

    public void GroundSprite(string spriteName, int x0, int x1, int topY, int depth = 3)
    {
        for (int i = 0; i < depth; i++) {
            RowSprite(spriteName, x0, x1, topY - i);
        }
    }

    public void Brick(int tx, int ty)
    {
        Tile("Brick", tx, ty);
    }

    public void BrickRow(int x0, int x1, int ty)
    {
        Row("Brick", x0, x1, ty);
    }

    public void Mystery(int tx, int ty, string item = "BlockCoin")
    {
        GameObject block = Tile("MysteryBlock", tx, ty);

        if (block == null) {
            return;
        }

        BlockHit hit = block.GetComponent<BlockHit>();

        if (hit != null) {
            hit.item = LoadPrefab(item);
            hit.maxHits = 1;
        }
    }

    public void Coin(int tx, int ty, bool underground = false)
    {
        GameObject coin = Tile("Coin", tx, ty);

        if (coin == null) {
            return;
        }

        if (underground)
        {
            SpriteRenderer renderer = coin.GetComponent<SpriteRenderer>();
            Sprite sprite = LoadSprite("Coin_Underground");

            if (renderer != null && sprite != null) {
                renderer.sprite = sprite;
            }
        }
    }

    public void CoinRow(int x0, int x1, int ty, bool underground = false)
    {
        for (int tx = x0; tx <= x1; tx++) {
            Coin(tx, ty, underground);
        }
    }

    public void Goomba(int tx, int groundY)
    {
        Spawn("Goomba", tx + 0.5f, groundY + 1.5f);
    }

    public void Koopa(int tx, int groundY)
    {
        Spawn("Koopa", tx + 0.5f, groundY + 1.75f);
    }

    public void CheepCheep(int tx, float y, float patrol = 6f, float speed = 2f)
    {
        GameObject fish = new GameObject("CheepCheep");
        fish.transform.SetParent(root);
        fish.transform.position = new Vector3(tx + 0.5f, y, 0f);
        fish.layer = LayerMask.NameToLayer("Enemy");

        SpriteRenderer renderer = fish.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite("CheepCheep1");
        renderer.sortingOrder = 3;

        Rigidbody2D body = fish.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.simulated = true;

        BoxCollider2D collider = fish.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.9f, 0.8f);

        CheepCheep cheep = fish.AddComponent<CheepCheep>();
        cheep.Initialize(LoadSprite("CheepCheep1"), LoadSprite("CheepCheep2"));
        cheep.SetPatrol(tx + 0.5f - patrol, tx + 0.5f + patrol, speed);
    }

    /* -------------------------------- tuberias ----------------------------- */

    public GameObject Pipe(int tx, int groundY, int height)
    {
        GameObject pipe = new GameObject("Pipe");
        pipe.transform.SetParent(root);
        pipe.transform.position = new Vector3(tx + 1f, groundY + height * 0.5f, 0f);

        BoxCollider2D solid = pipe.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(2f, height);

        GameObject top = new GameObject("Top");
        top.transform.SetParent(pipe.transform);
        top.transform.localPosition = new Vector3(0f, height * 0.5f - 0.5f, 0f);
        SpriteRenderer topRenderer = top.AddComponent<SpriteRenderer>();
        topRenderer.sprite = LoadSprite("PipeTop");
        topRenderer.sortingOrder = 4;
        BoxCollider2D topCollider = top.AddComponent<BoxCollider2D>();
        topCollider.size = new Vector2(2f, 1f);
        topCollider.offset = top.transform.localPosition;

        GameObject body = new GameObject("Body");
        body.transform.SetParent(pipe.transform);
        body.transform.localPosition = new Vector3(0f, -height * 0.5f + (height - 1f) * 0.5f, 0f);
        SpriteRenderer bodyRenderer = body.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = LoadSprite("PipeBottom");
        bodyRenderer.drawMode = SpriteDrawMode.Tiled;
        bodyRenderer.size = new Vector2(2f, height - 1f);
        bodyRenderer.sortingOrder = 4;

        return pipe;
    }

    public void ExitPipe(int tx, int groundY, int height)
    {
        GameObject pipe = Pipe(tx, groundY, height);

        GameObject trigger = new GameObject("Exit Trigger");
        trigger.transform.SetParent(pipe.transform);
        trigger.transform.localPosition = new Vector3(0f, height * 0.5f + 0.35f, 0f);

        BoxCollider2D triggerCollider = trigger.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(1.8f, 1.8f);

        trigger.AddComponent<LevelExitPipe>();
    }

    /* ------------------------------- bandera ------------------------------- */

    public void FlagPole(int tx, int groundY, int height = 10)
    {
        float baseY = groundY + 1f;

        GameObject pole = new GameObject("Flag Pole");
        pole.transform.SetParent(root);
        pole.transform.position = new Vector3(tx + 0.5f, baseY, 0f);

        SpriteRenderer renderer = pole.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite("FlagPole");
        renderer.sortingOrder = 5;

        GameObject flag = new GameObject("Flag");
        flag.transform.SetParent(pole.transform);
        flag.transform.localPosition = new Vector3(-1f, height - 1f, 0f);
        SpriteRenderer flagRenderer = flag.AddComponent<SpriteRenderer>();
        flagRenderer.sprite = LoadSprite("Flag");
        flagRenderer.sortingOrder = 6;

        GameObject bottom = new GameObject("Bottom");
        bottom.transform.SetParent(pole.transform);
        bottom.transform.localPosition = new Vector3(0f, 0f, 0f);

        BoxCollider2D trigger = pole.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.3f, height);
        trigger.offset = new Vector2(0.1f, height * 0.5f);

        FlagPole flagPole = pole.AddComponent<FlagPole>();
        flagPole.flag = flag.transform;
        flagPole.poleBottom = bottom.transform;
        flagPole.castle = Castle(tx + 6, groundY);
    }

    public Transform Castle(int tx, int groundY)
    {
        GameObject castle = Spawn("Castle", tx + 0.5f, groundY + 1f);

        if (castle == null) {
            return null;
        }

        foreach (Collider2D collider in castle.GetComponentsInChildren<Collider2D>()) {
            Object.Destroy(collider);
        }

        return castle.transform;
    }

    /* ------------------------------- decorados ----------------------------- */

    public GameObject SpriteObject(string spriteName, float x, float y, int sortingOrder = 0, bool solid = false, bool tiled = false, float width = 1f, float height = 1f)
    {
        GameObject go = new GameObject(spriteName);
        go.transform.SetParent(root);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite(spriteName);
        renderer.sortingOrder = sortingOrder;

        if (tiled)
        {
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(width, height);
        }

        if (solid)
        {
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, height);
        }

        return go;
    }

    public void TileDecoration(string spriteName, int tx, int ty, int sortingOrder = -5, bool tiled = false)
    {
        SpriteObject(spriteName, tx + 0.5f, ty + 0.5f, sortingOrder, false, tiled);
    }

    public void AnimatedDecoration(int tx, float y, string[] frames, float framerate, int sortingOrder = 0)
    {
        GameObject go = SpriteObject(frames[0], tx + 0.5f, y, sortingOrder);
        Sprite[] list = new Sprite[frames.Length];

        for (int i = 0; i < frames.Length; i++) {
            list[i] = LoadSprite(frames[i]);
        }

        AnimatedSprite animation = go.AddComponent<AnimatedSprite>();
        animation.sprites = list;
        animation.framerate = framerate;
    }

    /* --------------------------------- lava -------------------------------- */

    public void LavaPool(int x0, int x1, int tileY, bool hazard = true)
    {
        for (int tx = x0; tx <= x1; tx++) {
            AnimatedDecoration(tx, tileY + 0.5f, new[] { "Lava1", "Lava2", "Lava3" }, 0.14f, -2);
        }

        if (!hazard) {
            return;
        }

        GameObject hazardObject = new GameObject("Lava Hazard");
        hazardObject.transform.SetParent(root);
        hazardObject.transform.position = new Vector3((x0 + x1 + 1) * 0.5f, tileY + 0.5f, 0f);

        BoxCollider2D trigger = hazardObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(x1 - x0 + 1, 0.8f);

        Hazard zone = hazardObject.AddComponent<Hazard>();
        zone.instantKill = true;
    }

    public void HazardZone(float x, float y, float width, float height, bool instantKill)
    {
        GameObject hazardObject = new GameObject("Hazard");
        hazardObject.transform.SetParent(root);
        hazardObject.transform.position = new Vector3(x, y, 0f);

        BoxCollider2D trigger = hazardObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(width, height);

        Hazard zone = hazardObject.AddComponent<Hazard>();
        zone.instantKill = instantKill;
    }

    public void DeathBarrier(float y)
    {
        GameObject barrier = new GameObject("Death Barrier");
        barrier.transform.SetParent(root);
        barrier.transform.position = new Vector3(length * 0.5f, y, 0f);

        BoxCollider2D collider = barrier.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(length + 60f, 1f);

        barrier.AddComponent<DeathBarrier>();
    }

    public void FireBar(int tx, int ty, int segments, float speed)
    {
        GameObject bar = new GameObject("Fire Bar");
        bar.transform.SetParent(root);
        bar.transform.position = new Vector3(tx + 0.5f, ty + 0.5f, 0f);

        FireBar fireBar = bar.AddComponent<FireBar>();
        fireBar.segments = segments;
        fireBar.speed = speed;
        fireBar.ballSprite = LoadSprite("FirebarBall");
    }

    public BridgeCollapse Bridge()
    {
        GameObject holder = new GameObject("Bridge");
        holder.transform.SetParent(root);
        return holder.AddComponent<BridgeCollapse>();
    }

    public GameObject BridgePlank(int tx, int tileY, BridgeCollapse bridge)
    {
        GameObject plank = SpriteObject("Bridge", tx + 0.5f, tileY + 0.5f, 1, true, true, 1f, 1f);
        plank.name = "Plank";

        if (bridge != null) {
            bridge.AddPlank(plank);
        }

        return plank;
    }

    public BowserBoss Bowser(int tx, int groundY, float patrolLeft, float patrolRight)
    {
        GameObject boss = new GameObject("Bowser");
        boss.transform.SetParent(root);
        boss.transform.position = new Vector3(tx + 0.5f, groundY + 2f, 0f);

        BowserBoss bowser = boss.AddComponent<BowserBoss>();
        bowser.Initialize(LoadSprite("Bowser"), LoadSprite("Fireball"), null);
        bowser.SetPatrol(patrolLeft, patrolRight);

        return bowser;
    }

    public void Axe(int tx, int groundY, BridgeCollapse bridge)
    {
        GameObject axe = SpriteObject("Axe", tx + 0.5f, groundY + 1.5f, 2);

        BoxCollider2D trigger = axe.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.2f, 1.6f);

        AxeGoal goal = axe.AddComponent<AxeGoal>();
        goal.bridge = bridge;
    }

    /* ------------------------------ jugador -------------------------------- */

    public Transform SpawnPlayer(int tx, int groundY)
    {
        GameObject marioPrefab = LoadPrefab("Mario");

        if (marioPrefab == null) {
            return null;
        }

        GameObject mario = Object.Instantiate(marioPrefab, new Vector3(tx + 0.5f, groundY + 1.5f, 0f), Quaternion.identity, root);
        mario.name = "Mario";

        player = mario.transform;
        playerMovement = mario.GetComponent<PlayerMovement>();

        return player;
    }

    public void Bubbles(float x0, float x1, float y0, float y1, int count)
    {
        GameObject holder = new GameObject("Bubbles");
        holder.transform.SetParent(root);
        holder.transform.position = Vector3.zero;

        Sprite sprite = LoadSprite("Bubble");

        for (int i = 0; i < count; i++)
        {
            GameObject bubble = new GameObject("Bubble");
            bubble.transform.SetParent(holder.transform);

            SpriteRenderer renderer = bubble.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;

            BubbleFloat floater = bubble.AddComponent<BubbleFloat>();
            floater.minX = x0;
            floater.maxX = x1;
            floater.minY = y0;
            floater.maxY = y1;
            floater.randomizeStart = true;
        }
    }

}
