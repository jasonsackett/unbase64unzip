using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logdecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                DecodeLog(args[0]);
            }
            else
            {
                System.Console.WriteLine("logdecoder takes an input file,\n and runs a System.Convert.FromBase64String on it,\n and runs System.IO.Compression.GZipStream Decompress on the result.\n The output is saved to '{input}x.txt'\n usage: logdecoder.exe {inputfile}");
            }
        }

        public static void DecodeLog(string file)
        {
            try
            {
                var fs = new FileStream(file, FileMode.Open);
                if (fs.CanRead)
                {
                    System.Console.WriteLine("reading file: " + file);
                    const int blockSize = 16384;
                    int offset = 0;
                    int lengthToRead = blockSize;
                    byte[] fullread = new byte[fs.Length];
                    var msFullread = new MemoryStream(fullread);
                    while (fs.Length > offset)
                    {
                        lengthToRead = blockSize;
                        if (offset + blockSize > fs.Length)
                        {
                            lengthToRead = (int)fs.Length - offset;
                        }
                        byte[] chunk = new byte[blockSize];
                        var read = fs.Read(chunk, offset, lengthToRead);
                        msFullread.Write(chunk, offset, read);
                        offset += read;
                    }
                    byte[] uncompressedandencoded = Compress.decompress(Compress.Base64Decode(Compress.GetString(fullread)));
                    string nameAppend = "x.txt";
                    var fsout = new FileStream(file + nameAppend, FileMode.Create);
                    if (fsout.CanWrite)
                    {
                        fsout.Write(uncompressedandencoded, 0, uncompressedandencoded.Length);
                        System.Console.WriteLine("wrote file: " + file + nameAppend);
                    }
                    else
                    {
                        System.Console.WriteLine("cannot write output file: " + file + nameAppend);
                    }
                }
                else
                {
                    System.Console.WriteLine("cannot read input file: " + file);
                }
            }
            catch(Exception e)
            {
                System.Console.WriteLine("exception: " + e.Message);
            }
        }
    }

    public class Compress
    {
        public static byte[] compress(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, buffer.Length);
            zip.Dispose();
            ms.Position = 0;

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }
        public static byte[] decompress(byte[] gzBuffer)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }
        public static string Base64Encode(byte[] buf)
        {
            return Convert.ToBase64String(buf);
        }
        public static byte[] Base64Decode(string buf)
        {
            return Convert.FromBase64String(buf);
        }
        public static byte[] GetBytes(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}
