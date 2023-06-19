using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SSAGlobals;
using SimsCCManager.Packages.Containers;
using SimsCCManager.App.PreviewImage;
using System.Data.SQLite;
using System.Data;
using System.Threading;
using SQLiteNetExtensions.Extensions;

namespace SimsCCManager.SortingUIResults {
    /// <summary>
    /// Results window; a datagrid with all of the package files. (Later, will include ts3packs and ts2packs as well.)
    /// </summary>

    public partial class ResultsWindow : Window {
        LoggingGlobals log = new LoggingGlobals();
        public static int gameNum = 0;
        private bool showallfiles = false;
        private DataTable packagesDataTable = new DataTable();
        private DataTable allFilesDataTable = new DataTable();
        //public static ObservableCollection<SimsPackage> resultspackageslist = new ObservableCollection<SimsPackage>();
        CancellationTokenSource cts = new CancellationTokenSource();
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        public static System.Windows.Controls.ListView resultsView = new System.Windows.Controls.ListView();
        ContextMenu contextmenu = new ContextMenu();
        public static System.Windows.Controls.TextBox searchbox = new System.Windows.Controls.TextBox();
        public static System.Windows.Controls.TextBox gotobox = new System.Windows.Controls.TextBox();
        public static System.Windows.Controls.ComboBox comboBox = new System.Windows.Controls.ComboBox();
        public static System.Windows.Controls.ListView tagslist = new System.Windows.Controls.ListView();
        public static Grid tagsgrid = new Grid();
        public static Dictionary<string, string> comboboxoptions = new Dictionary<string, string>();
        public static int game = 0;
        public static int numPackages = 0;
        public static int pageNum = 0;
        public static int pages = 0;
        public static int itemsPerPage = 100;
        public static int actualpages = 0;
        public static int currentPage = 0;
        public static bool PackagesView = true;
        public static int Filter = 0;
        public static string Sorting = "PackageName";
        public static System.Windows.Controls.Label pageNumberLabel = new();
        public static System.Windows.Controls.Label pageTotalLabel = new();
        
        public ResultsWindow(CancellationTokenSource cts) 
        {
            this.cts = cts;
            log.MakeLog("Initializing results window.", true);
            InitializeComponent(); 
            log.MakeLog("Running results grid method.", true); 
            Loaded += ResultsWindow_Loaded;
            resultsView = ResultsView;
            tagslist = TagsListBox;
            tagsgrid = this.TagsList;
            contextmenu = ResultsView.ContextMenu;            
            searchbox = SearchBox;
            gotobox = GoToPageBox;
            comboBox = this.ComboBoxSearch;
            pageNumberLabel = PageNumber;
            pageTotalLabel = PageTotal;
            currentPage = pageNum + 1;            
            numPackages = GlobalVariables.DatabaseConnection.ExecuteScalar<int>("select count(PackageName) from Packages");
            pages = (int)Math.Ceiling((double)numPackages / (double)itemsPerPage);
            actualpages = pages - 1;
            Dispatcher.Invoke(new Action(() => pageNumberLabel.Content = currentPage.ToString()));
            Dispatcher.Invoke(new Action(() => pageTotalLabel.Content = actualpages.ToString()));
            List<string> comboboxsearch = new List<string>
            {
                "Package Name",
                "Title",
                "Description",
                "InstanceIDs",
                "Tags",
                "Type",
                "Category",
                "Age",
                "Gender",
                "Function",
                "Function Subcategory",
                "Required EPs",
                "Overridden Instances",
                "Overridden Items",
                "Conflicts",
                "Matching Recolors",
                "Matching Meshes"
            };
            this.ComboBoxSearch.ItemsSource = comboboxsearch;
            
            DataContext = new PackagesViewModel();
            
        }     

