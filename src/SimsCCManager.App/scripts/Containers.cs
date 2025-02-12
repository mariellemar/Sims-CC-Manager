using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.Packages.Containers;
using SimsCCManager.Settings.Loaded;
using SimsCCManager.Settings.SettingsSystem;

namespace SimsCCManager.Containers
{
    [XmlInclude(typeof(Sims2Instance))]
    [XmlInclude(typeof(Sims3Instance))]
    [XmlInclude(typeof(Sims4Instance))]
    public abstract class GameInstanceBase
    {
        protected Guid identifier;
        public abstract Guid Identifier {get; set;}
        protected Games gamechoice;
        public abstract Games GameChoice {get; set;}
        protected string gameversion;
        public abstract string GameVersion {get; set;}
        protected string gameinstallfolder;
        public abstract string GameInstallFolder {get; set;}
        protected string gameexe;
        public abstract string GameExe {get; set;}
        protected string gamedocumentsfolder;
        public abstract string GameDocumentsFolder {get; set;}
        protected string exepath;
        public abstract string ExePath {get; set;}
        protected string instancefolder;
        public abstract string InstanceFolder {get; set;}
        protected string instancename;
        public abstract string InstanceName {get; set;}
        protected string instancepackagesfolder;
        public abstract string InstancePackagesFolder {get; set;}
        protected string instancedatafolder;
        public abstract string InstanceDataFolder {get; set;}
        protected string instancedownloadsfolder;
        public abstract string InstanceDownloadsFolder {get; set;}
        protected string instanceprofilesfolder;
        public abstract string InstanceProfilesFolder {get; set;}
        protected string instancesharedgamedatafolder;
        public abstract string InstanceSharedGameDataFolder {get; set;}
        protected List<Category> categories;
        public abstract List<Category> Categories {get; set;}
        protected List<Executable> executables;
        public abstract List<Executable> Executables {get; set;}
        protected List<string> profiles;
        public abstract List<string> Profiles {get; set;}
        protected string activeprofile;
        public abstract string ActiveProfile {get; set;}


        public GameInstanceBase(){
            categories = new List<Category>();
            executables = new List<Executable>();  
            profiles = new List<string>();
        }

        public string XMLfile(){
            return Path.Combine(InstanceFolder, "Instance.xml");
        }

        public abstract void TestInstance();
        

        public void SetCoreDirectories(){
            InstanceDownloadsFolder = Path.Combine(InstanceFolder, "Downloads");;
            InstanceDataFolder = Path.Combine(InstanceFolder, "Data");
            InstancePackagesFolder = Path.Combine(InstanceFolder, "Packages");
            InstanceProfilesFolder = Path.Combine(InstanceFolder, "Profiles");
            InstanceSharedGameDataFolder = Path.Combine(InstanceFolder, "SharedGameData");            
        }

        public void BuildInstanceCore(){
            if (Directory.Exists(InstanceFolder)){
                string renamed = RenameExistingFolder(InstanceFolder);
                Directory.Move(InstanceFolder, renamed);
                var old = LoadedSettings.SetSettings.Instances.Where(x => x.InstanceLocation == InstanceFolder);
                if (old.Any()){
                    LoadedSettings.SetSettings.Instances[LoadedSettings.SetSettings.Instances.IndexOf(old.First())].InstanceLocation = renamed;
                    SettingsFileManagement.SaveSettings();
                }
            }
            string defaultprofilefolder = Path.Combine(InstanceProfilesFolder, "Default");
            Directory.CreateDirectory(defaultprofilefolder);
            Directory.CreateDirectory(InstanceFolder);
            Directory.CreateDirectory(InstanceDownloadsFolder);
            Directory.CreateDirectory(InstanceDataFolder);
            Directory.CreateDirectory(InstancePackagesFolder);
            Directory.CreateDirectory(InstanceProfilesFolder);
            Directory.CreateDirectory(InstanceSharedGameDataFolder);
            //Identifier = Guid.NewGuid();
            string nm = "";
            if (GameChoice == Games.Sims2) nm = "The Sims 2";
            if (GameChoice == Games.Sims3) nm = "The Sims 3";
            if (GameChoice == Games.Sims4) nm = "The Sims 4";
            Executables.Add(new Executable() { Path = ExePath, Exe = GameExe, Arguments = "", Selected = true, Name = nm});
            Categories.Add(new Category() { Name = "Default", Description = "The default category.", Background = new Godot.Color(Color.FromHtml("DDDDDD")), TextColor = new Godot.Color(Color.FromHtml("222222"))});
            Profiles.Add("Default");
        }

