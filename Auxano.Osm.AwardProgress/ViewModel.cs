using Auxano.Osm.Api;
using Newtonsoft.Json;
using SpreadsheetLight;
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
        private readonly Dictionary<string, Badge[]> cachedBadges = new Dictionary<string, Badge[]>();
        private readonly Dictionary<string, Member[]> cachedMembers = new Dictionary<string, Member[]>();
        private readonly Dictionary<string, TermArray> cachedTerms = new Dictionary<string, TermArray>();
        private readonly Manager manager;
        private bool isBusy;
        private Badge selectedBadge;
        private Group selectedGroup;
        private Section selectedSection;
        private Term selectedTerm;
        private string status;

        public ViewModel()
        {
            this.GoCommand = new ActionCommand(this.Go);
            this.Members = new ObservableCollection<MemberSelection>();
            this.Exceptions = new ObservableCollection<ErrorReport>();
            this.Groups = new ObservableCollection<Group>();
            this.Sections = new ObservableCollection<Section>();
            this.Terms = new ObservableCollection<Term>();
            this.Badges = new ObservableCollection<Badge>();
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

        public ObservableCollection<Badge> Badges { get; private set; }
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

        public ObservableCollection<MemberSelection> Members { get; private set; }
        public ObservableCollection<Section> Sections { get; private set; }

        public Badge SelectedBadge
        {
            get
            {
                return this.selectedBadge;
            }
            set
            {
                this.selectedBadge = value;
                this.FirePropertyChanged();
                this.UpdateGoStatus();
            }
        }

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
                        this.cachedTerms[value.Id] = t;
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
                this.DisplayBadges(() => this.DisplayMembers());
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
            this.Status = "Finished generating progress reports";
        }

        private void DisplayBadges(Action onCompleted = null)
        {
            Badge[] badges;
            if (this.cachedBadges.TryGetValue(this.selectedTerm.Id, out badges))
            {
                this.RefreshBadges(badges, onCompleted);
            }
            else
            {
                this.Status = "Retrieving list of badges for term, please wait...";
                this.StartBackgroundWork(this.LoadBadgesForTerm, m => this.RefreshBadges(m, onCompleted));
            }
        }

        private void DisplayMembers(Action onCompleted = null)
        {
            Member[] members;
            if (this.cachedMembers.TryGetValue(this.selectedTerm.Id, out members))
            {
                this.RefreshMembers(members, onCompleted);
            }
            else
            {
                this.Status = "Retrieving list of members for term, please wait...";
                this.StartBackgroundWork(this.LoadMembersForTerm, m => this.RefreshMembers(m, onCompleted));
            }
        }

        private void FirePropertyChanged([CallerMemberName] string memberName = null)
        {
            if (this.PropertyChanged == null) return;
            var handler = this.PropertyChanged;
            handler.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        private async Task<object> GenerateAwardProgress(Member[] members)
        {
            var progress = await this.manager.Badge.ListProgressForBadgeAsync(this.selectedSection, this.selectedBadge, this.selectedTerm);
            var indexed = progress.ToDictionary(p => p.Member.Id);
            var tasks = this.selectedBadge.Tasks
                .OrderBy(t => t.Module)
                .ThenBy(t => t.Id)
                .ToArray();
            var modules = tasks.GroupBy(t => t.Module)
                .Select(m => new { Name = m.Key, Gold = m.Count(), Silver = m.Count() / 2 })
                .OrderBy(m => m.Name)
                .ToArray();
            foreach (var member in members)
            {
                BadgeProgress memberProgress;
                if (indexed.TryGetValue(member.Id, out memberProgress))
                {
                    var document = new SLDocument();
                    var row = 0;
                    document.SetCellValue(++row, 1, this.selectedBadge.Name + " Progress for " + member.FirstName + " " + member.FamilyName);
                    document.MergeWorksheetCells(1, 1, 1, 4);
                    document.SetCellValue(++row, 1, "Module");
                    document.SetCellValue(row, 2, "Task");
                    document.SetCellValue(row, 3, "Completed");
                    document.SetCellValue(row, 4, "Details");
                    document.SetCellValue(row, 5, "Description");
                    var moduleProgress = modules.ToDictionary(m => m.Name, m => 0);

                    foreach (var task in tasks)
                    {
                        document.SetCellValue(++row, 1, task.Module);
                        document.SetCellValue(row, 2, task.Name);
                        document.SetCellValue(row, 5, task.Description);
                        string status;
                        if (memberProgress.TaskStatus.TryGetValue(task.Id, out status))
                        {
                            var completed = !string.IsNullOrEmpty(status);
                            document.SetCellValue(row, 3, completed ? "Yes" : null);
                            document.SetCellValue(row, 4, status == "Yes" ? null : status);
                            if (completed) moduleProgress[task.Module] += 1;
                        }
                    }

                    document.SetCellValue(++row, 1, "Module");
                    document.SetCellValue(row, 2, "# Tasks");
                    document.SetCellValue(row, 3, "Silver");
                    document.SetCellValue(row, 4, "Gold");
                    foreach (var module in modules)
                    {
                        document.SetCellValue(++row, 1, module.Name);
                        document.SetCellValue(row, 2, moduleProgress[module.Name]);
                        document.SetCellValue(row, 3, moduleProgress[module.Name] >= module.Silver ? "Yes" : null);
                        document.SetCellValue(row, 4, moduleProgress[module.Name] >= module.Gold ? "Yes" : null);
                    }

                    for (var loop = 0; loop < 4; loop++) document.AutoFitColumn(loop);

                    var fileName = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "OSM",
                        "AwardProgress",
                        this.selectedTerm.Name,
                        member.FamilyName + "," + member.FirstName + "-" + this.selectedBadge.Name + ".xlsx");
                    var directory = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                    if (File.Exists(fileName)) File.Delete(fileName);
                    document.SaveAs(fileName);
                }
            }

            return null;
        }

        private void Go(object arg)
        {
            var selectedMembers = this.Members
                .Where(m => m.Selected)
                .Select(m => m.Member)
                .ToArray();
            if (selectedMembers.Any())
            {
                this.Status = "Generating progress reports, please wait...";
                this.StartBackgroundWork(() => this.GenerateAwardProgress(selectedMembers), this.AwardProgressGenerated);
            }
            else
            {
                this.Status = "Please select the members to generate the award progress reports for";
            }
        }

        private async Task<Badge[]> LoadBadgesForTerm()
        {
            var badges = await this.manager.Badge.ListForSectionAsync(this.selectedSection, 1, this.selectedTerm);
            var loadedBadges = badges.ToArray();
            this.cachedBadges[this.selectedTerm.Id] = loadedBadges;
            return loadedBadges;
        }

        private async Task<Member[]> LoadMembersForTerm()
        {
            var members = await this.manager.Member.ListForSectionAsync(this.selectedSection, this.selectedTerm);
            var loadedMembers = members.ToArray();
            this.cachedMembers[this.selectedTerm.Id] = loadedMembers;
            return loadedMembers;
        }

        private void LoadSettings(InitialData data)
        {
            this.Groups.Clear();
            this.SelectedGroup = null;
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
            foreach (var term in terms.OrderByDescending(t => t.StartDate))
            {
                this.Terms.Add(term);
            }

            this.SelectedTerm = terms.Current;
            this.UpdateGoStatus();
        }

        private void RefreshBadges(Badge[] badges, Action onCompleted)
        {
            this.Status = string.Empty;
            this.Badges.Clear();
            foreach (var badge in badges)
            {
                this.Badges.Add(badge);
            }

            if (onCompleted != null) onCompleted();
        }

        private void RefreshMembers(Member[] members, Action onCompleted)
        {
            this.Status = string.Empty;
            this.Members.Clear();
            foreach (var member in members)
            {
                this.Members.Add(new MemberSelection(member));
            }

            if (onCompleted != null) onCompleted();
        }

        private void StartBackgroundWork<T>(Func<Task<T>> onWork, Action<T> onDone)
        {
            this.IsBusy = true;
            T result = default(T);
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
                && (this.selectedTerm != null)
                && (this.selectedBadge != null);
        }
    }
}