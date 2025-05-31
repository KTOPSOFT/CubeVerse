using InsaneScatterbrain.DataStructures;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public static class ConnectionsNodeView
    {
        public static TextureData GetConnectionsTextureData(Pair<Vector2Int>[] connections)
        {
            // Calculate the max width and height for a texture to fit the connections on.
            var width = 1;
            var height = 1;

            foreach (var connection in connections)
            {
                // The farthest x and y values are the width and height of the texture.
                width = Mathf.Max(width, connection.First.x);
                width = Mathf.Max(width, connection.Second.x);

                height = Mathf.Max(height, connection.First.y);
                height = Mathf.Max(height, connection.Second.y);
            }

            var textureData = new TextureData();
            textureData.Set(width, height);

            // Fill the texture with a completely black background
            textureData.Fill(Color.black);

            // Draw the connections in white.
            foreach (var connection in connections)
            {
                textureData.DrawLine(connection.First, connection.Second, Color.white);
            }

            return textureData;
        }

        /// <summary>
        /// Returns a default texture for the connections.
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <returns>The texture.</returns>
        public static Texture2D GetConnectionsTexture(Pair<Vector2Int>[] connections)
        {
            var textureData = GetConnectionsTextureData(connections);

            return textureData.ToTexture2D();
        }
    }
}