        public void BuildInstanceCoreFromCurrent(){
            if (Directory.Exists(InstanceFolder)){
                string renamed = RenameExistingFolder(InstanceFolder);
                Directory.Move(InstanceFolder, renamed);
                var old = LoadedSettings.SetSettings.Instances.Where(x => x.InstanceLocation == InstanceFolder);
                if (old.Any()){
                    LoadedSettings.SetSettings.Instances[LoadedSettings.SetSettings.Instances.IndexOf(old.First())].InstanceLocation = renamed;
                    SettingsFileManagement.SaveSettings();
                }
            }
            string defaultprofilefolder = Path.Combine(InstanceProfilesFolder, "Default");
            Directory.CreateDirectory(defaultprofilefolder);
            Directory.CreateDirectory(InstanceFolder);
            Directory.CreateDirectory(InstanceDownloadsFolder);
            Directory.CreateDirectory(InstanceDataFolder);
            Directory.CreateDirectory(InstanceProfilesFolder);
            Directory.CreateDirectory(InstanceSharedGameDataFolder);
            //Identifier = Guid.NewGuid();
            string nm = "";
            if (GameChoice == Games.Sims2) nm = "The Sims 2";
            if (GameChoice == Games.Sims3) nm = "The Sims 3";
            if (GameChoice == Games.Sims4) nm = "The Sims 4";
            Executables.Add(new Executable() { Path = ExePath, Exe = GameExe, Arguments = "", Selected = true, Name = nm});
            Categories.Add(new Category() { Name = "Default", Description = "The default category.", Background = new Godot.Color(Color.FromHtml("DDDDDD")), TextColor = new Godot.Color(Color.FromHtml("222222"))});
            Profiles.Add("Default");
        }

        public abstract void MakeFolderTree();
        public abstract void CreateFromCurrent();

        public abstract void WriteXML();

        public static string RenameExistingFolder(string folder){
            if (Directory.Exists(folder)){
                string renamed = string.Format("{0}_Backup", folder);
                return renamed = RenameExistingFolder(renamed);
            } else {
                return folder;
            }
        }

        public dynamic GetProperty(string propName){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking {0} property", propName));
            var prop = this.ProcessProperty(propName);
            PropertyInfo property = this.GetType().GetProperty(propName);
            if (property.PropertyType == typeof(Games)){
                Games game = (Games)prop;
                if (game == Games.Sims1){
                    return "Sims 1";
                } else if (game == Games.Sims2){
                    return "Sims 2";
                } else if (game == Games.Sims3){
                    return "Sims 3";
                } else if (game == Games.Sims4){
                    return "Sims 4";
                } else if (game == Games.SimCity5){
                    return "SimCity 5";
                } else if (game == Games.SimsMedieval){
                    return "Sims Medieval";
                } else {
                    return "none";
                }
            } else if (property.PropertyType == typeof(string)){
                return prop;
            } else if (property.PropertyType == typeof(Guid)){
                return prop.ToString();
            } else if (property.PropertyType == typeof(DateTime)){
                DateTime dt = (DateTime)prop;                
                return dt.ToString("MM/dd/yyyy H:mm");
            } else if (property.PropertyType == typeof(bool)){
                return prop;
            } else {
                return null;
            }
        }

        public void SetProperty(string propName, dynamic input){
            var prop = this.ProcessProperty(propName);
            PropertyInfo property = this.GetType().GetProperty(propName);
            if (property != null){
                if (property.PropertyType == typeof(Godot.Color)){
                    Godot.Color newcolor = Godot.Color.FromHtml(input);
                    property.SetValue(this, newcolor);
                } else if (property.PropertyType == typeof(Guid)){
                    if (input.GetType() == typeof(string)){
                        string inp = input as string;
                        property.SetValue(this, Guid.Parse(inp));
                    } else if (input.GetType() == typeof(Guid)){
                        property.SetValue(this, input);
                    }
                } else if (property.PropertyType == typeof(string)) {
                    property.SetValue(this, input as string);
                } else if (property.PropertyType == typeof(bool)) {
                    if (input.GetType() == typeof(bool)) {
                        property.SetValue(this, input);
                    } else if (input.GetType() == typeof(string)){
                        property.SetValue(this, bool.Parse(input));
                    }                    
                } else if (property.PropertyType == typeof(Games)){
                    if (input.GetType() == typeof(Games)){
                        property.SetValue(this, input);
                    } else if (input.GetType() == typeof(string)) {
                        string data = (string)input;
                        if (data == "Sims 1") {
                            property.SetValue(this, Games.Sims1);
                        } else if (data == "Sims 2") {
                            property.SetValue(this, Games.Sims2);
                        } else if (data == "Sims 3") {
                            property.SetValue(this, Games.Sims3);
                        } else if (data == "Sims 4") {
                            property.SetValue(this, Games.Sims4);
                        } else if (data == "Sims Medieval" || data == "SimsMedieval") {
                            property.SetValue(this, Games.SimsMedieval);
                        } else if (data == "Simcity 5" || data == "SimCity 5" || data == "SimCity5") {
                            property.SetValue(this, Games.SimCity5);
                        } else if (data == "none" || data == "" || data == "null") {
                            property.SetValue(this, Games.Null);
                        } else {
                            property.SetValue(this, Games.Null);
                        }
                    }             
                }
            }            
        }

