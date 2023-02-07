using System;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using System.Linq;
using Earthgen.render;
using Earthgen.planet.climate;

namespace Earthgen.unity
{
    public class RenderedPlanetData : MonoBehaviour
    {
        public Mesh[] meshes;
        public Texture2D tileTexture;
        public Planet_colours tileColors;

        public MeshParameters meshParameters;
        public TextureParameters textureParameters;

        private GeneratedPlanetData Data => GetComponent<GeneratedPlanetData>();

        public void RenderMeshes(MeshParameters par, string name)
        {
            meshParameters = par;
            var planet = Data.planet;
            var grid = planet.grid;

            if (par.tilesPerRenderer == 1) {
                // no need to do the splits, we're rendering each tile to its own mesh
                Debug.Log($"Rendering {grid.tiles.Length} singleton tiles");
                ResizeMeshes(grid.tiles.Length);
                int idx = 0;
                foreach (var tile in grid.tiles) {
                    meshes[idx] = RenderMesh(new[] { tile }, meshes[idx], $"{name} [Tile {tile.id}]");
                    idx++;
                }
                return;
            }
            Queue<ArraySegment<Tile>> segments = new();
            segments.Enqueue(new(grid.tiles));
            // the particular segmentation style means that the segment with the greatest size should always be first,
            // with a possible off-by-1 because of odd divisions
            while (segments.TryPeek(out var segment) && segment.Count > meshParameters.tilesPerRenderer) {
                segment = segments.Dequeue();
                int limit = (segment.Count + 1) / 2;
                segments.Enqueue(segment.Slice(0, limit));
                segments.Enqueue(segment.Slice(limit));
            }
            var segmentCount = segments.Count;
            Debug.Log($"Rendering {segmentCount} segments with max tile count {segments.Peek().Count}");
            ResizeMeshes(segmentCount);
            for (int i = 0; i < meshes.Length; i++) {
                var segment = segments.Dequeue();
                meshes[i] = RenderMesh(segment, meshes[i], meshes.Length > 1 ? $"{name} [Segment {i}]" : $"{name} [Mesh]");
            }

            void ResizeMeshes(int segmentCount)
            {
                if ((meshes?.Length ?? 0) > segmentCount) {
                    for (int i = segmentCount; i < meshes.Length; i++) {
                        if (Application.isPlaying) {
                            Destroy(meshes[i]);
                        }
                        else {
                            DestroyImmediate(meshes[i]);
                        }
                    }
                }
                Array.Resize(ref meshes, segmentCount);
            }
        }

        private Mesh RenderMesh(ICollection<Tile> tiles, Mesh mesh, string name = null)
        {
            if (!mesh) mesh = new();
            mesh.Clear();
            mesh.name = name;
            var planet = Data.planet;
            float seaLevel = (float)planet.sea_level();
            float radius = (float)planet.radius();
            float elevationScale = meshParameters.elevationScale;
            bool modelTopography = meshParameters.modelTopography && elevationScale > 1;
            int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));
            Vector2 texelSize = new(1.0f / tilesPerSide, 1.0f / tilesPerSide);
            //Debug.Log($"Scaling topography {modelTopography} by {elevationScale} with planet radius {radius} and sea level {seaLevel}");

            // we need this to scale the UVs, just assume that tiles are roughly the size of the first one, with a margin
            float tileRadians = Mathf.Sqrt(planet.unit_area(tiles.First())) * 1.5f;
            //Debug.Log($"tileRadians={tileRadians} for tile area {planet.unit_area(tiles.First())}, polygon {{ {string.Join(',', tiles.First().polygon(Quaternion.identity))} }}");

            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();

