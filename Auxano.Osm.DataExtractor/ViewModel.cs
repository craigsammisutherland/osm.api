using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Auxano.Osm.Api;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Auxano.Osm.DataExtractor
{
    public class ViewModel
        : INotifyPropertyChanged
    {
        private readonly Dictionary<string, TermArray> cachedTerms = new Dictionary<string, TermArray>();
        private readonly Manager manager;
        private bool isBusy;
        private string lastFileName;
        private string selectedFile;
        private Group selectedGroup;
        private Report selectedReport;
        private Section selectedSection;
        private Term selectedTerm;
        private string status;

        public ViewModel()
        {
            this.BrowseCommand = new ActionCommand(this.Browse);
            this.GoCommand = new ActionCommand(this.Go);
            this.Exceptions = new ObservableCollection<ErrorReport>();
            this.Reports = new ObservableCollection<Report>(Report.All());
            this.Groups = new ObservableCollection<Group>();
            this.Sections = new ObservableCollection<Section>();
            this.Terms = new ObservableCollection<Term>();
            this.SelectedReport = this.Reports.FirstOrDefault();
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "settings.json")))
            {
                this.manager = ReadSettings();
                this.Status = "Retrieving group and section details, please wait...";
                this.StartBackgroundWork(async () => await InitialData.LoadAsync(this.manager), this.LoadSettings);
            }
            else
            {
                this.Status = "Can not to find settings file, unable to continue";
                this.IsBusy = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ActionCommand BrowseCommand { get; private set; }
        public ObservableCollection<ErrorReport> Exceptions { get; private set; }

        public ActionCommand GoCommand { get; private set; }

        public ObservableCollection<Group> Groups { get; private set; }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                this.isBusy = value;
                this.FirePropertyChanged();
                this.UpdateGoStatus();
            }
        }

        public ObservableCollection<Report> Reports { get; private set; }

        public ObservableCollection<Section> Sections { get; private set; }

        public string SelectedFile
        {
            get { return this.selectedFile; }
            set
            {
                this.selectedFile = value;
                this.FirePropertyChanged();
            }
        }

        public Group SelectedGroup
        {
            get { return this.selectedGroup; }
            set
            {
                this.selectedGroup = value;
                this.FirePropertyChanged();
                this.Sections.Clear();
                if (value == null)
                {
                    this.UpdateGoStatus();
                    return;
                }

                foreach (var section in value.Sections)
                {
                    this.Sections.Add(section);
                }

                this.SelectedSection = this.Sections.FirstOrDefault();
                this.UpdateGoStatus();
                this.GenerateFileName();
            }
        }

        public Report SelectedReport
        {
            get { return this.selectedReport; }
            set
            {
                this.selectedReport = value;
                this.FirePropertyChanged();
                this.UpdateGoStatus();
                this.GenerateFileName();
            }
        }

        public Section SelectedSection
        {
            get { return this.selectedSection; }
            set
            {
                this.selectedSection = value;
                this.FirePropertyChanged();
                this.Terms.Clear();
                if (value == null)
                {
                    this.UpdateGoStatus();
                    return;
                }

                TermArray terms;
                this.Terms.Clear();
                if (this.cachedTerms.TryGetValue(value.Id, out terms))
                {
                    this.LoadTerms(terms);
                }
                else
                {
                    this.StartBackgroundWork(async () => await this.manager.Term.ListForSectionAsync(value), t =>
                    {
                        var newTerms = t as TermArray;
                        this.cachedTerms[value.Id] = newTerms;
                        this.LoadTerms(terms);
                    });
                }
            }
        }

        public Term SelectedTerm
        {
            get { return this.selectedTerm; }
            set
            {
                this.selectedTerm = value;
                this.FirePropertyChanged();
                if (value == null)
                {
                    this.UpdateGoStatus();
                    return;
                }

                this.cachedTerms[value.Section.Id] = this.cachedTerms[value.Section.Id].ChangeCurrentTerm(value);
                this.UpdateGoStatus();
                this.GenerateFileName();
            }
        }

        public string Status
        {
            get { return this.status; }
            set
            {
                this.status = value;
                this.FirePropertyChanged();
            }
        }

        public ObservableCollection<Term> Terms { get; private set; }

        private static Manager ReadSettings()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "settings.json"));
            var setings = JsonConvert.DeserializeObject<Settings>(json);
            var manager = new Manager(setings.ApiId, setings.Token, setings.UserId, setings.Secret);
            return manager;
        }

        private void Browse(object obj)
        {
            var dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                DereferenceLinks = true,
                InitialDirectory = Path.GetDirectoryName(this.SelectedFile),
                Filter = "Reports (*" + this.selectedReport.Extension + ")|*" + this.selectedReport.Extension + "|All files (*.*)|*.*",
                FilterIndex = 1,
                OverwritePrompt = true,
                ValidateNames = true,
                Title = "Select Filename to Save the Report To",
                FileName = this.selectedFile
            };
            if (dialog.ShowDialog().GetValueOrDefault()) this.SelectedFile = dialog.FileName;
        }

        private void DoneStatus(object obj)
        {
            this.Status = this.SelectedReport.Name + " complete";
        }

        private void FirePropertyChanged([CallerMemberName] string memberName = null)
        {
            if (this.PropertyChanged == null) return;
            var handler = this.PropertyChanged;
            handler.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        private async Task<object> Generate()
        {
            var fileName = await this.SelectedReport.Generate(this, this.manager);
            Process.Start(fileName);
            return null;
        }

        private void GenerateFileName()
        {
            if ((this.SelectedReport == null)
                    || (this.selectedGroup == null)
                    || (this.selectedSection == null)
                    || (this.SelectedTerm == null))
            {
                return;
            }

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                string.Join("-", new[] {
                    this.SelectedReport.Name,
                    this.selectedGroup.Name,
                    this.selectedSection.Name,
                    this.SelectedTerm.Name
                }) + this.SelectedReport.Extension);
            if (string.IsNullOrEmpty(this.selectedFile) || (this.selectedFile == this.lastFileName))
            {
                this.SelectedFile = path;
            }

            this.lastFileName = path;
        }

        private void Go(object arg)
        {
            this.Status = "Generating " + this.SelectedReport.Name + ", please wait...";
            this.StartBackgroundWork(this.Generate, this.DoneStatus);
        }

        private void LoadSettings(object output)
        {
            this.Groups.Clear();
            this.SelectedGroup = null;

            var data = output as InitialData;
            if (data == null)
            {
                this.Status = "Unable to retrieve group and section details";
                return;
            }
            if (data.Terms != null) this.cachedTerms.Add(data.Terms.Section.Id, data.Terms);
            foreach (var group in data.Groups)
            {
                this.Groups.Add(group);
            }

            this.SelectedGroup = this.Groups.FirstOrDefault();
            this.Status = string.Empty;
        }

        private void LoadTerms(TermArray terms)
        {
            foreach (var term in terms)
            {
                this.Terms.Add(term);
            }

            this.SelectedTerm = terms.Current;
            this.UpdateGoStatus();
            this.GenerateFileName();
        }

        private void StartBackgroundWork(Func<Task<object>> onWork, Action<object> onDone)
        {
            this.IsBusy = true;
            object result = null;
            this.Exceptions.Clear();
            Task.Run(async () =>
            {
                result = await onWork();
            }).ContinueWith(t =>
            {
                this.IsBusy = false;
                if (t.Exception != null)
                {
                    this.Status = "An unexpected error has occurred!";
                    foreach (var exception in t.Exception.Flatten().InnerExceptions)
                    {
                        this.Exceptions.Add(new ErrorReport(exception));
                    }
                }
                else
                {
                    onDone(result);
                }

                this.UpdateGoStatus();
                this.GenerateFileName();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateGoStatus()
        {
            this.GoCommand.IsEnabled = !this.isBusy
                && (this.selectedGroup != null)
                && (this.selectedReport != null)
                && (this.selectedSection != null)
                && (this.selectedTerm != null);
            this.BrowseCommand.IsEnabled = !this.isBusy;
        }
    }
}