        public object ProcessProperty(string propName){
            return this.GetType().GetProperty(propName).GetValue (this, null);
        }
    }

    public class Sims2Instance : GameInstanceBase {
        public override Guid Identifier{ 
            get { return identifier; } set { identifier = value; }
        }
        public override Games GameChoice{ 
            get { return gamechoice; } set { gamechoice = value; }
        }
        public override string GameVersion{ 
            get { return gameversion; } set { gameversion = value; }
        }
        public override string GameInstallFolder{
            get { return gameinstallfolder; } set { gameinstallfolder = value; } 
        }
        public override string GameExe{
            get { return gameexe; } set { gameexe = value; } 
        }
        public override string GameDocumentsFolder{ 
            get { return gamedocumentsfolder; } set { gamedocumentsfolder = value; } 
        }
        public override string ExePath{ 
            get { return exepath; } set { exepath = value; } 
        }
        public override string InstanceFolder{ 
            get { return instancefolder; } set { instancefolder = value; } 
        }
        public override string InstanceName{ 
            get { return instancename; } set { instancename = value; } 
        }
        public override string InstanceDownloadsFolder{ 
            get { return instancedownloadsfolder; } set { instancedownloadsfolder = value; } 
        }
        public override string InstanceDataFolder{ 
            get { return instancedatafolder; } set { instancedatafolder = value; } 
        }
        public override string InstancePackagesFolder{ 
            get { return instancepackagesfolder; } set { instancepackagesfolder = value; } 
        }
        public override string InstanceProfilesFolder{ 
            get { return instanceprofilesfolder; } set { instanceprofilesfolder = value; } 
        }
        public override string InstanceSharedGameDataFolder{ 
            get { return instancesharedgamedatafolder; } set { instancesharedgamedatafolder = value; } 
        }
        public override List<Category> Categories{ 
            get { return categories; } set { categories = value; } 
        }
        public override List<Executable> Executables{ 
            get { return executables; } set { executables = value; } 
        }
        public override List<string> Profiles{ 
            get { return profiles; } set { profiles = value; } 
        }

        public override string ActiveProfile { 
            get {return activeprofile; } set { activeprofile = value; }
        }

        public string DownloadsFolder {get; set;} = "";

        //data
        
        public string TerrainsFolder {get; set;} = "";
        public string LotCatalogFolder {get; set;} = "";
        public string CollectionsFolder {get; set;} = "";
        public string PackagedLotsFolder {get; set;} = "";
        //media        
        public string ScreenshotsFolder {get; set;} = "";
        public string MusicFolder {get; set;} = "";        
        public string MoviesFolder {get; set;} = "";

        //settings
        public string ConfigFolder {get; set;} = "";
        public string CamerasFolder {get; set;} = "";

        //saves
        
        public string NeighborhoodsFolder {get; set;} = "";      
        
               
        

        public List<string> CacheFiles {get; set; } = new List<string>();
        public List<string> ThumbnailsFiles {get; set;} = new List<string>();
        


        public void BuildInstance(bool createfromcurrent, Guid ident){
            new Thread (() => {
                this.Identifier = ident;
                SetCoreDirectories();
                MakeFolderTree();
                if (createfromcurrent){
                    BuildInstanceCoreFromCurrent();
                    CreateFromCurrent();
                } else {
                    BuildInstanceCore();
                }
                WriteXML();
            }){IsBackground = true}.Start();

        }

        public override void CreateFromCurrent()
        {
            //reimplement



        }


        public override void TestInstance()
        {
            StringBuilder sb = new();
            sb.AppendLine(MoviesFolder);
            sb.AppendLine(DownloadsFolder);
            sb.AppendLine(TerrainsFolder);
            sb.AppendLine(InstanceFolder);
            sb.AppendLine(InstanceDataFolder);
            sb.AppendLine(GameExe);
            foreach (string cache in CacheFiles){
                sb.AppendLine(cache);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(sb.ToString());
        }

        public override void MakeFolderTree(){
            DownloadsFolder = Path.Combine(GameDocumentsFolder, "Downloads");
            CollectionsFolder = Path.Combine(GameDocumentsFolder, "Collections");
            ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
            MusicFolder = Path.Combine(GameDocumentsFolder, "Music");
            TerrainsFolder = Path.Combine(GameDocumentsFolder, "SC4Terrains");
            LotCatalogFolder = Path.Combine(GameDocumentsFolder, "LotCatalog");
            ConfigFolder = Path.Combine(GameDocumentsFolder, "Config");
            CamerasFolder = Path.Combine(GameDocumentsFolder, "Cameras");
            NeighborhoodsFolder = Path.Combine(GameDocumentsFolder, "Neighborhoods");
            MoviesFolder = Path.Combine(GameDocumentsFolder, "Movies");
            PackagedLotsFolder = Path.Combine(GameDocumentsFolder, "PackagedLots");
            
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "Accessory.cache"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "Groups.cache"));

            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\BuildModeThumbnails.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\CANHObjectsThumbnails.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\CASThumbnails.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\DesignModeThumbnails.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\ObjectThumbnails.package"));

