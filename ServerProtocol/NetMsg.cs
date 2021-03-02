using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerProtocol
{
    [Serializable]
    public class NetMsg
    {
        /// <summary>
        /// 操作码
        /// </summary>
        public int opCode { get; set; }

        /// <summary>
        /// 副操作码
        /// </summary>
        public int subOpCode { get; set; }

        /// <summary>
        /// 传递信息的对象
        /// </summary>
        public object value { get; set; }

        public NetMsg()
        {

        }

        public NetMsg(int opCode, int subOpCode, object value)
        {
            Reset(opCode, subOpCode, value);
        }

        /// <summary>
        /// 重设状态，减少new的使用
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subOpCode"></param>
        /// <param name="value"></param>
        public void Reset(int opCode, int subOpCode, object value)
        {
            this.opCode = opCode;
            this.subOpCode = subOpCode;
            this.value = value;
        }

        /// <summary>
        /// 将字节流反序列化为对象
        /// </summary>
        /// <param name="data">需要解析的字节流</param>
        /// <returns>解析出的对象</returns>
        public static NetMsg Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                NetMsg msg = formatter.Deserialize(stream) as NetMsg;
                return msg;
            }
        }

        /// <summary>
        /// 将对象序列化为字节流
        /// </summary>
        /// <returns>序列化后的字节流</returns>
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                byte[] data = new byte[stream.Length];
                Buffer.BlockCopy(stream.GetBuffer(), 0, data, 0, (int)stream.Length);
                return data;
            }
        }

    }
}
