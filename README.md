# Procedural Terrain Generator


This Procedural Terrain Generator is a tool for generating custom 3D terrain meshes. The project was created in Unity 2022.3.11f1 and uses 3D perlin noise and marching cubes algorithm to create square terrain meshes of set size.


## Features

- Pseudo-random 3D terrain generation
- Generating terrain with a random or set (deterministic) seed
- Customizable noise and mesh generation:
    - Noise parameters (scale, octaves, persistence, lacunarity, etc)
    - Mesh size (how many connected chunks of terrain to generate)
    - Mesh detail level
    - Base terrain ground level
- Optional simple water plane generation (no fluid simulation)
- Optional random object placing on terrain (trees)
- Terrain sculpting using a brush-like tool
- Ability to export the terrain to an .obj file
- Vizualization options:
    - Customizable environment and terrain colors
    - Light source simulating day-night cycle

## Screenshots

![screenshot of generated terrain](/Resources/terrain1.png)
![screenshot of generated terrain](/Resources/terrain2.png)
