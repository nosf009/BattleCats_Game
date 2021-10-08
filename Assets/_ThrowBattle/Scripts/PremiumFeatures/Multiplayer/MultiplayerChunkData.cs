using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace _ThrowBattle
{
    /// <summary>
    /// If your data in <see cref="MultiplayerGameData"/> is too large,
    /// you can use this class to break it into smaller pieces.
    /// </summary>
    [Serializable]
    public class MultiplayerChunkData : MultiplayerGameData
    {
        /// <summary>
        /// Broken from original data, used to join back into original data.
        /// </summary>
        public byte[] ChunkData { get; private set; }

        /// <summary>
        /// Index to join back to original data.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Number of pieces the original data was broken into.
        /// </summary>
        public int PiecesCount { get; private set; }

        /// <summary>
        /// Unique stamp, only join chunks with same stamp value.
        /// </summary>
        public int Stamp { get; private set; }

        /// <summary>
        /// <see cref="ChunkData"/>'s length.
        /// </summary>
        public int Size { get { return ChunkData != null ? ChunkData.Length : 0; } }

        private MultiplayerChunkData(byte[] data, int index, int piecesCount, int stamp)
        {
            ChunkData = data;
            Index = index;
            PiecesCount = piecesCount;
            Stamp = stamp;
        }

        /// <summary>
        /// Break <see cref="MultiplayerGameData"/> into smaller chunks <see cref="MultiplayerChunkData"/>.
        /// </summary>
        /// <param name="chunkSize">Chunk size (byte).</param>
        public static List<MultiplayerChunkData> ToChunks(MultiplayerGameData data, int chunkSize = 2048)
        {
            if (data == null)
                return null;

            if (chunkSize <= 0)
            {
                Debug.LogError("Chunk size must be bigger than 0.");
                return null;
            }

            var originalBytes = data.ToByteArray();
            if (originalBytes == null)
            {
                Debug.LogError("Couldn't convert MultiplayerGameData to byte array.");
                return null;
            }

            var stamp = GenerateStamp();
            var originalSize = originalBytes.Length;

            if (originalSize <= chunkSize)
            {
                Debug.LogWarning("The data size isn't bigger than the chunk size, no need to break it.");
                return new List<MultiplayerChunkData>() { new MultiplayerChunkData(originalBytes, 0, 1, stamp) };
            }

            List<MultiplayerChunkData> chunks = new List<MultiplayerChunkData>();
            int piecesCount = (originalSize / chunkSize) + (originalSize % chunkSize != 0 ? 1 : 0);
            int startIndex = 0, remainedSize = originalSize;
            for(int i = 0; i < piecesCount; i++)
            {
                var nextChunkSize = remainedSize >= chunkSize ? chunkSize : remainedSize;
                var chunkData = new byte[nextChunkSize];
                Array.Copy(originalBytes, startIndex, chunkData, 0, nextChunkSize);
                var chunk = new MultiplayerChunkData(chunkData, i, piecesCount, stamp);
                remainedSize -= nextChunkSize;
                startIndex += nextChunkSize;
                chunks.Add(chunk);
            }

            return chunks;
        }

        /// <summary>
        /// Join all chunks <see cref="MultiplayerChunkData"/> into one <see cref="MultiplayerGameData"/>.
        /// </summary>
        public static MultiplayerGameData FromChunks(IEnumerable<MultiplayerChunkData> chunks)
        {
            var stamp = chunks.First().Stamp;
            var validation = chunks.Any(chunk => chunk == null || chunk.ChunkData == null || chunk.Stamp != stamp || chunk.PiecesCount != chunks.Count());
            if (validation)
            {
                Debug.LogError("Invalid chunks!!!");
                return null;
            }

            var sortedChunks = chunks.OrderBy(chunk => chunk.Index);
            var originalSize = chunks.Sum(chunk => chunk.Size);
            var newData = new byte[originalSize];
            var currentIndex = 0;
            foreach(var chunk in sortedChunks)
            {
                Array.Copy(chunk.ChunkData, 0, newData, currentIndex, chunk.Size);
                currentIndex += chunk.Size;
            }

            return newData.ToMultiplayerGameData();
        }

        private static int GenerateStamp()
        {
            return DateTime.Now.GetHashCode() + UnityEngine.Random.Range(1000, 9999);
        }
    }

    public static class MultiplayerChunkDataExtension
    {
        /// <summary>
        /// Break <see cref="MultiplayerGameData"/> into smaller chunks <see cref="MultiplayerChunkData"/>.
        /// </summary>
        /// <param name="chunkSize">Chunk size (byte).</param>
        public static List<MultiplayerChunkData> ToChunks(this MultiplayerGameData data, int chunkSize = 2048)
        {
            return MultiplayerChunkData.ToChunks(data, chunkSize);
        }

        /// <summary>
        /// Join all chunks <see cref="MultiplayerChunkData"/> into one <see cref="MultiplayerGameData"/>.
        /// </summary>
        public static MultiplayerGameData ToMultiplayerGameData(this IEnumerable<MultiplayerChunkData> chunks)
        {
            return MultiplayerChunkData.FromChunks(chunks);
        }
    }
}
