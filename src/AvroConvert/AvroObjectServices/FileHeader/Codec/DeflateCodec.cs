﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/** Modifications copyright(C) 2022 Adrian Strugala **/

using System.IO;
using System.IO.Compression;

namespace SolTechnology.Avro.AvroObjectServices.FileHeader.Codec
{
    internal class DeflateCodec : AbstractCodec
    {
        internal override string Name { get; } = CodecType.Deflate.ToString().ToLower();

        internal override MemoryStream Compress(MemoryStream toCompress)
        {
            var toCompressBytes = toCompress.ToArray();
            MemoryStream outStream = new MemoryStream();

            using DeflateStream compress = new DeflateStream(outStream, CompressionMode.Compress, true);
            compress.Write(toCompressBytes, 0, toCompressBytes.Length);
            return outStream;
        }

        internal override byte[] Decompress(byte[] compressedData)
        {
            MemoryStream inStream = new MemoryStream(compressedData);
            MemoryStream outStream = new MemoryStream();

            using (DeflateStream decompress =
                        new DeflateStream(inStream,
                        CompressionMode.Decompress))
            {
                CopyTo(decompress, outStream);
            }
            return outStream.ToArray();
        }

        private static void CopyTo(Stream from, Stream to)
        {
            byte[] buffer = new byte[4096];
            int read;
            while ((read = from.Read(buffer, 0, buffer.Length)) != 0)
            {
                to.Write(buffer, 0, read);
            }
        }
    }
}
