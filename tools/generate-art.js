/*
 * Generador de sprites pixel-art para los niveles nuevos (subterraneo, acuatico y castillo).
 * Escribe PNGs (RGBA, sin dependencias externas) y su archivo .meta de Unity.
 *
 * Uso: node tools/generate-art.js
 */
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const OUT_DIR = path.join(__dirname, '..', 'Assets', 'Resources', 'Sprites');
const PPU = 16;

/* ----------------------------- codificador PNG ----------------------------- */

const CRC_TABLE = (() => {
  const table = new Int32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    table[n] = c;
  }
  return table;
})();

function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}

function chunk(type, data) {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length, 0);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), data]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body), 0);
  return Buffer.concat([len, body, crc]);
}

function encodePNG(width, height, rgba) {
  const raw = Buffer.alloc((width * 4 + 1) * height);
  for (let y = 0; y < height; y++) {
    raw[y * (width * 4 + 1)] = 0; // filtro none
    rgba.copy(raw, y * (width * 4 + 1) + 1, y * width * 4, (y + 1) * width * 4);
  }
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8;  // bit depth
  ihdr[9] = 6;  // RGBA
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', ihdr),
    chunk('IDAT', zlib.deflateSync(raw, { level: 9 })),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

/* --------------------------------- lienzo --------------------------------- */

function color(hex, a = 255) {
  const h = hex.replace('#', '');
  return [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16), a];
}

function shade(c, factor) {
  return [
    Math.max(0, Math.min(255, Math.round(c[0] * factor))),
    Math.max(0, Math.min(255, Math.round(c[1] * factor))),
    Math.max(0, Math.min(255, Math.round(c[2] * factor))),
    c[3],
  ];
}

