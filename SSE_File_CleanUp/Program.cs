using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SSE_File_CleanUp {
    class Program {

        static List<string> keptFiles = new List<string>();
        static List<string> excludedFiles = new List<string>();

        static List<string> excludeList = new List<string>();

        static List<string> textureExtensions = new List<string>();
        static List<string> meshExtensions = new List<string>();

        static string meshPath;
        static string textPath;
        static string excludePath;

        static bool keep;

        static void Main(string[] args) {
            DateTime start = DateTime.Now;
            Console.WriteLine("Starting run");
            string ExcludeListPath = "";
            //Get values from the command line
            var results = Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(options =>
            {
                meshPath = options.meshesDir;
                textPath = options.texturesDir;
                ExcludeListPath = options.exclusionList;
                if (!string.IsNullOrEmpty(options.Output)) excludePath = options.Output;
                else excludePath = "Excluded";

                string mTemp = options.meshExtensions;

                if (!string.IsNullOrEmpty(mTemp)) {
                    mTemp = mTemp.Replace(" ", "");
                    string[] tempArr = mTemp.Split(',');
                    foreach (string str in tempArr) {
                        meshExtensions.Add(str);
                    }
                    
                }
                else {
                    meshExtensions.Add("nif");
                    meshExtensions.Add("tri");
                }

                string tTemp = options.textureExtensions;

                if (!string.IsNullOrEmpty(tTemp)) {
                    tTemp = tTemp.Replace(" ", "");
                    string[] tempArr = tTemp.Split(',');
                    foreach (string str in tempArr) {
                        textureExtensions.Add(str);
                    }

                }
                else {
                    textureExtensions.Add("dds");
                    textureExtensions.Add("tga");
                }

                keep = options.keep;
            }).WithNotParsed<Options>(options =>
            {
                return;
            });


            
            ////Mesh folder 
            //meshPath = @"F:\Test Data\KS Hairdos SSE-6817-1-7-1593347494\meshes";
            ////Texture folder 
            //textPath = @"F:\Test Data\KS Hairdos SSE-6817-1-7-1593347494\textures";
            
            //excludePath = "Excluded";
            ////Exclude List
            //ExcludeListPath = @"F:\Test Data\excludeList.txt";

            bool cont = false;
            if (!string.IsNullOrEmpty(meshPath) && Directory.Exists(meshPath) &&
                !string.IsNullOrEmpty(textPath) && Directory.Exists(textPath) &&
                !string.IsNullOrEmpty(ExcludeListPath) && File.Exists(ExcludeListPath) && 
                (Path.GetExtension(ExcludeListPath) == ".txt" || Path.GetExtension(ExcludeListPath) == ".csv" ) &&
                !string.IsNullOrEmpty(excludePath)) {


                if (Directory.Exists(excludePath) && !IsDirEmpty(excludePath)) {
                    Console.WriteLine("Exclude directory already exists. " +
                        "Please check the directory and either move to back up location, delete the directory, or supply a new Directory name");

                    //Add a prompt to continue anyway
                }
                else {
                    cont = true;
                }
            }
            else {
                if (string.IsNullOrEmpty(meshPath) && !Directory.Exists(meshPath)) {
                    Console.WriteLine("Invalid meshes path.");
                }
                if (string.IsNullOrEmpty(textPath) && !Directory.Exists(textPath)) {
                    Console.WriteLine("Invalid textures path.");
                }
                if (string.IsNullOrEmpty(ExcludeListPath) && !Directory.Exists(ExcludeListPath)) {
                    Console.WriteLine("Invalid exclusion list path.");
                }

                if (Path.GetExtension(ExcludeListPath) != ".txt" && Path.GetExtension(ExcludeListPath) != ".csv") {
                    Console.WriteLine("Exclusion list must be either a txt or csv file");
                }
                if (string.IsNullOrEmpty(excludePath)) {
                    Console.WriteLine("Exclude path cannot be blank");
                }
            }

            if (cont) {
                if (!Path.IsPathFullyQualified(meshPath)) meshPath = Path.GetFullPath(meshPath);
                if (!Path.IsPathFullyQualified(textPath)) textPath = Path.GetFullPath(textPath);
                if (!Path.IsPathFullyQualified(excludePath)) excludePath = Path.GetFullPath(excludePath);
                if (Path.EndsInDirectorySeparator(meshPath)) meshPath = Path.TrimEndingDirectorySeparator(meshPath);
                if (Path.EndsInDirectorySeparator(textPath)) textPath = Path.TrimEndingDirectorySeparator(textPath);
                if (Path.EndsInDirectorySeparator(excludePath)) excludePath = Path.TrimEndingDirectorySeparator(excludePath);

                BuildExcludeList(ExcludeListPath);
                ProcessDirectory(meshPath);
                SaveLists();

                MoveFiles();
            }

            //string filePath = @"F:\Test Data\KS Hairdos SSE-6817-1-7-1593347494\meshes\KS Hairdo's\Siamese.nif";
            //ProcessTextures(filePath, false);

            Console.WriteLine("Finishing run");
            DateTime finish = DateTime.Now;
            Console.WriteLine("Elapsed time: " + (finish - start).TotalSeconds);

            
        }

        static void ProcessTextures(string fileName, bool excluded) {
            //Open file
            Console.WriteLine("Opening file " + fileName);
            byte[] byteContents = File.ReadAllBytes(fileName);

            string contents = System.Text.Encoding.Default.GetString(byteContents);

            foreach (string extension in textureExtensions) {

                //Read file and check for files with the supplied extension.
                Console.WriteLine("Does file reference file with " + extension + " extension?");

                if (contents.Contains(extension)) {

                    Match match = Regex.Match(contents, @"((?:\\[a-zA-Z_\-\s0-9\.']+)+\." + extension + ")");
                    //Match match = Regex.Match(contents, @"((?:[a-zA-Z_\-\s0-9\.']+)?(?:\\[a-zA-Z_\-\s0-9\.']+)+\." + extension + ")");
                    while (match.Success) {
                        Console.WriteLine(match);

                        ////Add textures to collection based on exclusion.

                        if (excluded) {
                            if (keptFiles.Contains(textPath + match.Value) || excludedFiles.Contains(textPath + match.Value)) {
                                //Do Nothing
                                Console.WriteLine("Excluded texture file already accounted for.");
                            }
                            else {
                                excludedFiles.Add(textPath + match.Value);
                                Console.WriteLine("Texture file added to exclusion list");
                            }
                        }
                        else {
                            if (keptFiles.Contains(textPath + match.Value)) {
                                //Do nothing
                                Console.WriteLine("Texture file already in kept list");
                            }
                            else if (excludedFiles.Contains(textPath + match.Value)) {
                                excludedFiles.Remove(textPath + match.Value);
                                keptFiles.Add(textPath + match.Value);
                                Console.WriteLine("Texture file removed from excluded list and added to kept list");
                            }
                            else {
                                keptFiles.Add(textPath + match.Value);
                                Console.WriteLine("Texture file added to kept list");
                            }
                        }


                        match = match.NextMatch();
                    }
                }
                else {
                    Console.WriteLine("No references found");
                }

            }


        }

        static void BuildExcludeList(string excludeListPath) {
            Console.WriteLine("Building Exclusion list from provided text document");
            StreamReader reader = new StreamReader(excludeListPath);

            string line;

            while ((line = reader.ReadLine()) != null) {
                if (!String.IsNullOrEmpty(line)) {
                    Console.WriteLine(line);
                    if (Path.GetExtension(excludeListPath) == ".txt") {
                        foreach (string extension in meshExtensions) {
                            if ((Path.GetExtension(line) == @"." + extension) &&
                                !excludeList.Contains(line)) {
                                excludeList.Add(meshPath + @"\" + line);
                                excludedFiles.Add(meshPath + @"\" + line);
                            }
                        }
                    }
                    else if (Path.GetExtension(excludeListPath) == ".csv") {
                        string[] temp = line.Split(',');
                        foreach (string str in temp) {
                            foreach (string extension in meshExtensions) {
                                if ((Path.GetExtension(str) == @"." + extension) &&
                                !excludeList.Contains(str)) {
                                    excludeList.Add(meshPath + @"\" + str);
                                    excludedFiles.Add(meshPath + @"\" + str);
                                }
                            }
                        }
                    }
                    
                }
            }
            Console.WriteLine("Finished building Exclusion list");
        }

        static void ProcessDirectory(string DirectoryPath) {
            string[] files = Directory.GetFiles(DirectoryPath);

            if (files.Length > 0) {
                foreach (string str in files) {
                    ProcessFile(str);
                }
            }

            string[] subDirs = Directory.GetDirectories(DirectoryPath);
            if (subDirs.Length > 0) {
                foreach (string str in subDirs) {
                    ProcessDirectory(str);
                }
            }
        }

        static void ProcessFile(string filePath) {
            bool exclude = false;
            
            if (excludeList.Contains(filePath)) {
                Console.WriteLine("File is excluded");
                exclude = true;
            }
            else {
                keptFiles.Add(filePath);
                Console.WriteLine("File is kept");
            }

            ProcessTextures(filePath, exclude);
        }

        static void MoveFiles() {
            if (Directory.Exists(excludePath))
                Directory.Delete(excludePath);

            Directory.CreateDirectory(excludePath);

            StreamWriter errorLog = new StreamWriter(excludePath + @"\error.txt");

            foreach (string file in excludedFiles) {
                try {
                    if (File.Exists(file)) {

                        string target = "";
                        if (file.ToLower().Contains("meshes")) {
                            target = excludePath + @"\meshes\" + file.Replace(meshPath + @"\", "");
                        }
                        else if (file.ToLower().Contains("textures")) {
                            target = excludePath + @"\textures\" + file.Replace(textPath + @"\", "");
                        }

                        Console.WriteLine("Copying " + file + " to " + target);

                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.Copy(file, target);
                    }
                    else {
                        errorLog.WriteLine("File not found: " + file);
                    }

                    if (!keep) File.Delete(file);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    errorLog.WriteLine(e.Message);
                }
            }

            errorLog.Close();

        }

        static void SaveLists() {
            using (StreamWriter exclude = new StreamWriter("exclude.txt"))
                foreach (string str in excludedFiles) exclude.WriteLine(str);

            using (StreamWriter keep = new StreamWriter("keep.txt"))
                foreach (string str in keptFiles) keep.WriteLine(str);
        }

        static bool IsDirEmpty(string dir) {
            bool empty = false;

            string[] files = Directory.GetFiles(dir);
            string[] subDirs = Directory.GetDirectories(dir);

            if (files.Length == 0 && subDirs.Length == 0) empty = true;

            return empty;
        }
    }

    class Options {
        //-m MeshesDir
        [Option('m', "meshesDir")]
        public string meshesDir { get; set; }

        //-t TexturesDir
        [Option('t', "texturesDir")]
        public string texturesDir { get; set; }

        //-l ExclusionList
        [Option('l', "exclusionList")]
        public string exclusionList { get; set; }

        //-o OutputDir [OPTIONAL]
        [Option('o', "outputDir", Default = "Excluded")]
        public string Output { get; set; }

        //-i meshExt [OPTIONAL]
        [Option('i', "meshExtensions", Separator = ',', Default = "nif,tri" )]
        public string meshExtensions { get; set; }

        //-e textureExt [OPTIONAL]
        [Option('e', "textureExtensions", Separator = ',', Default =  "dds,tga" )]
        public string textureExtensions { get; set; }

        //-k keep files [OPTIONAL]
        [Option('k', "keep")]
        public bool keep { get; set; }
    }
}