            GameVersion = Utilities.GetGameVersion(GameChoice, GameDocumentsFolder);


        }

        public override void WriteXML(){
            
            if (File.Exists(this.XMLfile())){
                if (File.Exists(this.XMLfile().Replace(".xml", ".xml.bk"))){
                    File.Delete(this.XMLfile().Replace(".xml", ".xml.bk"));
                }
                File.Copy(this.XMLfile(), this.XMLfile().Replace(".xml", ".xml.bk"));
                File.Delete(this.XMLfile());
            }
            XmlSerializer InstanceSerializer = new XmlSerializer(this.GetType());
            using (var writer = new StreamWriter(this.XMLfile()))
            {
                InstanceSerializer.Serialize(writer, this);
            }

            /*StringBuilder sb = new();
            sb.AppendLine(string.Format("{0}={1}", "InstanceName", GetProperty("InstanceName")));
            sb.AppendLine(string.Format("{0}={1}", "GameChoice", GetProperty("GameChoice")));
            sb.AppendLine(string.Format("{0}={1}", "Identifier", GetProperty("Identifier")));
            sb.AppendLine(string.Format("{0}={1}", "GameInstallFolder", GetProperty("GameInstallFolder")));
            sb.AppendLine(string.Format("{0}={1}", "GameExe", GetProperty("GameExe")));
            sb.AppendLine(string.Format("{0}={1}", "GameDocumentsFolder", GetProperty("GameDocumentsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ExePath", GetProperty("ExePath")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceFolder", GetProperty("InstanceFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDownloadsFolder", GetProperty("InstanceDownloadsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDataFolder", GetProperty("InstanceDataFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstancePackagesFolder", GetProperty("InstancePackagesFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceProfilesFolder", GetProperty("InstanceProfilesFolder")));
            
            sb.AppendLine(string.Format("{0}={1}", "DownloadsFolder", GetProperty("DownloadsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ScreenshotsFolder", GetProperty("ScreenshotsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "MusicFolder", GetProperty("MusicFolder")));
            sb.AppendLine(string.Format("{0}={1}", "TerrainsFolder", GetProperty("TerrainsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "LotCatalogFolder", GetProperty("LotCatalogFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ConfigFolder", GetProperty("ConfigFolder")));
            sb.AppendLine(string.Format("{0}={1}", "CollectionsFolder", GetProperty("CollectionsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "CamerasFolder", GetProperty("CamerasFolder")));
            sb.AppendLine(string.Format("{0}={1}", "NeighborhoodsFolder", GetProperty("NeighborhoodsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "MoviesFolder", GetProperty("MoviesFolder")));
            sb.AppendLine(string.Format("{0}={1}", "PackagedLotsFolder", GetProperty("PackagedLotsFolder")));
            
            sb.AppendLine(string.Format("[CACHE FILES]"));

            foreach (string item in CacheFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[THUMBNAIL FILES]"));

            foreach (string item in ThumbnailsFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[EXECUTABLES]"));
            foreach (Executable executable in Executables){
                sb.AppendLine(string.Format("{0}={1}", "Exe", executable.Exe));
                sb.AppendLine(string.Format("{0}={1}", "Path", executable.Path));
                sb.AppendLine(string.Format("{0}={1}", "Name", executable.Name));
                string args = executable.Arguments;
                if (executable.Arguments == "") args = "None";
                sb.AppendLine(string.Format("{0}={1}", "Arguments", args));
                sb.AppendLine(string.Format("{0}={1}", "Selected", executable.Selected));
            }

            sb.AppendLine(string.Format("[CATEGORIES]"));
            foreach (Category category in Categories){
                sb.AppendLine(string.Format("{0}={1}", "Name", category.Name));
                sb.AppendLine(string.Format("{0}={1}", "Description", category.Description));
                sb.AppendLine(string.Format("{0}={1}", "Background", category.Background.ToHtml()));
                sb.AppendLine(string.Format("{0}={1}", "TextColor", category.TextColor.ToHtml()));
            }

            sb.AppendLine(string.Format("[PROFILES]"));
            foreach (string profile in profiles){
                sb.AppendLine(profile);
            }
            return sb;*/
        }
    }
    public class Sims3Instance : GameInstanceBase {
        public override Guid Identifier{ 
            get { return identifier; } set { identifier = value; }
        }
        public override Games GameChoice{ 
            get { return gamechoice; } set { gamechoice = value; }
        }
        public override string GameVersion{ 
            get { return gameversion; } set { gameversion = value; }
        }
        public override string GameInstallFolder{
            get { return gameinstallfolder; } set { gameinstallfolder = value; } 
        }
        public override string GameExe{
            get { return gameexe; } set { gameexe = value; } 
        }
        public override string GameDocumentsFolder{ 
            get { return gamedocumentsfolder; } set { gamedocumentsfolder = value; } 
        }
        public override string ExePath{ 
            get { return exepath; } set { exepath = value; } 
        }
        public override string InstanceFolder{ 
            get { return instancefolder; } set { instancefolder = value; } 
        }
        public override string InstanceName{ 
            get { return instancename; } set { instancename = value; } 
        }
        public override string InstanceDownloadsFolder{ 
            get { return instancedownloadsfolder; } set { instancedownloadsfolder = value; } 
        }
        public override string InstanceDataFolder{ 
            get { return instancedatafolder; } set { instancedatafolder = value; } 
        }
        public override string InstancePackagesFolder{ 
            get { return instancepackagesfolder; } set { instancepackagesfolder = value; } 
        }
        public override string InstanceProfilesFolder{ 
            get { return instanceprofilesfolder; } set { instanceprofilesfolder = value; } 
        }
        public override string InstanceSharedGameDataFolder{ 
            get { return instancesharedgamedatafolder; } set { instancesharedgamedatafolder = value; } 
        }
        public override List<Category> Categories{ 
            get { return categories; } set { categories = value; } 
        }
        public override List<Executable> Executables{ 
            get { return executables; } set { executables = value; } 
        }
        public override List<string> Profiles{ 
            get { return profiles; } set { profiles = value; } 
        }

        public override string ActiveProfile { 
            get {return activeprofile; } set { activeprofile = value; }
        }

        //data

        //media

        //saves

        //settings

        public string CollectionsFolder {get; set;} = "";
        public string CustomMusicFolder {get; set;} = "";
        public string InstalledWorldsFolder {get; set;} = "";
        public string DownloadsFolder {get; set;} = "";
        public string ExportsFolder {get; set;} = "";
        public string ModsFolder {get; set;} = "";
        public string VideosFolder {get; set;} = "";
        public string SavedOutfitsFolder {get; set;} = "";
        public string SavedSimsFolder {get; set;} = "";
        public string SavesFolder {get; set;} = "";
        public string ScreenshotsFolder {get; set;} = "";
        public string ThumbnailsFolder {get; set;} = "";

        public string InstanceInstalledWorldsFolder {get; set;} = "";
        public string InstanceExportsFolder {get; set;} = "";
        public string InstanceThumbnailsFolder {get; set;} = "";
        public string InstanceS3DownloadsFolder {get; set;}

        public List<string> CacheFiles {get; set;} = new List<string>();
        public List<string> ThumbnailsFiles {get; set;} = new List<string>();

        public override void TestInstance()
        {
            throw new NotImplementedException();
        }

        public override void CreateFromCurrent()
        {
            //reimplement

        }
        
        public override void MakeFolderTree(){
            DownloadsFolder = Path.Combine(GameDocumentsFolder, "Downloads");
            CollectionsFolder = Path.Combine(GameDocumentsFolder, "Collections");
            ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
            CustomMusicFolder = Path.Combine(GameDocumentsFolder, "CustomMusic");
            InstalledWorldsFolder = Path.Combine(GameDocumentsFolder, "InstalledWorlds");
            ExportsFolder = Path.Combine(GameDocumentsFolder, "Exports");
            ModsFolder = Path.Combine(GameDocumentsFolder, "Mods");
            VideosFolder = Path.Combine(GameDocumentsFolder, "Recorded Videos");
            SavedOutfitsFolder = Path.Combine(GameDocumentsFolder, "SavedOutfits");
            SavedSimsFolder = Path.Combine(GameDocumentsFolder, "SavedSims");
            SavesFolder = Path.Combine(GameDocumentsFolder, "Saves");
            ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
            ThumbnailsFolder = Path.Combine(GameDocumentsFolder, "Thumbnails");

            InstanceInstalledWorldsFolder = Path.Combine(InstanceSharedGameDataFolder, "InstalledWorldsFolder");
            InstanceExportsFolder = Path.Combine(InstanceSharedGameDataFolder, "ExportsFolder");
            InstanceThumbnailsFolder = Path.Combine(InstanceSharedGameDataFolder, "ThumbnailsFolder");
            InstanceS3DownloadsFolder = Path.Combine(InstanceSharedGameDataFolder, "Downloads");
            
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "CASPartCache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "compositorCache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "scriptCache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "simCompositorCache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "socialCache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"DCCache\missingdeps.idx"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"DCCache\dcc.ent"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"IGACache"));

            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "CASPartCache.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "compositorCache.package"));
            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "simCompositorCache.package"));

            GameVersion = Utilities.GetGameVersion(GameChoice, GameDocumentsFolder);
        }

        public void BuildInstance(bool createfromcurrent, Guid ident){
            new Thread (() => {
                this.Identifier = ident;
                SetCoreDirectories();
                MakeFolderTree();
                if (createfromcurrent){
                    BuildInstanceCoreFromCurrent();
                    CreateFromCurrent();
                } else {
                    BuildInstanceCore();
                }            
                WriteXML();
            }){IsBackground = true}.Start();

        }

                
        public override void WriteXML(){            

            if (File.Exists(this.XMLfile())){
                File.Delete(this.XMLfile());                
            }
            XmlSerializer InstanceSerializer = new XmlSerializer(this.GetType());
            using (var writer = new StreamWriter(this.XMLfile()))
            {
                InstanceSerializer.Serialize(writer, this);
            }
            /*StringBuilder sb = new();
            sb.AppendLine(string.Format("{0}={1}", "InstanceName", GetProperty("InstanceName")));
            sb.AppendLine(string.Format("{0}={1}", "GameChoice", GetProperty("GameChoice")));
            sb.AppendLine(string.Format("{0}={1}", "Identifier", GetProperty("Identifier")));
            sb.AppendLine(string.Format("{0}={1}", "GameInstallFolder", GetProperty("GameInstallFolder")));
            sb.AppendLine(string.Format("{0}={1}", "GameExe", GetProperty("GameExe")));
            sb.AppendLine(string.Format("{0}={1}", "GameDocumentsFolder", GetProperty("GameDocumentsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ExePath", GetProperty("ExePath")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceFolder", GetProperty("InstanceFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDownloadsFolder", GetProperty("InstanceDownloadsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDataFolder", GetProperty("InstanceDataFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstancePackagesFolder", GetProperty("InstancePackagesFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceProfilesFolder", GetProperty("InstanceProfilesFolder")));
            
            sb.AppendLine(string.Format("{0}={1}", "CollectionsFolder", GetProperty("CollectionsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "CustomMusicFolder", GetProperty("CustomMusicFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstalledWorldsFolder", GetProperty("InstalledWorldsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "DownloadsFolder", GetProperty("DownloadsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ExportsFolder", GetProperty("ExportsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "LibraryFolder", GetProperty("LibraryFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ModsFolder", GetProperty("ModsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "VideosFolder", GetProperty("VideosFolder")));
            sb.AppendLine(string.Format("{0}={1}", "SavedOutfitsFolder", GetProperty("SavedOutfitsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "SavedSimsFolder", GetProperty("SavedSimsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "SavesFolder", GetProperty("SavesFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ScreenshotsFolder", GetProperty("ScreenshotsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ThumbnailsFolder", GetProperty("ThumbnailsFolder")));

            sb.AppendLine(string.Format("[CACHE FILES]"));

            foreach (string item in CacheFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[THUMBNAIL FILES]"));

            foreach (string item in ThumbnailsFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[EXECUTABLES]"));
            foreach (Executable executable in Executables){
                sb.AppendLine(string.Format("{0}={1}", "Exe", executable.Exe));
                sb.AppendLine(string.Format("{0}={1}", "Path", executable.Path));
                sb.AppendLine(string.Format("{0}={1}", "Name", executable.Name));
                string args = executable.Arguments;
                if (executable.Arguments == "") args = "None";
                sb.AppendLine(string.Format("{0}={1}", "Arguments", args));
                sb.AppendLine(string.Format("{0}={1}", "Selected", executable.Selected));
            }

            sb.AppendLine(string.Format("[CATEGORIES]"));
            foreach (Category category in Categories){
                sb.AppendLine(string.Format("{0}={1}", "Name", category.Name));
                sb.AppendLine(string.Format("{0}={1}", "Description", category.Description));
                sb.AppendLine(string.Format("{0}={1}", "Background", category.Background.ToHtml()));
                sb.AppendLine(string.Format("{0}={1}", "TextColor", category.TextColor.ToHtml()));
            }

            sb.AppendLine(string.Format("[PROFILES]"));
            foreach (string profile in profiles){
                sb.AppendLine(profile);
            }
            return sb;*/
        }
    }
    public class Sims4Instance : GameInstanceBase {
        public override Guid Identifier{ 
            get { return identifier; } set { identifier = value; }
        }
        public override Games GameChoice{ 
            get { return gamechoice; } set { gamechoice = value; }
        }
        public override string GameVersion{ 
            get { return gameversion; } set { gameversion = value; }
        }
        public override string GameInstallFolder{
            get { return gameinstallfolder; } set { gameinstallfolder = value; } 
        }
        public override string GameExe{
            get { return gameexe; } set { gameexe = value; } 
        }
        public override string GameDocumentsFolder{ 
            get { return gamedocumentsfolder; } set { gamedocumentsfolder = value; } 
        }
        public override string ExePath{ 
            get { return exepath; } set { exepath = value; } 
        }
        public override string InstanceFolder{ 
            get { return instancefolder; } set { instancefolder = value; } 
        }
        public override string InstanceName{ 
            get { return instancename; } set { instancename = value; } 
        }
        public override string InstanceDownloadsFolder{ 
            get { return instancedownloadsfolder; } set { instancedownloadsfolder = value; } 
        }
        public override string InstanceDataFolder{ 
            get { return instancedatafolder; } set { instancedatafolder = value; } 
        }
        public override string InstancePackagesFolder{ 
            get { return instancepackagesfolder; } set { instancepackagesfolder = value; } 
        }
        public override string InstanceProfilesFolder{ 
            get { return instanceprofilesfolder; } set { instanceprofilesfolder = value; } 
        }
        public override string InstanceSharedGameDataFolder{ 
            get { return instancesharedgamedatafolder; } set { instancesharedgamedatafolder = value; } 
        }
        public override List<Category> Categories{ 
            get { return categories; } set { categories = value; } 
        }
        public override List<Executable> Executables{ 
            get { return executables; } set { executables = value; } 
        }
        public override List<string> Profiles{ 
            get { return profiles; } set { profiles = value; } 
        }

        public override string ActiveProfile { 
            get {return activeprofile; } set { activeprofile = value; }
        }
        
        public string ContentFolder {get; set;} = "";
        public string ConfigOverrideFolder {get; set;} = "";
        public string ModsFolder {get; set;} = "";
        public string VideosFolder {get; set;} = "";
        public string ScreenshotsFolder {get; set;} = "";
        public string TrayFolder {get; set;} = "";
        public string SavesFolder {get; set;} = "";

        public string InstanceContentFolder {get; set;} = "";
        public string InstanceScreenshotsFolder {get; set;} = "";
        public string InstanceVideosFolder {get; set;} = "";

        public List<string> CacheFiles {get; set; } = new List<string>();
        public List<string> ThumbnailsFiles {get; set;} = new List<string>();
        public override void MakeFolderTree(){
            ContentFolder = Path.Combine(GameDocumentsFolder, "Content");
            ConfigOverrideFolder = Path.Combine(GameDocumentsFolder, "ConfigOverride");
            ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
            ModsFolder = Path.Combine(GameDocumentsFolder, "Mods");
            VideosFolder = Path.Combine(GameDocumentsFolder, "Recorded Videos");
            TrayFolder = Path.Combine(GameDocumentsFolder, "Tray");
            SavesFolder = Path.Combine(GameDocumentsFolder, "Saves");
            InstanceContentFolder = Path.Combine(InstanceSharedGameDataFolder, "Content");
            InstanceScreenshotsFolder = Path.Combine(InstanceSharedGameDataFolder, "Screenshots");
            InstanceVideosFolder = Path.Combine(InstanceSharedGameDataFolder, "Videos");
            
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "avatarcache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "localthumbcache.package"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "cachestr"));
            CacheFiles.Add(Path.Combine(GameDocumentsFolder, "onlinethumbnailcache"));

            ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "localthumbcache.package"));

            GameVersion = Utilities.GetGameVersion(GameChoice, GameDocumentsFolder);
        }

        

        public void BuildInstance(bool createfromcurrent, Guid ident){
            new Thread (() => {
                this.Identifier = ident;
                SetCoreDirectories();
                MakeFolderTree();
                if (createfromcurrent){
                    BuildInstanceCoreFromCurrent();
                    CreateFromCurrent();
                } else {
                    BuildInstanceCore();
                }            
                WriteXML();
            }){IsBackground = true}.Start();

        }

        public override void TestInstance()
        {
            throw new NotImplementedException();
        }

        public override void CreateFromCurrent()
        {
            //reimplement
        }
        
        public override void WriteXML(){
            
            if (File.Exists(this.XMLfile())){
                File.Delete(this.XMLfile());                
            }
            XmlSerializer InstanceSerializer = new XmlSerializer(this.GetType());
            using (var writer = new StreamWriter(this.XMLfile()))
            {
                InstanceSerializer.Serialize(writer, this);
            }
            /*StringBuilder sb = new();
            sb.AppendLine(string.Format("{0}={1}", "InstanceName", GetProperty("InstanceName")));
            sb.AppendLine(string.Format("{0}={1}", "GameChoice", GetProperty("GameChoice")));
            sb.AppendLine(string.Format("{0}={1}", "Identifier", GetProperty("Identifier")));
            sb.AppendLine(string.Format("{0}={1}", "GameInstallFolder", GetProperty("GameInstallFolder")));
            sb.AppendLine(string.Format("{0}={1}", "GameExe", GetProperty("GameExe")));
            sb.AppendLine(string.Format("{0}={1}", "GameDocumentsFolder", GetProperty("GameDocumentsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ExePath", GetProperty("ExePath")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceFolder", GetProperty("InstanceFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDownloadsFolder", GetProperty("InstanceDownloadsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceDataFolder", GetProperty("InstanceDataFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstancePackagesFolder", GetProperty("InstancePackagesFolder")));
            sb.AppendLine(string.Format("{0}={1}", "InstanceProfilesFolder", GetProperty("InstanceProfilesFolder")));
            
            sb.AppendLine(string.Format("{0}={1}", "ContentFolder", GetProperty("ContentFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ModsFolder", GetProperty("ModsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "VideosFolder", GetProperty("VideosFolder")));
            sb.AppendLine(string.Format("{0}={1}", "ScreenshotsFolder", GetProperty("ScreenshotsFolder")));
            sb.AppendLine(string.Format("{0}={1}", "TrayFolder", GetProperty("TrayFolder")));
            sb.AppendLine(string.Format("{0}={1}", "SavesFolder", GetProperty("SavesFolder")));

            sb.AppendLine(string.Format("[CACHE FILES]"));

            foreach (string item in CacheFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[THUMBNAIL FILES]"));

            foreach (string item in ThumbnailsFiles){
                sb.AppendLine(string.Format("{0}", item));
            }

            sb.AppendLine(string.Format("[EXECUTABLES]"));
            foreach (Executable executable in Executables){
                sb.AppendLine(string.Format("{0}={1}", "Exe", executable.Exe));
                sb.AppendLine(string.Format("{0}={1}", "Path", executable.Path));
                sb.AppendLine(string.Format("{0}={1}", "Name", executable.Name));
                string args = executable.Arguments;
                if (executable.Arguments == "") args = "None";
                sb.AppendLine(string.Format("{0}={1}", "Arguments", args));
                sb.AppendLine(string.Format("{0}={1}", "Selected", executable.Selected));
            }

            sb.AppendLine(string.Format("[CATEGORIES]"));
            foreach (Category category in Categories){
                sb.AppendLine(string.Format("{0}={1}", "Name", category.Name));
                sb.AppendLine(string.Format("{0}={1}", "Description", category.Description));
                sb.AppendLine(string.Format("{0}={1}", "Background", category.Background.ToHtml()));
                sb.AppendLine(string.Format("{0}={1}", "TextColor", category.TextColor.ToHtml()));
            }

            sb.AppendLine(string.Format("[PROFILES]"));
            foreach (string profile in profiles){
                sb.AppendLine(profile);
            }
            return sb;*/
        }        
    }

    public class ExeInfo {
        public string folder {get; set;}
        public string name {get; set;}
    }

    public class ProfileInfo {
        public string ProfileName {get; set;} = "";
        public string InfoLocation {get; set;} = "";
        public string ProfileFolder {get; set;} = "";
        public string ScreenshotsFolder {get; set;} = "";
        public string VideosFolder {get; set;} = "";
        public string TrayFolder {get; set;} = "";
        public bool LocalSaves {get; set;} = false;
        public bool LocalData {get; set;} = false;
        public bool LocalMedia {get; set;} = false;
        public bool LocalSettings {get; set;} = false;
        public string LocalSavesFolder {get; set;} = "";
        public string LocalDataFolder {get; set;} = "";
        public string LocalMediaFolder {get; set;} = "";
        public string LocalSettingsFolder {get; set;} = "";
        private List<ProfilePackage> _packages {get; set;} = new();
        public List<ProfilePackage> Packages {
            get { return _packages; }
            set { _packages = value; 
            SaveProfile();}
        }

        public List<LocalFolders> LocalDataFolders {get; set;} = new();
        public List<LocalFolders> LocalMediaFolders {get; set;} = new();
        public List<LocalFolders> LocalSettingsFolders {get; set;} = new();
        public List<LocalFolders> LocalSaveFolders {get; set;} = new();
        public List<LocalFolders> LocalDataFiles {get; set;} = new();
        public List<LocalFolders> LocalMediaFiles {get; set;} = new();
        public List<LocalFolders> LocalSettingsFiles {get; set;} = new();
        public List<LocalFolders> LocalSaveFiles {get; set;} = new();

        public ProfileInfo(){
            Packages = new();
        }        

        public void MakeInfoLocation(){
            if (ProfileName != ""){
                string proffolder = GlobalVariables.thisinstance.InstanceProfilesFolder;
                string thisproffolder = Path.Combine(proffolder, ProfileName);
                ProfileFolder = thisproffolder;
                string profileinfo = Path.Combine(thisproffolder, "ProfileInformation.info");
                InfoLocation = profileinfo;
            }
        }

        public void SaveProfile(){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving profile {0}.", this.ProfileName));
            if (InfoLocation != ""){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving profile to: {0}.", this.InfoLocation));
                if (File.Exists(InfoLocation)){
                    File.Delete(InfoLocation);                
                } else if (!Directory.Exists(new FileInfo(InfoLocation).DirectoryName)){
                    Directory.CreateDirectory(new FileInfo(InfoLocation).DirectoryName);
                }
                XmlSerializer packageSerializer = new XmlSerializer(this.GetType());
                try { 
                    using (var writer = new StreamWriter(InfoLocation))
                    {
                        
                            packageSerializer.Serialize(writer, this); 
                        
                    }
                } catch (Exception e) {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Writing info file for profile {0} failed: {1}\n{2}\n{3}\n.", this.ProfileName, e.Message, e.StackTrace, e.Source));
                }
            } else {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Profile {0} has no infolocation.", this.ProfileName));
            }
        }
    }

    public class ProfilePackage {
        public string PackageFile {get; set;}
        public int LoadOrder {get; set;} = -1;
        public Guid Identifier {get; set;}
    }

    public class LocalFolders {
        public string LocalFolder {get; set;} = "";
        public string OriginalFolder {get; set;} = "";
    }
}