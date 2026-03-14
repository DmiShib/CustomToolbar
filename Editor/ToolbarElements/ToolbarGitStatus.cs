namespace CustomToolbar.Editor.ToolbarElements
{
    using Utils;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using System.Collections.Generic;


    internal sealed class ToolbarGitStatus : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/GitStatus";

        private string _rootRepoPath;
        private List<string> _subRepoPaths;
        private bool _isGitReady;

        public static ToolbarGitStatus Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Git Status";
        protected override string Tooltip => "View and switch Git branches. A '*' indicates uncommitted changes.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnInit()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            if (RootElement != null)
            {
                RootElement.content = GetStatusContent();
                RefreshUI();
            }
        }

        protected override MainToolbarElement CreateElement()
        {
            _isGitReady = GitUtils.IsGitInstalled;
            var dropdown = new MainToolbarDropdown(GetStatusContent(), rect => BuildGitMenu().DropDown(rect));
            dropdown.enabled = _isGitReady;
            return dropdown;
        }

        private MainToolbarContent GetStatusContent()
        {
            _isGitReady = GitUtils.IsGitInstalled;

            if (!_isGitReady)
            {
                Texture2D warnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D;
                return new MainToolbarContent(" Git: N/A", warnIcon, "Git command not found. Is Git installed and in your system's PATH?");
            }

            List<string> allRepos = GitUtils.FindGitRepositories();
            string projectRootPath = Directory.GetParent(Application.dataPath)?.FullName;

            _rootRepoPath = allRepos.Find(p => p == projectRootPath);
            _subRepoPaths = allRepos.Where(p => p != projectRootPath).ToList();

            int totalRepos = _subRepoPaths.Count + (!string.IsNullOrEmpty(_rootRepoPath) ? 1 : 0);

            if (totalRepos > 0)
            {
                Texture2D connectedIcon = EditorGUIUtility.IconContent("d_CacheServerConnected").image as Texture2D;
                return new MainToolbarContent($" Git: {totalRepos}", connectedIcon, $"{totalRepos} Git repositories found in the project.");
            }
            else
            {
                Texture2D disconnectedIcon = EditorGUIUtility.IconContent("d_CacheServerDisconnected").image as Texture2D;
                return new MainToolbarContent("Git: (None)", disconnectedIcon, "No Git repository found in the project.");
            }
        }

        private GenericMenu BuildGitMenu()
        {
            var menu = new GenericMenu();

            if (!_isGitReady)
            {
                menu.AddDisabledItem(new GUIContent("Git not found on this system"));
                return menu;
            }

            if (!string.IsNullOrEmpty(_rootRepoPath))
            {
                string currentBranch = GitUtils.GetCurrentBranch(_rootRepoPath);
                List<string> allBranches = GitUtils.GetLocalBranches(_rootRepoPath);
                bool isDirty = GitUtils.HasUncommittedChanges(_rootRepoPath);

                string rootMenuName = $"Unity{(isDirty ? "*" : "")}";

                foreach (string branch in allBranches)
                {
                    menu.AddItem(new GUIContent($"{rootMenuName}/{branch}"), branch == currentBranch, () => GitUtils.SwitchBranch(_rootRepoPath, branch));
                }
            }

            if (_subRepoPaths != null && _subRepoPaths.Any())
            {
                if (!string.IsNullOrEmpty(_rootRepoPath)) menu.AddSeparator("");

                foreach (string repoPath in _subRepoPaths)
                {
                    string repoName = Path.GetFileName(repoPath);
                    string currentBranch = GitUtils.GetCurrentBranch(repoPath);
                    List<string> allBranches = GitUtils.GetLocalBranches(repoPath);
                    bool isDirty = GitUtils.HasUncommittedChanges(repoPath);

                    if (!allBranches.Any()) continue;

                    string repoMenuName = $"{repoName}{(isDirty ? "*" : "")}";
                    foreach (string branch in allBranches)
                    {
                        menu.AddItem(new GUIContent($"{repoMenuName}/{branch}"), branch == currentBranch, () => GitUtils.SwitchBranch(repoPath, branch));
                    }
                }
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No Git repository found"));

            return menu;
        }
    }
}