function mulberry32(seed) {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

class Canvas {
  constructor(w, h, seed = 1) {
    this.w = w;
    this.h = h;
    this.data = Buffer.alloc(w * h * 4);
    this.random = mulberry32(seed);
  }

  set(x, y, c) {
    if (x < 0 || y < 0 || x >= this.w || y >= this.h) return;
    const i = (y * this.w + x) * 4;
    this.data[i] = c[0];
    this.data[i + 1] = c[1];
    this.data[i + 2] = c[2];
    this.data[i + 3] = c.length > 3 ? c[3] : 255;
  }

  get(x, y) {
    if (x < 0 || y < 0 || x >= this.w || y >= this.h) return [0, 0, 0, 0];
    const i = (y * this.w + x) * 4;
    return [this.data[i], this.data[i + 1], this.data[i + 2], this.data[i + 3]];
  }

  opaque(x, y) {
    return this.get(x, y)[3] > 0;
  }

  rect(x, y, w, h, c) {
    for (let j = y; j < y + h; j++) for (let i = x; i < x + w; i++) this.set(i, j, c);
    return this;
  }

  ellipse(cx, cy, rx, ry, c) {
    for (let y = Math.floor(cy - ry); y <= Math.ceil(cy + ry); y++) {
      for (let x = Math.floor(cx - rx); x <= Math.ceil(cx + rx); x++) {
        const dx = (x - cx) / rx;
        const dy = (y - cy) / ry;
        if (dx * dx + dy * dy <= 1.0) this.set(x, y, c);
      }
    }
    return this;
  }

  /* Mancha irregular de ruido, util para dar textura a las piedras. */
  speckle(colorA, colorB, chance = 0.25) {
    for (let y = 0; y < this.h; y++) {
      for (let x = 0; x < this.w; x++) {
        if (!this.opaque(x, y)) continue;
        if (this.random() < chance) this.set(x, y, colorB);
        else if (this.random() < chance) this.set(x, y, colorA);
      }
    }
    return this;
  }

  /* Oscurece el borde de la silueta para que el sprite tenga contorno. */
  outline(c, alpha = 160) {
    const edges = [];
    for (let y = 0; y < this.h; y++) {
      for (let x = 0; x < this.w; x++) {
        if (this.opaque(x, y)) continue;
        if (this.opaque(x - 1, y) || this.opaque(x + 1, y) || this.opaque(x, y - 1) || this.opaque(x, y + 1)) {
          edges.push([x, y]);
        }
      }
    }
    for (const [x, y] of edges) this.set(x, y, [c[0], c[1], c[2], alpha]);
    return this;
  }

  /* Sombra interior en el borde inferior derecho. */
  bottomShade(factor) {
    const copy = Buffer.from(this.data);
    const at = (x, y) => {
      const i = (y * this.w + x) * 4;
      return [copy[i], copy[i + 1], copy[i + 2], copy[i + 3]];
    };
    for (let y = 0; y < this.h; y++) {
      for (let x = 0; x < this.w; x++) {
        const c = at(x, y);
        if (c[3] === 0) continue;
        const below = y + 1 < this.h ? at(x, y + 1) : [0, 0, 0, 0];
        const right = x + 1 < this.w ? at(x + 1, y) : [0, 0, 0, 0];
        if (below[3] === 0 || right[3] === 0) this.set(x, y, shade(c, factor));
      }
    }
    return this;
  }
}

/* ------------------------------- utilidades ------------------------------- */

function mirroX(canvas) {
  const copy = Buffer.from(canvas.data);
  for (let y = 0; y < canvas.h; y++) {
    for (let x = 0; x < canvas.w; x++) {
      const src = (y * canvas.w + (canvas.w - 1 - x)) * 4;
      const dst = (y * canvas.w + x) * 4;
      canvas.data[dst] = copy[src];
      canvas.data[dst + 1] = copy[src + 1];
      canvas.data[dst + 2] = copy[src + 2];
      canvas.data[dst + 3] = copy[src + 3];
    }
  }
  return canvas;
}

const META_TEMPLATE = (guid, name) => `fileFormatVersion: 2
guid: ${guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 11
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: ${PPU}
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 256
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 3
    buildTarget: Standalone
    maxTextureSize: 256
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
  spritePackingTag: 
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;

/* guid determinista y unico por nombre de archivo */
function guidFor(name) {
  const { createHash } = require('crypto');
  return createHash('md5').update(`mario-alejandro-acho/${name}`).digest('hex');
}

let written = 0;
function write(name, canvas) {
  fs.writeFileSync(path.join(OUT_DIR, `${name}.png`), encodePNG(canvas.w, canvas.h, canvas.data));
  fs.writeFileSync(path.join(OUT_DIR, `${name}.png.meta`), META_TEMPLATE(guidFor(name), name));
  written++;
}

/* ---------------------------------- tiles --------------------------------- */

const STONE = color('#70707f');
const STONE_DARK = color('#45454f');
const STONE_LIGHT = color('#96969f');
const MORTAR = color('#26262e');
const LAVA_HOT = color('#ffd34d');
const LAVA_MID = color('#ff8400');
const LAVA_LOW = color('#c62b00');
const LAVA_ROCK = color('#5a2010');

function castleGround() {
  const c = new Canvas(16, 16, 11);
  c.rect(0, 0, 16, 16, STONE);
  c.rect(0, 0, 16, 2, STONE_LIGHT);
  c.rect(0, 6, 16, 1, MORTAR);
  c.rect(0, 14, 16, 2, STONE_DARK);
  c.rect(7, 0, 2, 6, MORTAR);
  c.rect(3, 7, 1, 7, MORTAR);
  c.rect(11, 7, 1, 7, MORTAR);
  c.speckle(STONE_LIGHT, STONE_DARK, 0.12);
  return c;
}

function castleBrick() {
  const c = new Canvas(16, 16, 23);
  c.rect(0, 0, 16, 16, MORTAR);
  const brick = color('#6b6b7c');
  for (let row = 0; row < 2; row++) {
    const y = row * 8 + 1;
    const offset = row === 0 ? 0 : 4;
    for (let bx = -8; bx < 16; bx += 8) {
      const x = bx + offset;
      c.rect(x, y, 7, 6, brick);
      c.rect(x, y, 7, 1, STONE_LIGHT);
      c.rect(x, y + 5, 7, 1, STONE_DARK);
    }
  }
  c.speckle(STONE_LIGHT, STONE_DARK, 0.08);
  return c;
}

function castleWindow() {
  const c = new Canvas(16, 16, 7);
  c.rect(2, 2, 12, 14, MORTAR);
  c.ellipse(8, 5, 5.5, 4.5, color('#12121a'));
  c.rect(3, 5, 10, 11, color('#12121a'));
  c.rect(2, 14, 12, 2, STONE_DARK);
  const bar = color('#9a9aa6');
  c.rect(7, 1, 2, 15, bar);
  c.rect(3, 7, 10, 1, bar);
  c.rect(3, 11, 10, 1, bar);
  c.rect(0, 0, 2, 16, STONE);
  c.rect(14, 0, 2, 16, STONE);
  return c;
}

function lavaFrame(phase) {
  const c = new Canvas(16, 16, 31 + phase);
  for (let y = 0; y < 16; y++) {
    const t = y / 15;
    const base = [
      Math.round(LAVA_HOT[0] * (1 - t) + LAVA_LOW[0] * t),
      Math.round(LAVA_HOT[1] * (1 - t) + LAVA_LOW[1] * t),
      Math.round(LAVA_HOT[2] * (1 - t) + LAVA_LOW[2] * t),
      255,
    ];
    for (let x = 0; x < 16; x++) c.set(x, y, base);
  }
  // cresta ondulada, con el desfase por fotograma para animarla
  for (let x = 0; x < 16; x++) {
    const wave = Math.sin(((x + phase * 8) / 16) * Math.PI * 2) * 1.6;
    const top = Math.round(2 + wave);
    for (let y = Math.max(0, top - 2); y <= top; y++) {
      c.set(x, y, y < top ? LAVA_HOT : LAVA_MID);
    }
    c.set(x, top + 1, LAVA_MID);
  }
  // burbujas
  const rnd = mulberry32(77 + phase);
  for (let i = 0; i < 6; i++) {
    const x = Math.floor(rnd() * 16);
    const y = 5 + Math.floor(rnd() * 9);
    c.set(x, y, LAVA_HOT);
    c.set(x + 1, y, LAVA_MID);
    c.set(x, y + 1, LAVA_MID);
  }
  c.rect(0, 15, 16, 1, LAVA_ROCK);
  return c;
}

function bridge() {
  const c = new Canvas(16, 8, 41);
  const wood = color('#b0742f');
  const dark = color('#6d421a');
  const light = color('#d69a4d');
  c.rect(0, 1, 16, 6, wood);
  c.rect(0, 1, 16, 1, light);
  c.rect(0, 6, 16, 1, dark);
  c.rect(4, 1, 1, 6, dark);
  c.rect(9, 1, 1, 6, dark);
  c.speckle(light, dark, 0.1);
  return c;
}

function axe() {
  const c = new Canvas(16, 16, 53);
  const handle = color('#8a5a2b');
  const handleDark = color('#5d3a17');
  c.rect(7, 5, 2, 11, handle);
  c.rect(7, 5, 1, 11, handleDark);
  const blade = color('#d8dde8');
  const bladeDark = color('#8b93a3');
  c.rect(3, 2, 8, 4, blade);
  c.rect(3, 2, 8, 1, color('#ffffff'));
  c.rect(3, 5, 8, 1, bladeDark);
  c.set(2, 3, blade);
  c.set(2, 4, bladeDark);
  c.set(11, 3, blade);
  c.set(11, 4, bladeDark);
  c.outline(color('#1a1a22'), 190);
  return c;
}

function torchFrame(frame) {
  const c = new Canvas(8, 16, 61 + frame);
  const stick = color('#7a4a22');
  c.rect(3, 8, 2, 8, stick);
  c.rect(3, 8, 1, 8, color('#5a3416'));
  // llama animada
  const sizes = frame === 0 ? [3, 5, 2] : [2, 6, 3];
  c.ellipse(4, 6, 3, sizes[1] - 1, LAVA_LOW);
  c.ellipse(4, 6, 2.4, sizes[0] + 1, LAVA_MID);
  c.ellipse(4, 7, 1.4, sizes[2], LAVA_HOT);
  return c;
}

function chain() {
  const c = new Canvas(8, 8, 71);
  const metal = color('#b9bec9');
  const dark = color('#6f747f');
  c.rect(2, 0, 4, 2, metal);
  c.rect(2, 6, 4, 2, metal);
  c.rect(3, 2, 2, 4, dark);
  c.rect(2, 0, 4, 1, color('#ffffff'));
  c.outline(color('#1a1a22'), 170);
  return c;
}

function fireball() {
  const c = new Canvas(8, 8, 83);
  c.ellipse(3.5, 4, 3.5, 3.5, LAVA_LOW);
  c.ellipse(3.5, 4, 2.4, 2.4, LAVA_MID);
  c.ellipse(3.5, 4, 1.3, 1.3, LAVA_HOT);
  return c;
}

function firebarBall() {
  const c = new Canvas(8, 8, 89);
  c.ellipse(4, 4, 4, 4, LAVA_MID);
  c.ellipse(4, 4, 2.6, 2.6, LAVA_HOT);
  c.ellipse(4, 4, 1.2, 1.2, color('#ffffff'));
  return c;
}

/* --------------------------------- Bowser --------------------------------- */

function bowser() {
  const w = 32;
  const h = 32;
  const c = new Canvas(w, h, 97);
  const GREEN = color('#3f9e2d');
  const GREEN_DARK = color('#25701c');
  const CREAM = color('#f4d49a');
  const CREAM_DARK = color('#d8ae6c');
  const RED = color('#d93a1f');
  const RED_DARK = color('#94220f');
  const SPIKE = color('#f4f4f4');
  const EYE = color('#ffffff');
  const PUPIL = color('#101010');

  // caparazon con pinchos
  c.ellipse(21, 18, 9, 10, GREEN_DARK);
  c.ellipse(21, 18, 8, 9, GREEN);
  for (const [sx, sy] of [[13, 12], [11, 19], [15, 26], [22, 28], [28, 24], [29, 14], [22, 8]]) {
    c.ellipse(sx, sy, 2, 2, SPIKE);
  }
  c.ellipse(21, 18, 4, 4.5, color('#6fcf52'));

  // cola
  c.ellipse(29, 26, 3, 2, GREEN_DARK);

  // barriga
  c.ellipse(14, 24, 6, 5, CREAM);
  c.ellipse(14, 25, 5, 4, CREAM_DARK);

  // piernas y brazos
  c.rect(9, 27, 6, 5, GREEN);
  c.rect(17, 27, 6, 5, GREEN);
  c.rect(7, 30, 8, 2, GREEN_DARK);
  c.rect(17, 30, 8, 2, GREEN_DARK);
  c.ellipse(9, 21, 3.5, 3, GREEN);
  c.ellipse(6, 24, 2.5, 2.5, GREEN_DARK);

  // cabeza
  c.ellipse(9, 11, 9, 8.5, GREEN);
  c.ellipse(7, 14, 6, 5, CREAM);
  c.ellipse(6, 15, 4, 3, CREAM_DARK);

  // cuernos
  c.ellipse(3, 3, 2, 2.4, SPIKE);
  c.ellipse(14, 3, 2, 2.4, SPIKE);

  // pelo rojo
  c.ellipse(6, 5, 4, 3.2, RED);
  c.ellipse(12, 6, 4, 3, RED);
  c.ellipse(17, 8, 3, 2.6, RED_DARK);
  c.ellipse(9, 4, 3.4, 2.4, RED);

  // ojos
  c.ellipse(6, 9, 2.4, 2.6, EYE);
  c.ellipse(12, 9, 2.2, 2.6, EYE);
  c.ellipse(5, 9, 1.3, 1.5, PUPIL);
  c.ellipse(11, 9, 1.2, 1.5, PUPIL);
  // ceno enojado
  c.rect(3, 5, 5, 1, GREEN_DARK);
  c.rect(10, 5, 5, 1, GREEN_DARK);

  // boca y colmillos
  c.rect(4, 16, 7, 1, color('#7a2a12'));
  c.rect(4, 17, 7, 1, color('#f7f7f7'));
  c.set(5, 16, color('#f7f7f7'));
  c.set(9, 16, color('#f7f7f7'));

  c.outline(color('#101a0c'), 190);
  return c;
}

/* --------------------------------- peces ---------------------------------- */

function cheepCheep(frame) {
  const c = new Canvas(16, 16, 107 + frame);
  const RED = color('#e5473a');
  const RED_DARK = color('#a51f18');
  const WHITE = color('#fdfdfd');
  const FIN = color('#ffd34d');

  // cola (cambia de posicion entre fotogramas)
  const tailY = frame === 0 ? 8 : 6;
  c.ellipse(12.5, tailY, 3, 3.6, RED_DARK);
  c.ellipse(13.5, tailY + 2, 2, 2.6, RED);
  // cuerpo
  c.ellipse(7, 8, 6, 5.4, RED);
  c.ellipse(6, 10, 4.4, 3.2, WHITE);
  c.ellipse(5, 9, 6, 3.4, RED);
  // aleta dorsal e inferior
  c.ellipse(7, 2.6, 3.4, 1.8, FIN);
  c.ellipse(7, 13.4, 3.2, 1.8, FIN);
  // ojo
  c.ellipse(2.6, 7, 1.9, 2, WHITE);
  c.ellipse(2.2, 7, 1.1, 1.2, color('#111111'));
  // boca
  c.rect(0, 9, 3, 1, RED_DARK);

  c.outline(color('#3a0d08'), 180);
  return c;
}

/* -------------------------------- acuatico -------------------------------- */

function seaGround() {
  const c = new Canvas(16, 16, 131);
  const sand = color('#d9c07b');
  const sandDark = color('#b39a55');
  const rock = color('#6d7f92');
  const rockDark = color('#4a5a6b');
  c.rect(0, 0, 16, 16, rock);
  c.rect(0, 0, 16, 4, sand);
  c.rect(0, 3, 16, 1, sandDark);
  c.rect(0, 13, 16, 3, rockDark);
  c.rect(6, 4, 1, 9, rockDark);
  c.rect(12, 4, 1, 7, rockDark);
  c.speckle(sand, sandDark, 0.12);
  return c;
}

function seaBrick() {
  const c = new Canvas(16, 16, 137);
  const base = color('#5c7fa5');
  const baseDark = color('#3d5a78');
  const baseLight = color('#84a8c9');
  c.rect(0, 0, 16, 16, base);
  c.rect(0, 0, 16, 2, baseLight);
  c.rect(0, 14, 16, 2, baseDark);
  c.rect(0, 7, 16, 1, baseDark);
  c.rect(4, 0, 1, 7, baseDark);
  c.rect(10, 8, 1, 8, baseDark);
  c.speckle(baseLight, baseDark, 0.13);
  return c;
}

function coral(variant) {
  const c = new Canvas(16, 16, 149 + variant);
  const pink = variant === 0 ? color('#e4607f') : color('#f0813f');
  const pinkDark = variant === 0 ? color('#a5324f') : color('#b4551a');
  const trunk = variant === 0 ? [8] : [5, 11];
  for (const tx of trunk) {
    c.rect(tx - 1, 8, 2, 8, pinkDark);
    c.rect(tx - 1, 8, 1, 8, pink);
    c.ellipse(tx, 6, 2.4, 2.6, pink);
  }
  c.ellipse(4, 10, 2.6, 2.2, pink);
  c.ellipse(12, 10, 2.6, 2.2, pink);
  c.ellipse(8, 3, 2.2, 2.4, pink);
  c.ellipse(8, 2.6, 1.1, 1.2, color('#ffd0dd', 200));
  c.outline(color('#5c1030'), 150);
  return c;
}

function seaweed(variant) {
  const c = new Canvas(16, 24, 163 + variant);
  const green = variant === 0 ? color('#3f9e4a') : color('#2f8f6a');
  const greenDark = variant === 0 ? color('#256b2e') : color('#1c5f45');
  const stems = variant === 0 ? [4, 8, 12] : [6, 10];
  for (const sx of stems) {
    const height = 14 + ((sx % 3) * 3);
    for (let y = 0; y < height; y++) {
      const wobble = Math.sin((y / 4) + sx) * 1.6;
      const x = Math.round(sx + wobble);
      c.set(x, 23 - y, greenDark);
      c.set(x - 1, 23 - y, green);
    }
  }
  c.outline(color('#123321'), 140);
  return c;
}

function bubble() {
  const c = new Canvas(8, 8, 179);
  const blue = color('#bfe9ff', 170);
  const white = color('#ffffff', 220);
  for (let y = 0; y < 8; y++) {
    for (let x = 0; x < 8; x++) {
      const dx = (x - 3.5) / 3.2;
      const dy = (y - 3.5) / 3.2;
      const d = dx * dx + dy * dy;
      if (d <= 1) c.set(x, y, blue);
      if (d > 0.55 && d <= 1) c.set(x, y, white);
    }
  }
  c.set(2, 2, white);
  c.set(3, 2, white);
  return c;
}

function waterSurface() {
  const c = new Canvas(16, 16, 191);
  const deepTop = color('#2b7fd4', 235);
  const deep = color('#1d5ba8', 235);
  const foam = color('#dff4ff', 235);
  for (let y = 0; y < 16; y++) {
    for (let x = 0; x < 16; x++) {
      c.set(x, y, y === 0 ? foam : y < 3 ? deepTop : deep);
    }
  }
  for (let x = 0; x < 16; x++) {
    const p = Math.sin((x / 16) * Math.PI * 2) * 1.4;
    if (p > 0.4) c.set(x, 1, foam);
    if (p > 1) c.set(x, 2, color('#bfe9ff', 235));
  }
  return c;
}

/* ---------------------------------  salida  -------------------------------- */

fs.mkdirSync(OUT_DIR, { recursive: true });

write('CastleGround', castleGround());
write('CastleBrick', castleBrick());
write('CastleWindow', castleWindow());
write('Lava1', lavaFrame(0));
write('Lava2', lavaFrame(1));
write('Lava3', lavaFrame(2));
write('Bridge', bridge());
write('Axe', axe());
write('Torch1', torchFrame(0));
write('Torch2', torchFrame(1));
write('Chain', chain());
write('Bowser', bowser());
write('Fireball', fireball());
write('FirebarBall', firebarBall());
write('CheepCheep1', cheepCheep(0));
write('CheepCheep2', cheepCheep(1));
write('SeaGround', seaGround());
write('SeaBrick', seaBrick());
write('Coral1', coral(0));
write('Coral2', coral(1));
write('Seaweed1', seaweed(0));
write('Seaweed2', seaweed(1));
write('Bubble', bubble());
write('WaterSurface', waterSurface());

console.log(`Sprites generados: ${written}`);
