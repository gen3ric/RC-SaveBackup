using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Saved_Game_Backup.Helper;

namespace Saved_Game_Backup.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        
        private Visibility _autoBackupVisibility;
        public Visibility AutoBackupVisibility {
            get { return _autoBackupVisibility; }
            set { _autoBackupVisibility = value; }
        }

        private string _myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _sbtPath = "\\Save Backup Tool\\";
       
        public ObservableCollection<string> HardDrives { get; set; } 
        public ObservableCollection<Game> GamesList { get; set; } 
        public ObservableCollection<Game> GamesToBackup { get; set; }
        public ObservableCollection<string> GameNames { get; set; }

        private Brush _background;
        public Brush Background {
            get { return _background; }
            set { _background = value; }
        }
        private string _selectedHardDrive;
        public string SelectedHardDrive {
            get { return _selectedHardDrive; }
            set { _selectedHardDrive = value; }
        }
        private Game _selectedGame;
        public Game SelectedGame
        {
            get { return _selectedGame; }
            set { _selectedGame = value; }
        }
        private Game _selectedBackupGame;
        public Game SelectedBackupGame
        {
            get { return _selectedBackupGame; }
            set { _selectedBackupGame = value; }
        }
        private string _specifiedFolder;
        public string SpecifiedFolder {
            get { return _specifiedFolder; }
            set { _specifiedFolder = value; }
        }
        private bool _backupEnabled;
        private int _maxBackups;
        public int MaxBackups {
            get { return _maxBackups; }
            set { _maxBackups = value; }
        }
        private int _theme;
        public int Theme {
            get { return _theme; }
            set { _theme = value; }
        }

        private BackupType _backupType;
        public BackupType BackupType {
            get { return _backupType; }
            set { _backupType = value; }
        }

        public RelayCommand MoveToBackupList
        {
            get
            {
                return new RelayCommand(() => ToBackupList());
            }
        }
        public RelayCommand MoveToGamesList
        {
            get
            {
                return new RelayCommand(() => ToGamesList());
            }
        }
        //public RelayCommand BackupSaves {
        //    get { return new RelayCommand(() => ExecuteBackup());}
        //}
        //public RelayCommand BackupAndZip {
        //    get { return new RelayCommand(() => ExecuteBackupAndZip()); }
        //}
        public RelayCommand OpenAddGameWindow {
            get { return new RelayCommand(() => ExecuteAddGame());}
        }
        public RelayCommand StartBackup {
            get { return new RelayCommand(() => ExecuteStartBackup());}
        }
        public RelayCommand ResetList
        {
            get { return new RelayCommand(() => ExecuteReset()); }
        }
        public RelayCommand SpecifyFolder {
            get {return new RelayCommand(() => ExecuteSpecifyFolder());}
        }
        public RelayCommand AutoBackup {
            get { return new RelayCommand(() => ExecuteAutoBackupToggle()); }
        }
        public RelayCommand DetectGames {
            get { return new RelayCommand(() => ExecuteDetectGames()); }
        }
        public RelayCommand SetThemeLight
        {
            get { return new RelayCommand(() => ExecuteSetThemeLight()); }
        }
        public RelayCommand SetThemeDark
        {
            get { return new RelayCommand(() => ExecuteSetThemeDark()); }
        }
        public RelayCommand OpenOptionsWindow {
            get { return new RelayCommand(() => ExecuteOpenOptionsWindow());}
        }
        public RelayCommand Close {
            get { return new RelayCommand(() => CloseApplication()); }
        }

        public MainViewModel() {
            HardDrives = new ObservableCollection<string>();
            GamesList = DirectoryFinder.ReturnGamesList();
            GamesToBackup = new ObservableCollection<Game>();
            DirectoryFinder.CheckDirectories();
            SetUpInterface();
            CreateHardDriveCollection();

            Messenger.Default.Register<OptionMessage>(this, s => {
                    BackupType = s.BackupType;
                if (s.HardDrive != null)
                    SelectedHardDrive = s.HardDrive;
                if (s.SpecifiedFolder != null)
                    SpecifiedFolder = s.SpecifiedFolder;
            });
        }

        ~MainViewModel() {
          CloseApplication();
        }

        private void SetUpInterface() {
            if (!PrefSaver.CheckForPrefs())
            {
                _maxBackups = 5;
                _theme = 0;
            }
            else
            {
                var p = new PrefSaver();
                var prefs = p.LoadPrefs();
                _maxBackups = prefs.MaxBackups;
                _theme = prefs.Theme;
                GamesToBackup = prefs.SelectedGames;
                _selectedHardDrive = prefs.HardDrive;

                var listToRemove = new ObservableCollection<Game>();
                foreach (Game game in prefs.SelectedGames)
                {
                    foreach (Game g in GamesList)
                    {
                        if (game.Name == g.Name)
                            listToRemove.Add(g);
                    }
                    foreach (Game gameBeingRemoved in listToRemove)
                        GamesList.Remove(gameBeingRemoved);

                }
                RaisePropertyChanged(() => GamesList);
                ToggleTheme();
            }
            AutoBackupVisibility = Visibility.Hidden;
            RaisePropertyChanged(() => AutoBackupVisibility);
        }

        private void SaveUserPrefs() {
            var p = new PrefSaver();
            p.SavePrefs(new UserPrefs(_theme, _maxBackups, _selectedHardDrive, GamesToBackup));
        }

        private void ExecuteDetectGames() {
            if (_selectedHardDrive == null) {
                MessageBox.Show("Storage disk not selected. \r\nPlease select the drive where your \r\nsaved games are stored.");
                ExecuteReset();
                return;
            }
            
            GamesToBackup = DirectoryFinder.PollDirectories(_selectedHardDrive, GamesList);
            foreach (Game game in GamesToBackup)
                GamesList.Remove(game);

            RaisePropertyChanged(() => GamesToBackup);
            RaisePropertyChanged(() => GamesList);
        
        }

        private void ExecuteSpecifyFolder() {
            _specifiedFolder = DirectoryFinder.SpecifyFolder();
        }

        private void CreateHardDriveCollection() {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives) {
                HardDrives.Add(drive.RootDirectory.ToString());
            }
        }

        private void ExecuteAutoBackupToggle() {
            

            if (_backupEnabled) {
                //This is for turning **OFF** AutoBackup
                _backupEnabled = false;
                _autoBackupVisibility = Visibility.Hidden;
                Backup.DeactivateAutoBackup();
            }
            else {
                //This is for turning **ON** AutoBackup
                if (!CanBackup())
                    return;

                AutoBackupVisibility = Visibility.Visible;
                _backupEnabled = true;
                _specifiedFolder = DirectoryFinder.SpecifyFolder();
                Backup.ActivateAutoBackup(GamesToBackup, _selectedHardDrive, _specifiedFolder);
            }
            RaisePropertyChanged(() => AutoBackupVisibility);    
            SaveUserPrefs();
            
        }

        private async void ToBackupList() {
            Game game = null;
            if (_selectedGame == null)
                return;

            for (int i = 0; i < GamesList.Count(); i++) {
                if (_selectedGame.Name == GamesList[i].Name) {
                    game = GamesList[i];
                    break;
                }
            }

            if (game != null) {
                GamesList.Remove(game);
                RaisePropertyChanged(() => GamesList);
                
                //Pull in Thumb data with GiantBombAPI
                await GetThumb(game);
                GamesToBackup.Add(game);
                GamesToBackup = new ObservableCollection<Game>(GamesToBackup.OrderBy(x => x.Name));
                RaisePropertyChanged(() => GamesToBackup);
            }
        }

        private void ToGamesList()
        {
            Game game = null;
            if (_selectedBackupGame == null)
                return;

            for (int i = 0; i < GamesToBackup.Count(); i++) {
                if (_selectedBackupGame.Name == GamesToBackup[i].Name) {
                    game = GamesToBackup[i];
                    break;
                }
            }

            if (game != null) {
                GamesToBackup.Remove(game);
                RaisePropertyChanged(() => GamesToBackup);
                GamesList.Add(game);
            }
            GamesList = new ObservableCollection<Game>(GamesList.OrderBy(x => x.Name));
            RaisePropertyChanged(() => GamesList);
        }

        private void ExecuteBackup() {
            if (!CanBackup())
                return;

            if(Backup.BackupSaves(GamesToBackup, SelectedHardDrive, false, _specifiedFolder))
                MessageBox.Show("Saved games successfully backed up. \r\n");
            SaveUserPrefs();
            
        }
        
        private void ExecuteBackupAndZip() {
            if (!CanBackup())
                return;

            if(Backup.BackupAndZip(GamesToBackup, SelectedHardDrive, true, _specifiedFolder))
                MessageBox.Show("Saved games successfully backed up. \r\n");
            SaveUserPrefs();
        }

        private void ExecuteReset() {
            GamesList = DirectoryFinder.ReturnGamesList();
            GamesToBackup.Clear();
            _specifiedFolder = null;
            _selectedHardDrive = null;
            _selectedGame = null;
            _selectedBackupGame = null;

            RaisePropertyChanged(() => GamesList);
            RaisePropertyChanged(() => GamesToBackup);
            RaisePropertyChanged(() => SelectedHardDrive);
            RaisePropertyChanged(() => SelectedBackupGame);
            RaisePropertyChanged(() => SelectedGame);
        }

        private bool CanBackup() {
            if (SelectedHardDrive == null) {
                MessageBox.Show("Storage disk not selected. \r\nPlease select the drive where your \r\nsaved games are stored.");
                ExecuteReset();
                return false;
            }

            if (!GamesToBackup.Any()) {
                MessageBox.Show("No games selected. \n\rPlease select at least one game.");
                ExecuteReset();
                return false;
            }
            
            return true;
        }

        private void ExecuteSetThemeLight() {
            _theme = 0;
            ToggleTheme();
        }
        
        private void ExecuteSetThemeDark() {
            _theme = 1;
            ToggleTheme();
        }

        private void ToggleTheme() {
            var bc = new BrushConverter();  
            var brush = (Brush)bc.ConvertFrom("#FF2D2D30"); 
            brush.Freeze();
            _background = (_theme == 0) ? Brushes.DeepSkyBlue : brush;
            RaisePropertyChanged(() => Background);
            
        }

        private void CloseApplication() {
            SaveUserPrefs();
            if (Application.Current != null)
              Application.Current.Shutdown();
        }

        private async Task GetThumb(Game game) {
            var gb = new GiantBombAPI(game);
            await gb.GetThumb(game);
            game.ThumbnailPath = gb.ThumbnailPath;
        }

        private void ExecuteOpenOptionsWindow() {
            //var optionsWindowVM = SimpleIoc.Default.GetInstance<OptionsViewModel>();
            var optionsWindow = new OptionsWindow();
            optionsWindow.Show();
        }

        private void ExecuteStartBackup() {
            if (BackupType == BackupType.ToFolder)
                ExecuteBackup();
            else if (BackupType == BackupType.ToZip)
                ExecuteBackupAndZip();
            else
                ExecuteAutoBackupToggle();
        }

        private void ExecuteAddGame() {
            //var optionsWindowVM = SimpleIoc.Default.GetInstance<AddGameViewModel>();
            var addGameWindow = new AddGameWindow();
            addGameWindow.Show();
        }
    }
}