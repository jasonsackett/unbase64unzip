/**
 * Copyright (c) 2013 Jason Sackett. All rights reserved.
 * Use of this source code is governed by a GPLv2 license that can be
 * found in the LICENSE file.
 **/
 
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

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
                Console.WriteLine("logdecoder takes an input file,\n and runs a System.Convert.FromBase64String on it,\n and runs System.IO.Compression.GZipStream Decompress on the result.\n The output is saved to '{input}x.txt'\n usage: logdecoder.exe {inputfile}");
            }
        }

        public static void DecodeLog(string file)
        {
            try
            {
                var fs = new FileStream(file, FileMode.Open);
                if (fs.CanRead)
                {
                    Console.WriteLine("reading file: " + file);
                    const int blockSize = 16384;
                    var offset = 0;
                    var fullread = new byte[fs.Length];
                    var msFullread = new MemoryStream(fullread);
                    while (fs.Length > offset)
                    {
                        var lengthToRead = blockSize;
                        if (offset + blockSize > fs.Length)
                        {
                            lengthToRead = (int)fs.Length - offset;
                        }
                        var chunk = new byte[blockSize];
                        var read = fs.Read(chunk, offset, lengthToRead);
                        msFullread.Write(chunk, offset, read);
                        offset += read;
                    }
                    var uncompressedandencoded = Compress.Decompress(Compress.Base64Decode(Compress.GetString(fullread)));
                    const string nameAppend = "x.txt";
                    var fsout = new FileStream(file + nameAppend, FileMode.Create);
                    if (fsout.CanWrite)
                    {
                        fsout.Write(uncompressedandencoded, 0, uncompressedandencoded.Length);
                        Console.WriteLine("wrote file: " + file + nameAppend);
                    }
                    else
                    {
                        Console.WriteLine("cannot write output file: " + file + nameAppend);
                    }
                }
                else
                {
                    Console.WriteLine("cannot read input file: " + file);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("exception: " + e.Message);
            }
        }
    }

    public class Compress
    {
/*
        public static byte[] Compress(byte[] buffer)
        {
            var ms = new MemoryStream();
            var zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, buffer.Length);
            zip.Dispose();
            ms.Position = 0;

            var compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            var gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }
*/
        public static byte[] Decompress(byte[] gzBuffer)
        {
            var ms = new MemoryStream();
            var msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            var buffer = new byte[msgLength];

            ms.Position = 0;
            var zip = new GZipStream(ms, CompressionMode.Decompress);
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
            return Encoding.UTF8.GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}