            foreach (var tile in tiles) {
                var terrain = tile.terrain(planet);
                float u = (tile.id % tilesPerSide + 0.5f) * texelSize.x;
                float v = (tile.id / tilesPerSide + 0.5f) * texelSize.y;
                Vector2 uv = new(u, v);
                var polygon = tile.polygon(Quaternion.identity);
                int centerVertex = addVertex(scaleElevation(tile.v, terrain.elevation), tile.v, uv);
                int[] cornerVertices = new int[tile.edge_count];
                for (int i = 0; i < tile.edge_count; i++) {
                    var c = tile.corners[i];
                    var cter = c.terrain(planet);
                    cornerVertices[i] = addVertex(scaleElevation(c.v, cter.elevation), tile.v, uv + polygon[i] * texelSize / tileRadians);
                }
                Vector3 norm = Vector3.zero;
                for (int i = 0; i < tile.edge_count; i++) {
                    int a = centerVertex;
                    int b = cornerVertices[i];
                    int c = cornerVertices[(i + 1) % tile.edge_count];
                    Plane tilePlane = new(vertices[a], vertices[b], vertices[c]);
                    //Debug.Log($"Vertices {vertices[a]}, {vertices[b]}, {vertices[c]} make plane {tilePlane} with normal {tilePlane.normal}");
                    norm += tilePlane.normal;
                    triangles.AddRange(new[] { a, b, c });
                }
                norm = norm.normalized;
                normals[centerVertex] = norm;
                for (int i = 0; i < tile.edge_count; i++) {
                    normals[cornerVertices[i]] = norm;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

            return mesh;

            int addVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                int idx = vertices.Count;
                vertices.Add(position);
                normals.Add(normal);
                uvs.Add(uv);
                return idx;
            }

            Vector3 scaleElevation(Vector3 v, float elevation)
            {
                if (!modelTopography) return v;
                if (elevation < seaLevel) elevation = 0;
                else elevation -= seaLevel;
                return v * (1 + elevation / radius * elevationScale);
            }

        }

        public void RenderTextures(TextureParameters par, string name)
        {
            textureParameters = par;
            if (tileTexture) tileTexture.Reinitialize(2048, 2048, TextureFormat.RGB24, false);
            else tileTexture = new(2048, 2048, TextureFormat.RGB24, false);
            tileTexture.name = $"{name} [Tile Texture]";
            tileTexture.wrapMode = TextureWrapMode.Clamp;
            var planet = Data.planet;
            int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));
            float pixelsPerTile = 2048f / tilesPerSide;
            int ceilPPT = Mathf.CeilToInt(pixelsPerTile);
            var pixels = new Color[ceilPPT * ceilPPT];
            var colorSpan = pixels.AsSpan();

            tileColors.init_colours(planet);
            int nseasons = planet.season_count();
            if (nseasons > 0) {
                int n = Mathf.FloorToInt(textureParameters.timeOfYear * nseasons * 0.9999f);
                Season season = planet.nth_season(n);
                Debug.Log($"Rendering planet in season {n} with scheme {textureParameters.colourScheme}");
                tileColors.set_colours(planet, season, textureParameters.colourScheme);
            } else {
                Debug.Log($"Rendering planet with scheme {textureParameters.colourScheme}");
                tileColors.set_colours(planet, textureParameters.colourScheme);
            }

            foreach (var tile in planet.tiles()) {
                //Color tileColor = Color.HSVToRGB((tile.id % 12) / 12f, 1 - (float)tile.id / planet.tile_count() / 2f, 1);
                //tileColors.tiles[tile.id] = tileColor;
                colorSpan.Fill(tileColors.tiles[tile.id]);
                int i = tile.id % tilesPerSide;
                int j = tile.id / tilesPerSide;
                int u = Mathf.RoundToInt(i * pixelsPerTile);
                int v = Mathf.RoundToInt(j * pixelsPerTile);
                int du = ceilPPT;
                int dv = ceilPPT;
                //Debug.Log($"Drawing tile {tile.id} with color {tileColor} at {u},{v}+{du}+{dv}");
                tileTexture.SetPixels(u, v, du, dv, pixels);
            }

            tileTexture.Apply();
        }
    }
}
