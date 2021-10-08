using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace _ThrowBattle
{
    /// <summary>
    /// Data holder for multiplayer games. Edit or inherit this class to use in your game.
    /// </summary>
    [Serializable]
    public class MultiplayerGameData
    {
        public string playerID;
        public float force;
        public float angle;
        public float angleForUI;
        public float shootDirectionX;
        public float shootDirectionY;
        public float shootPositionX;
        public float shootPositionY;
        public float otherPlayerPositionX;
        public float otherPlayerPositionY;
        /// <summary>
        /// Convert <see cref="MultiplayerGameData"/> to <see cref="byte[]"/>.
        /// </summary>
        public virtual byte[] ToByteArray()
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, this);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error orcurs when trying to convert MultiplayerGameData to byte array: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Convert <see cref="byte[]"/> to <see cref="MultiplayerGameData"/>.
        /// </summary>
        public static MultiplayerGameData FromByteArray(byte[] bytes)
        {
            try
            {
                if (bytes == null)
                    throw new ArgumentNullException();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    memoryStream.Write(bytes, 0, bytes.Length);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    object obj = binaryFormatter.Deserialize(memoryStream);
                    return obj is MultiplayerGameData ? obj as MultiplayerGameData : null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error orcurs when trying to convert byte array to MultiplayerGameData: " + e.Message);
                return null;
            }
        }
    }

    public static class MultiplayerGameDataExtension
    {
        /// <summary>
        /// Convert <see cref="byte[]"/> to <see cref="MultiplayerGameData"/>.
        /// </summary>
        public static MultiplayerGameData ToMultiplayerGameData(this byte[] bytes)
        {
            return MultiplayerGameData.FromByteArray(bytes);
        }
    }
}
