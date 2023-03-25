//using MusicLibrary.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Security.Cryptography;
//using System.Threading.Tasks;

//namespace MusicLibrary
//{
//    public class MusicFileManager
//    {
//        public static string FilePath { get; set; } = @".\Download";
//        public static bool Contain(string md5)
//        {
//            return false;
//        }

//        public static void RecordMetaData(List<SongFileMetum> songMetaDatas)
//        {
//            using var context = new MusicLibraryContext();
//            context.SongFileMeta.AddRange(songMetaDatas);

//        }

//        public static async Task SaveFileStreamAsync(string fileName, Stream dataStream)
//        {
//            string fielPath = Path.Combine(FilePath, fileName);
//            Directory.CreateDirectory(Path.GetDirectoryName(fielPath));
//            using Stream stream = File.Open(fielPath, FileMode.Create);

//            //Console.WriteLine($"md5 from stream: {CalculateMD5(dataStream)}");
//            await dataStream.CopyToAsync(stream);
//        }

//        public static string CalculateMD5(Stream stream)
//        {
//            using MD5 md5 = MD5.Create();
//            return Convert.ToHexString(md5.ComputeHash(stream));
//        }
//    }
//}
