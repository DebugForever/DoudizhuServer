using System;
using System.Collections.Generic;
using System.IO;

namespace ServerProtocol
{
    public static class EncodingTools
    {
        /// <summary>
        /// 把字节流封装成数据包，解决粘包等问题
        /// </summary>
        /// <param name="data">要发送的字节流</param>
        /// <returns></returns>
        public static byte[] Encode(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(data.Length);
                    writer.Write(data);
                    byte[] packet = new byte[stream.Length];
                    Buffer.BlockCopy(stream.GetBuffer(), 0, packet, 0, (int)stream.Length);

                    return packet;
                }
            }
        }

        /// <summary>
        /// 从传入list中取出一个数据包还原为字节流，留下剩余数据
        /// </summary>
        /// <param name="cache">需要解析的数据包（字节流）</param>
        /// <returns>解析出的字节流，检测不到数据包则返回null</returns>
        public static byte[] Decode(ref List<byte> cache)
        {
            if (cache.Count < sizeof(Int32))
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream(cache.ToArray()))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int length = reader.ReadInt32();

                    int remainLength = (int)(stream.Length - stream.Position);
                    if (length > remainLength)
                    {
                        //剩余长度不够标识的长度，读取失败
                        return null;
                    }

                    byte[] data = reader.ReadBytes(length);

                    //将剩余数据写回传入list，这里直接删掉读取数据即可
                    cache.RemoveRange(0, (int)stream.Position);

                    return data;
                }
            }
        }

    }
}