        private void ResultsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);

        }

        private void Kofi_Click(object sender, EventArgs e){
            if (System.Windows.Forms.MessageBox.Show
            ("Open Kofi?", "Opening External URL",
            System.Windows.Forms.MessageBoxButtons.YesNo, 
            System.Windows.Forms.MessageBoxIcon.Question)
            ==System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("http://ko-fi.com/sinfulsimming") { UseShellExecute = true });
                }

            else
                {
                //React as needed.
                }
        }
        private void Git_Click(object sender, EventArgs e){
            if (System.Windows.Forms.MessageBox.Show
            ("Open the Sims CC Manager Github Repo?", "Opening External URL",
            System.Windows.Forms.MessageBoxButtons.YesNo, 
            System.Windows.Forms.MessageBoxIcon.Question)
            ==System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://github.com/sixstepsaway/Sims-CC-Manager") { UseShellExecute = true });
                }

            else
                {
                //React as needed.
                }
        }
        private void Twitter_Click(object sender, EventArgs e){
            if (System.Windows.Forms.MessageBox.Show
            ("Open SinfulSimming's Twitter?", "Opening External URL",
            System.Windows.Forms.MessageBoxButtons.YesNo, 
            System.Windows.Forms.MessageBoxIcon.Question)
            ==System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://twitter.com/sinfulsimming") { UseShellExecute = true });
                }

            else
                {
                //React as needed.
                }
        }
        private void Discord_Click(object sender, EventArgs e){
            if (System.Windows.Forms.MessageBox.Show
            ("Open Discord?", "Opening External URL",
            System.Windows.Forms.MessageBoxButtons.YesNo, 
            System.Windows.Forms.MessageBoxIcon.Question)
            ==System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://discord.gg/M6vnf842Fp") { UseShellExecute = true });  
                }

            else
                {
                //React as needed.
                }                         
        }

        private void menu_Click(object sender, EventArgs e)
        {
            log.MakeLog("Closing application.", false);
            cts.Cancel();
            Thread.Sleep(2000);
            GlobalVariables.InstancesCacheConnection.Commit();
            GlobalVariables.InstancesCacheConnection.Close();
            GlobalVariables.S4FunctionTypesConnection.Commit();
            GlobalVariables.S4FunctionTypesConnection.Close();
            GlobalVariables.S4OverridesConnection.Commit();
            GlobalVariables.S4OverridesConnection.Close();
            GlobalVariables.S4SpecificOverridesConnection.Commit();
            GlobalVariables.S4SpecificOverridesConnection.Close();
            GlobalVariables.DatabaseConnection.Commit();
            GlobalVariables.DatabaseConnection.Close();
            System.Windows.Application.Current.Shutdown();
        }

        private void loadErrorFix_Click(object sender, EventArgs e){
            
            bool gameChecked = checkGame();
            if (gameChecked == true){
                if (System.Windows.Forms.MessageBox.Show
                ("There is no guarantee this will fix all load errors, but it should. This will move all incorrect (wrong for your game version) or broken packages to a separate folder. Nothing will be deleted. Should we go ahead?", "Fix Load Errors",
                System.Windows.Forms.MessageBoxButtons.YesNo, 
                System.Windows.Forms.MessageBoxIcon.Question)
                ==System.Windows.Forms.DialogResult.Yes) {
                    //fix load errors o.o
                }
            } else {
                //do nothing, a message will have done the job
            }            
        }
        
        private void CloseTagsList_Click(object sender, EventArgs e){            
            tagsgrid.Visibility = Visibility.Hidden;            
        }

        private bool checkGame() {
            bool gamepicked = false;

            log.MakeLog("Checking which game is ticked.", true);
            if (Filter == 2) {
                log.MakeLog("Sims 2 picked.", true);
                gameNum = 2;
                gamepicked = true;
                return gamepicked;
            } else if (Filter == 3) {
                log.MakeLog("Sims 3 picked.", true);
                gameNum = 3;
                gamepicked = true;
                return gamepicked;
            } else if (Filter == 4) {
                log.MakeLog("Sims 4 picked.", true);
                gameNum = 4;
                gamepicked = true;
                return gamepicked;
            } else {
                System.Windows.Forms.MessageBox.Show("Please select a game.");
                log.MakeLog("No game picked.", true);
                gamepicked = false;
                return gamepicked;
            }
        }        
    }    

    public class PackagesViewModel : INotifyPropertyChanged{
        private ICollectionView _packagesView;
        private ICollectionView _tagsView;
        public event PropertyChangedEventHandler PropertyChanged;
        private PackagesViewModel _selectedFile;
        private List<SimsPackage> allPackages = new();
        private List<List<SimsPackage>> pagesOfPackages = new();

        public ICollectionView Packages
        {
            get {return _packagesView;}
        }
        public ICollectionView Tags
        {
            get {return _tagsView;}
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public object Resources { get; private set; }
        List<SimsPackage> thisPage = new List<SimsPackage>();
        public static ObservableCollection<TagsList> tags = new ObservableCollection<TagsList>();
        List<SimsPackage> previousPage = new();
        List<SimsPackage> nextPage = new();
        List<SimsPackage> firstPage = new();
        List<SimsPackage> lastPage = new();

        public PackagesViewModel(){
            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();            
            _tagsView = CollectionViewSource.GetDefaultView(tags);
            _packagesView = CollectionViewSource.GetDefaultView(thisPage);
        }

        public PackagesViewModel SelectedFileInfo  
        {  
            get { return _selectedFile; }  
            set 
            {  
                if (value != _selectedFile)  
                {  
                    _selectedFile = value;  
                    this.OnPropertyChanged("SelectedFileInfo");  
                }  
            }  
        }

        private void RefreshResults(){
            thisPage = new (GlobalVariables.DatabaseConnection.GetAllWithChildren<SimsPackage>());
            ResultsWindow.resultsView.ItemsSource = _packagesView;
            _packagesView = CollectionViewSource.GetDefaultView(thisPage);
        }


        
        #region Sorting and Pages

        public bool Ascending = true;
        public string SearchTerm = "";
        public bool Searching = false;
        public string SearchCriteria = "";

        public ICommand SubmitSearch
        {
            get { return new DelegateCommand(this.SearchResults); }
        }

        private void SearchResults(){
            if (String.IsNullOrEmpty(ResultsWindow.comboBox.Text) || String.IsNullOrWhiteSpace(ResultsWindow.comboBox.Text)){
                SearchCriteria = "All";
            } else {
                SearchCriteria = ResultsWindow.comboBox.Text;
            }            
            if (String.IsNullOrWhiteSpace(ResultsWindow.searchbox.Text) || String.IsNullOrEmpty(ResultsWindow.searchbox.Text)){
                Searching = false;
                SearchTerm = "";
                ResultsWindow.pageNum = 0;
                ChangePageReset();
            } else {
                Searching = true;
                SearchTerm = ResultsWindow.searchbox.Text;
                ResultsWindow.pageNum = 0;
                ChangePageSearch();
            }
        }

        public ICommand HeaderPackageName  
        {  
            get { return new DelegateCommand(this.Sort_HeaderPackageName); }  
        }  
        private void Sort_HeaderPackageName()  
        { 
            SortList("PackageName");
        }
        public ICommand HeaderTitle  
        {  
            get { return new DelegateCommand(this.Sort_HeaderTitle); }  
        }  
        private void Sort_HeaderTitle()  
        { 
            SortList("Title");
        }
        public ICommand HeaderType  
        {  
            get { return new DelegateCommand(this.Sort_HeaderType); }  
        }  
        private void Sort_HeaderType()  
        { 
            SortList("Type");
        }
        public ICommand HeaderDescription  
        {  
            get { return new DelegateCommand(this.Sort_HeaderDescription); }  
        }  
        private void Sort_HeaderDescription()  
        { 
            SortList("Description");
        }
        public ICommand HeaderFileSize  
        {  
            get { return new DelegateCommand(this.Sort_HeaderFileSize); }  
        }  
        private void Sort_HeaderFileSize()  
        { 
            SortList("FileSize");
        }
        public ICommand HeaderGameString  
        {  
            get { return new DelegateCommand(this.Sort_HeaderGameString); }  
        }  
        private void Sort_HeaderGameString()  
        { 
            SortList("GameString");
        }
        public ICommand HeaderFunction  
        {  
            get { return new DelegateCommand(this.Sort_HeaderFunction); }  
        }  
        private void Sort_HeaderFunction()  
        { 
            SortList("Function");
        }
        public ICommand HeaderFunctionSub  
        {  
            get { return new DelegateCommand(this.Sort_HeaderFunctionSub); }  
        }  
        private void Sort_HeaderFunctionSub()  
        { 
            SortList("FunctionSub");
        }
        public ICommand HeaderLocation  
        {  
            get { return new DelegateCommand(this.Sort_HeaderLocation); }  
        }  
        private void Sort_HeaderLocation()  
        { 
            SortList("Location");
        }        
        private void SortList(string column){
            if (column == "PackageName"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }                
            } else if (column == "Title"){                
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "Type"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "Description"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "FileSize"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "GameString"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "Function"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {                        
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "FunctionSub"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            } else if (column == "Location"){
                if (ResultsWindow.Sorting == column){
                    if (Ascending == false){
                        Ascending = true;
                        ChangePage();
                    } else {
                        Ascending = false;
                        ChangePage();
                    }
                } else {
                    ResultsWindow.Sorting = column;
                    Ascending = true;
                    ChangePage();
                }   
            }
        }

        public ICommand GoTo  
        {  
            get { return new DelegateCommand(this.GoToPage); }  
        }  
        private void GoToPage()  
        { 
            int gotonum = int.Parse(ResultsWindow.gotobox.Text);
            if (gotonum <= ResultsWindow.pages && gotonum != 0){
                ResultsWindow.pageNum = int.Parse(ResultsWindow.gotobox.Text) - 1;
                ResultsWindow.pageNum = gotonum; 
                ChangePage();
            } else if (gotonum == ResultsWindow.pages + 1) {    
                ResultsWindow.pageNum = gotonum - 1;           
                ChangePage();
            } else if (gotonum == 1) {   
                ResultsWindow.pageNum = gotonum - 1;             
                ChangePage();
            } else {
                System.Windows.Forms.MessageBox.Show(string.Format("Enter a number between 1 and {0}.", ResultsWindow.pages));
            }
            
        }

        public ICommand PageFirst  
        {  
            get { return new DelegateCommand(this.SwaptoFirstPage); }  
        }  
        private void SwaptoFirstPage()  
        { 
            ResultsWindow.pageNum = 0;
            ChangePage();
        }
        
        public ICommand PageLast  
        {  
            get { return new DelegateCommand(this.SwaptoLastPage); }  
        }  
        private void SwaptoLastPage()  
        { 
            ResultsWindow.pageNum = ResultsWindow.actualpages;
            ChangePage();
        }
        
        public ICommand PageForward  
        {  
            get { return new DelegateCommand(this.PageUp); }  
        }  
        private void PageUp()  
        { 
            if (ResultsWindow.pageNum == ResultsWindow.pages){
                ResultsWindow.pageNum = 0;
                ChangePage();
            } else {
                ResultsWindow.pageNum++;
                ChangePage();
            }            
        }

        public ICommand PageBack  
        {  
            get { return new DelegateCommand(this.PageBackward); }  
        }  
        private void PageBackward()  
        { 
            if (ResultsWindow.pageNum == 0){
                ResultsWindow.pageNum = ResultsWindow.pages;
                ChangePage();
            } else {
                ResultsWindow.pageNum--;
                ChangePage();
            } 
        }

        private void ChangePage(){            
            int skipnum = ResultsWindow.itemsPerPage * ResultsWindow.pageNum;
            int startingnum = skipnum + 1;
            int endingnum = skipnum + 1 + ResultsWindow.itemsPerPage;
            
            if (ResultsWindow.Filter == 0){
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                }
            } else {
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                }
            }
            
            int maxpage = ResultsWindow.actualpages + 1;
            int currentpage = ResultsWindow.pageNum + 1;
            ResultsWindow.pageNumberLabel.Content = currentpage.ToString();
            ResultsWindow.pageTotalLabel.Content = maxpage.ToString();
            _packagesView = CollectionViewSource.GetDefaultView(thisPage); 
            ResultsWindow.resultsView.ItemsSource = _packagesView;
        }

        private void ChangePageReset(){

            ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.ExecuteScalar<int>("select count(PackageName) from Packages");
            ResultsWindow.pages = (int)Math.Ceiling((double)ResultsWindow.numPackages / (double)ResultsWindow.itemsPerPage);
            ResultsWindow.actualpages = ResultsWindow.pages - 1;
            
            if (ResultsWindow.Filter == 0){
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                }
            } else {
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    } else {
                        thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                    }
                }
            }
            
            int maxpage = ResultsWindow.actualpages + 1;
            int currentpage = ResultsWindow.pageNum + 1;
            ResultsWindow.pageNumberLabel.Content = currentpage.ToString();
            ResultsWindow.pageTotalLabel.Content = maxpage.ToString();
            _packagesView = CollectionViewSource.GetDefaultView(thisPage); 
            ResultsWindow.resultsView.ItemsSource = _packagesView;
        }

        private void ChangePageSearch(){
            
            if (SearchCriteria == "Package Name"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Title"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Description"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "InstanceIDs"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Tags"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).Count();
            } else if (SearchCriteria == "Type"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Category"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Age"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Gender"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Function"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Function Subcategory"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Required EPs"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Overridden Instances"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Overridden Items"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Conflicts"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Matching Recolors"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).Count();
            } else if (SearchCriteria == "Matching Meshes"){
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).Count();
            } else {
                ResultsWindow.numPackages = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(
                    o => o.PackageName.Contains(SearchTerm) 
                    || 
                    (o.Title != null && o.Title.Contains(SearchTerm))
                    || 
                    (o.Description != null && o.Description.Contains(SearchTerm))
                    || 
                    (o.InstancesBlobbed != null && o.InstancesBlobbed.Contains(SearchTerm))
                    || 
                    (o.CatalogTags.Any() && o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)))
                    || 
                    (o.Type != null && o.Type.Contains(SearchTerm))
                    || 
                    (o.Category != null && o.Category.Contains(SearchTerm))
                    || 
                    (o.Age != null && o.Age.Contains(SearchTerm))
                    || 
                    (o.Gender != null && o.Gender.Contains(SearchTerm))
                    || 
                    (o.Function != null && o.Function.Contains(SearchTerm))
                    || 
                    (o.FunctionSubcategory != null && o.FunctionSubcategory.Contains(SearchTerm))
                    || 
                    (o.RequiredEPs.Any() && o.RequiredEPs.Contains(SearchTerm))
                    || 
                    (o.OverriddenInstances.Any() && o.OverriddenInstances.Contains(SearchTerm))
                    || 
                    (o.OverriddenItems.Any() && o.OverriddenItems.Contains(SearchTerm))
                    || 
                    (o.MatchingConflicts.Any() && o.MatchingConflicts.Contains(SearchTerm))
                    || 
                    (o.MatchingRecolors.Any() && o.MatchingRecolors.Contains(SearchTerm))
                    || 
                    (o.MatchingMesh != null && o.MatchingMesh.Contains(SearchTerm)) 
                ).Count();
            }
            
            ResultsWindow.pages = (int)Math.Ceiling((double)ResultsWindow.numPackages / (double)ResultsWindow.itemsPerPage);
            ResultsWindow.actualpages = ResultsWindow.pages - 1;
            
            if (ResultsWindow.Filter == 0){
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                }
            } else {
                if(ResultsWindow.Sorting == "PackageName"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.PackageName).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }                
                } else if(ResultsWindow.Sorting == "Title"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Title).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Type"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Type).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Description"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Description).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "FileSize"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FileSize).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "GameString"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.GameString).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Function"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Function).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "FunctionSub"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.FunctionSubcategory).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                } else if(ResultsWindow.Sorting == "Location"){
                    if (Ascending == true){
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderBy(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    } else {
                        if (SearchCriteria == "Package Name"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Title"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Title.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Description"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Description.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "InstanceIDs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.InstancesBlobbed.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Tags"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm))).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Type"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Type.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Category"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Category.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Age"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Age.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Gender"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Gender.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.Function.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Function Subcategory"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.FunctionSubcategory.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Required EPs"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.RequiredEPs.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Instances"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenInstances.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Overridden Items"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.OverriddenItems.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Conflicts"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingConflicts.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Recolors"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingRecolors.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else if (SearchCriteria == "Matching Meshes"){
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        } else {
                            thisPage = GlobalVariables.DatabaseConnection.Table<SimsPackage>().Where(p=> p.Game == ResultsWindow.Filter).Where(o => o.PackageName.Contains(SearchTerm) || o.Title.Contains(SearchTerm) || o.Description.Contains(SearchTerm) || o.InstancesBlobbed.Contains(SearchTerm) || o.CatalogTags.Any(t => t.TypeID.Contains(SearchTerm)) || o.Type.Contains(SearchTerm) || o.Category.Contains(SearchTerm) || o.Age.Contains(SearchTerm) || o.Gender.Contains(SearchTerm) || o.Function.Contains(SearchTerm) || o.FunctionSubcategory.Contains(SearchTerm) || o.RequiredEPs.Contains(SearchTerm) || o.OverriddenInstances.Contains(SearchTerm) || o.OverriddenItems.Contains(SearchTerm) || o.MatchingConflicts.Contains(SearchTerm) || o.MatchingRecolors.Contains(SearchTerm) || o.MatchingMesh.Contains(SearchTerm)).OrderByDescending(o=> o.Location).Skip(ResultsWindow.itemsPerPage * ResultsWindow.pageNum).Take(ResultsWindow.itemsPerPage).ToList();
                        }
                    }
                }                
            }
            
            int maxpage = ResultsWindow.actualpages + 1;
            int currentpage = ResultsWindow.pageNum + 1;
            ResultsWindow.pageNumberLabel.Content = currentpage.ToString();
            ResultsWindow.pageTotalLabel.Content = maxpage.ToString();
            _packagesView = CollectionViewSource.GetDefaultView(thisPage); 
            ResultsWindow.resultsView.ItemsSource = _packagesView;
        }

        public ICommand GameSims2  
        {  
            get { return new DelegateCommand(this.GameSims2Picked); }  
        } 
        
 
        private void GameSims2Picked()  
        {   
            ResultsWindow.Filter = 2;
            ResultsWindow.pageNum = 0;
            ChangePage();
        }

        public ICommand GameSims3  
        {  
            get { return new DelegateCommand(this.GameSims3Picked); }  
        } 
        
 
        private void GameSims3Picked()  
        {   
            ResultsWindow.Filter = 3;
            ResultsWindow.pageNum = 0;
            ChangePage();
        }

        public ICommand GameSims4  
        {  
            get { return new DelegateCommand(this.GameSims4Picked); }  
        } 
        
 
        private void GameSims4Picked()  
        {   
            ResultsWindow.Filter = 4;
            ResultsWindow.pageNum = 0;
            ChangePage();
        }

        public ICommand GameSimsNoGame  
        {  
            get { return new DelegateCommand(this.GameNonePicked); }  
        } 
        
 
        private void GameNonePicked()  
        {   
            ResultsWindow.Filter = 0;
            ResultsWindow.pageNum = 0;
            ChangePage();
        }

        #endregion



        #region Tag Viewer


        private void RefreshTagViewer(){
            _tagsView = CollectionViewSource.GetDefaultView(tags);
            ResultsWindow.tagslist.ItemsSource = _tagsView;
        }

        #endregion
        











        public ICommand RenameFile  
        {  
            get { return new DelegateCommand(this.OnRename); }  
        }  
        private void OnRename()  
        {  
            string stuff = "";
            string movefolder = "";
            string sourcefile = ((SimsPackage)_packagesView.CurrentItem).Location;
            FileInfo sf = new FileInfo(((SimsPackage)_packagesView.CurrentItem).Location);
            string filename = ((SimsPackage)_packagesView.CurrentItem).PackageName;
            string destination = "";
            int selection = ResultsWindow.resultsView.SelectedItems.Count;
            if (selection == 1){                
                using(var MoveFolder = new FolderBrowserDialog())
                {
                    string newname = ((SimsPackage)_packagesView.CurrentItem).Title;
                    destination = System.IO.Path.Combine(sf.DirectoryName, newname);
                    if (File.Exists(sourcefile)){
                        SimsPackage item = (SimsPackage)_packagesView.CurrentItem;
                        item.Location = destination;
                        File.Move(sourcefile, destination);                        
                        GlobalVariables.DatabaseConnection.UpdateWithChildren(item);
                        RefreshResults();
                    } else {
                        System.Windows.Forms.MessageBox.Show(string.Format("File {0} not found at source. Did it get deleted?", sourcefile));    
                    }
                    System.Windows.Forms.MessageBox.Show(string.Format("Renamed {0}!", ((SimsPackage)_packagesView.CurrentItem).PackageName));
                    
                }            
            } else if (selection != 1 && selection != 0){
                foreach (var item in ResultsWindow.resultsView.SelectedItems){
                    SimsPackage thing = (SimsPackage)item;
                    if (String.IsNullOrEmpty(stuff)){
                        stuff = string.Format("{0}", thing.PackageName);
                    } else {
                        stuff += string.Format("\n {0}", thing.PackageName);
                    }                
                }
                using(var MoveFolder = new FolderBrowserDialog())
                {
                    foreach (var thing in ResultsWindow.resultsView.SelectedItems){
                        sf = new FileInfo(((SimsPackage)_packagesView.CurrentItem).Location);
                        SimsPackage item = ((SimsPackage)thing);
                        string newname = ((SimsPackage)_packagesView.CurrentItem).Title;
                        destination = System.IO.Path.Combine(sf.DirectoryName, newname);
                        filename = item.PackageName;
                        destination = System.IO.Path.Combine(destination, newname);
                        sourcefile = item.Location;
                        if (File.Exists(sourcefile)){                            
                            item.Location = destination;
                            File.Move(sourcefile, destination);                        
                            GlobalVariables.DatabaseConnection.UpdateWithChildren(item);
                            RefreshResults();
                        } else {
                            System.Windows.Forms.MessageBox.Show(string.Format("File not found: {0}", item.PackageName));    
                        }
                    }
                    System.Windows.Forms.MessageBox.Show(string.Format("Renamed {0} packages", ResultsWindow.resultsView.SelectedItems.Count));
                    
                }
                
            }
        }  









        public ICommand ShowTags  
        {  
            get { return new DelegateCommand(this.OnShowTags); }  
        } 
        
 
        private void OnShowTags()  
        {   
            SimsPackage item = (SimsPackage)_packagesView.CurrentItem;
            Console.WriteLine("Showing tags for " + item.PackageName);            
            foreach (TagsList tag in item.CatalogTags){
                Console.WriteLine(tag.TypeID + " | " + tag.Description);
                tags.Add(tag);
            }
            RefreshTagViewer();
            ResultsWindow.tagsgrid.Visibility = Visibility.Visible;
        }
 
        public ICommand DeleteFile  
        {  
            get { return new DelegateCommand(this.OnDelete); }  
        } 
        
 
        private void OnDelete()  
        {   
            string sourcefile = "";
            string package = "";
            string stuff = "";
            int selection = ResultsWindow.resultsView.SelectedItems.Count;
            if (selection == 1){
                SimsPackage item = (SimsPackage)_packagesView.CurrentItem;
                sourcefile = item.Location;
                package = item.PackageName;
                if (System.Windows.Forms.MessageBox.Show
                (string.Format("Are you sure you want to delete: {0}", ((SimsPackage)_packagesView.CurrentItem).PackageName), "Confirm Delete",
                System.Windows.Forms.MessageBoxButtons.YesNo, 
                System.Windows.Forms.MessageBoxIcon.Question)
                ==System.Windows.Forms.DialogResult.Yes){
                    if (File.Exists(sourcefile)){                        
                        File.Delete(sourcefile);                        
                        GlobalVariables.DatabaseConnection.Delete(item);                        
                        thisPage = new (GlobalVariables.DatabaseConnection.GetAllWithChildren<SimsPackage>());
                        RefreshResults();
                        System.Windows.Forms.MessageBox.Show(string.Format("Deleted {0}!", package));
                    } else {
                        System.Windows.Forms.MessageBox.Show(string.Format("File {0} not found at source. Did it get deleted?", sourcefile));    
                    }                       
                } else {
                    //do not delete
                }
            } else if (selection != 1 && selection != 0){
                foreach (var item in ResultsWindow.resultsView.SelectedItems){
                    SimsPackage thing = (SimsPackage)item;
                    if (String.IsNullOrEmpty(stuff)){
                        stuff = string.Format("{0}", thing.PackageName);
                    } else {
                        stuff += string.Format("\n {0}", thing.PackageName);
                    }                
                }
                if (System.Windows.Forms.MessageBox.Show
                (string.Format("Are you sure you want to delete: {0}", stuff), "Confirm Delete",
                System.Windows.Forms.MessageBoxButtons.YesNo, 
                System.Windows.Forms.MessageBoxIcon.Question)
                ==System.Windows.Forms.DialogResult.Yes){
                    
                    if (System.Windows.Forms.MessageBox.Show
                    (string.Format("Are you sure you want to delete: {0}", ((SimsPackage)_packagesView.CurrentItem).PackageName), "Confirm Delete",
                    System.Windows.Forms.MessageBoxButtons.YesNo, 
                    System.Windows.Forms.MessageBoxIcon.Question)
                    ==System.Windows.Forms.DialogResult.Yes){
                        foreach (var selecteditem in ResultsWindow.resultsView.SelectedItems){
                            SimsPackage item = (SimsPackage)selecteditem;
                            sourcefile = item.Location;
                            package = item.PackageName;
                            if (File.Exists(sourcefile)){                        
                                File.Delete(sourcefile);                        
                                GlobalVariables.DatabaseConnection.Delete(item);                        
                                RefreshResults();                                
                            } else {
                                System.Windows.Forms.MessageBox.Show(string.Format("File not found: {0}", sourcefile));    
                            }
                        } 
                        System.Windows.Forms.MessageBox.Show(string.Format("Deleted {0}", stuff));
                    } else {
                        //do not delete
                    }
                }
            } 
        } 
        public ICommand MoveFile  
        {  
            get { return new DelegateCommand(this.OnMove); }  
        }  
        private void OnMove()  
        {  
            string stuff = "";
            string movefolder = "";
            string sourcefile = ((SimsPackage)_packagesView.CurrentItem).Location;
            string filename = ((SimsPackage)_packagesView.CurrentItem).PackageName;
            string destination = "";
            int selection = ResultsWindow.resultsView.SelectedItems.Count;
            if (selection == 1){                
                using(var MoveFolder = new FolderBrowserDialog())
                {
                    DialogResult result = MoveFolder.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        movefolder = MoveFolder.SelectedPath;
                        destination = System.IO.Path.Combine(movefolder, filename);
                        if (File.Exists(sourcefile)){
                            SimsPackage item = (SimsPackage)_packagesView.CurrentItem;
                            item.Location = destination;
                            File.Move(sourcefile, destination);                        
                            GlobalVariables.DatabaseConnection.UpdateWithChildren(item);
                            RefreshResults();
                        } else {
                            System.Windows.Forms.MessageBox.Show(string.Format("File {0} not found at source. Did it get deleted?", sourcefile));    
                        }
                        System.Windows.Forms.MessageBox.Show(string.Format("Moved {0}!", ((SimsPackage)_packagesView.CurrentItem).PackageName));
                    } else {
                        System.Windows.Forms.MessageBox.Show(string.Format("Please pick somewhere to move file."));
                    }
                }            
            } else if (selection != 1 && selection != 0){
                foreach (var item in ResultsWindow.resultsView.SelectedItems){
                    SimsPackage thing = (SimsPackage)item;
                    if (String.IsNullOrEmpty(stuff)){
                        stuff = string.Format("{0}", thing.PackageName);
                    } else {
                        stuff += string.Format("\n {0}", thing.PackageName);
                    }                
                }
                using(var MoveFolder = new FolderBrowserDialog())
                {
                    DialogResult result = MoveFolder.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        movefolder = MoveFolder.SelectedPath;
                        foreach (var thing in ResultsWindow.resultsView.SelectedItems){
                            SimsPackage item = ((SimsPackage)thing);
                            filename = item.PackageName;
                            destination = System.IO.Path.Combine(movefolder, filename);
                            sourcefile = item.Location;
                            if (File.Exists(sourcefile)){                            
                                item.Location = destination;
                                File.Move(sourcefile, destination);                        
                                GlobalVariables.DatabaseConnection.UpdateWithChildren(item);
                                RefreshResults();
                            } else {
                                System.Windows.Forms.MessageBox.Show(string.Format("File not found: {0}", item.PackageName));    
                            }
                        }
                        System.Windows.Forms.MessageBox.Show(string.Format("Moved {0}", stuff));
                    } else {
                        System.Windows.Forms.MessageBox.Show(string.Format("Please pick somewhere to move file."));
                    }
                }
                
            }
        }  
  
        protected virtual void OnPropertyChanged(string propertyName)  
        {  
            PropertyChangedEventHandler handler = this.PropertyChanged;  
            if (handler != null)  
            {  
                var e = new PropertyChangedEventArgs(propertyName);  
                handler(this, e);  
            }  
        }      
        
    }

     public class DelegateCommand : ICommand  
    {  
        public delegate void SimpleEventHandler();  
        private SimpleEventHandler handler;  
        private bool isEnabled = true;  
 
        public event EventHandler CanExecuteChanged;  
 
        public DelegateCommand(SimpleEventHandler handler)  
        {  
            this.handler = handler;  
        }  
 
        private void OnCanExecuteChanged()  
        {  
            if (this.CanExecuteChanged != null)  
            {  
                this.CanExecuteChanged(this, EventArgs.Empty);  
            }  
        }  
 
        bool ICommand.CanExecute(object arg)  
        {  
            return this.IsEnabled;  
        }  
 
        void ICommand.Execute(object arg)  
        {  
            this.handler();  
        }  
 
        public bool IsEnabled  
        {  
            get { return this.isEnabled; }  
 
            set 
            {  
                this.isEnabled = value;  
                this.OnCanExecuteChanged();  
            }  
        }  
    } 

    static class PagingUtils {
         
        public static IEnumerable<T> Page<T>(this IEnumerable<T> en, int pageSize, int page) {
            return en.Skip(page * pageSize).Take(pageSize);
        }
        public static IQueryable<T> Page<T>(this IQueryable<T> en, int pageSize, int page) {
            return en.Skip(page * pageSize).Take(pageSize);
        }
    }
}