using CommandLine;
using CommandLine.Text;

namespace cic
{
    public class Options
    {
        [Option('u', "UserName", HelpText = "Jira username", Required = true)]
        public string UserName { get; set; }

        [Option('p', "Password", HelpText = "Jira password", Required = true)]
        public string Password { get; set; }

        [Option('k', "ProjectKey", HelpText = "Jira project key", Required = true)]
        public string ProjectKey { get; set; }

        [Option('t', "IssueTitle", HelpText = "Title of the Jira issue", Required = true)]
        public string IssueTitle { get; set; }

        [Option('r', "RepositoryDirectory", HelpText = "Full path to the git repository", Required = true)]
        public string RepositoryDirectory { get; set; }

        [Option('i', "IssueType", HelpText = "Type of Jira issue. Possible values can depend on the Jira project, but are usually 'Bug', 'Task', 'Sub-Task' and 'Improvement'. Defaults to 'Bug'", Required = false)]
        public string IssueType { get; set; }
        
        [Option('d', "IssueDescription", HelpText = "Description of the Jira issue", Required = false)]
        public string IssueDescription { get; set; }

        [Option('a', "AssigneeUserName", HelpText = "UserName to assign the issue to. Defaults to the username supplied with the -u parameter", Required = false)]
        public string AssigneeUserName { get; set; }

        [Option('j', "OrganizationJiraUrl", HelpText = "Your organization's Jira Url. This is what you type into the browser to get to your hosted Jira website. If not specified here, it can live in the configuration file of this program as an app setting with key=OrganizationJiraUrl", Required = false)]
        public string OrganizationJiraUrl { get; set; }

        [HelpOption('?', "help")]
        public virtual string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}