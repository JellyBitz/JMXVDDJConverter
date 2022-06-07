using System;
using System.Collections.Generic;
using System.IO;

namespace JMXVDDJConverter
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Not files specified
            if (args.Length == 0)
                return;

            // Get file paths
            List<string> files = new List<string>();
            foreach (var arg in args)
            {
                if (File.Exists(arg))
                    files.Add(arg);
                else if (Directory.Exists(arg))
                    files.AddRange(Directory.GetFiles(arg));
            }

            // Convert each path provided
            foreach (string path in files)
            {
                try
                {
                    FileInfo file = new FileInfo(path);

                    // Try to read header
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs, System.Text.Encoding.ASCII))
                    {
                        // Check if file is JMXVDDJ
                        if (br.BaseStream.Length > 20)
                        {
                            // Reset cursor
                            br.BaseStream.Seek(0, SeekOrigin.Begin);

                            string header = new string(br.ReadChars(12));
                            if (header == "JMXVDDJ 1000")
                            {
                                // Try to convert file
                                try
                                {
                                    Console.WriteLine("Converting \"" + path + "\" (DDJ)");
                                    ConvertDDJ(file, fs);
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error: " + ex.Message);
                                }
                            }
                        }
                        // Check if file is DDS
                        if (br.BaseStream.Length > 4)
                        {
                            // Reset cursor
                            br.BaseStream.Seek(0, SeekOrigin.Begin);

                            string header = new string(br.ReadChars(4));
                            if (header == "DDS ")
                            {
                                // Try to convert file
                                try
                                {
                                    Console.WriteLine("Converting \"" + path + "\" (DDS)");
                                    ConvertDDS(file, fs);
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error: " + ex.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error, unable to check file: " + ex.Message);
                    continue;
                }
            }
        }

        #region Private Helpers
        /// <summary>
        /// Convert DDJ buffer into DDS
        /// </summary>
        private static void ConvertDDJ(FileInfo file, FileStream fileStream)
        {
            // new file path to be created
            string name = file.Name.Remove(file.Name.Length - file.Extension.Length) + ".dds";
            if (name == file.Name)
                name += ".dds";
            string newPath = Path.Combine(file.DirectoryName, name);
            // Set cursor to remove DDJ header
            fileStream.Seek(20, SeekOrigin.Begin);
            using (var fs = new FileStream(newPath, FileMode.Create, FileAccess.Write))
            {
                fileStream.CopyTo(fs);
            }
        }
        /// <summary>
        /// Convert DDS buffer into DDJ
        /// </summary>
        private static void ConvertDDS(FileInfo file, FileStream fileStream)
        {
            // new file path to be created
            string name = file.Name.Remove(file.Name.Length - file.Extension.Length) + ".ddj";
            if (name == file.Name)
                name += ".ddj";
            string newPath = Path.Combine(file.DirectoryName, name);
            // Set cursor
            fileStream.Seek(0, SeekOrigin.Begin);
            using (var fs = new FileStream(newPath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs, System.Text.Encoding.ASCII))
            {
                bw.Write("JMXVDDJ 1000".ToCharArray());
                bw.Write((int)fileStream.Length + 8);
                bw.Write(3); // 3 = Texture
                fileStream.CopyTo(fs);
            }
        }
        /// <summary>
        /// Skips bytes reading from current position
        /// </summary>
        public static void SkipRead(this BinaryReader BinaryReader, long count)
        {
            BinaryReader.BaseStream.Seek(count, SeekOrigin.Current);
        }
        #endregion
    }
}