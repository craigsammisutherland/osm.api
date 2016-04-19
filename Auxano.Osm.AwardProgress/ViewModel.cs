using Auxano.Osm.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Auxano.Osm.AwardProgress
{
    public class ViewModel
        : INotifyPropertyChanged
    {
        private readonly Dictionary<string, TermArray> cachedTerms = new Dictionary<string, TermArray>();
        private readonly Manager manager;
        private bool isBusy;
        private Group selectedGroup;
        private Section selectedSection;
        private Term selectedTerm;
        private string status;

        public ViewModel()
        {
            this.GoCommand = new ActionCommand(this.Go);
            this.Exceptions = new ObservableCollection<ErrorReport>();
            this.Groups = new ObservableCollection<Group>();
            this.Sections = new ObservableCollection<Section>();
            this.Terms = new ObservableCollection<Term>();
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

        public ObservableCollection<ErrorReport> Exceptions { get; private set; }

        public ActionCommand GoCommand { get; private set; }

        public ObservableCollection<Group> Groups { get; private set; }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }
            set
            {
                this.isBusy = value;
                this.FirePropertyChanged();
                this.UpdateGoStatus();
            }
        }

        public ObservableCollection<Section> Sections { get; private set; }

        public Group SelectedGroup
        {
            get
            {
                return this.selectedGroup;
            }
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
            }
        }

        public Section SelectedSection
        {
            get
            {
                return this.selectedSection;
            }
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
            get
            {
                return this.selectedTerm;
            }
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
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
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

        private void AwardProgressGenerated(object obj)
        {
            throw new NotImplementedException();
        }

        private void FirePropertyChanged([CallerMemberName] string memberName = null)
        {
            if (this.PropertyChanged == null) return;
            var handler = this.PropertyChanged;
            handler.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        private Task<object> GenerateAwardProgress()
        {
            throw new NotImplementedException();
        }

        private void Go(object arg)
        {
            this.StartBackgroundWork(this.GenerateAwardProgress, this.AwardProgressGenerated);
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
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateGoStatus()
        {
            this.GoCommand.IsEnabled = !this.isBusy
                && (this.selectedGroup != null)
                && (this.selectedSection != null)
                && (this.selectedTerm != null);
        }
    }
}