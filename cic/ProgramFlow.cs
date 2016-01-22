using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace cic
{
    class ProgramFlow
    {
        private readonly Options _options;
        private readonly ConsoleColor _originalColor;

        public ProgramFlow(Options options)
        {
            _originalColor = Console.ForegroundColor;
            _options = options;
        }

        public void Run()
        {
            var json = "{ \"fields\": " +
                            "{ " +
                                "\"project\": { \"key\": \"PROJECTKEY\" }, " +
                                "\"assignee\":{ \"name\": \"ISSUEASSIGNEE\" }, " + 
                                "\"summary\": \"ISSUETITLE\", " +
                                "\"description\": \"ISSUEDESCRIPTION\", " +
                                "\"issuetype\": { \"name\": \"ISSUETYPE\" } " +
                            "} " +
                       "}";

            json = json.Replace("PROJECTKEY", _options.ProjectKey);
            json = json.Replace("ISSUEASSIGNEE", string.IsNullOrWhiteSpace(_options.AssigneeUserName) ? _options.UserName : _options.AssigneeUserName);
            json = json.Replace("ISSUETITLE", _options.IssueTitle);
            json = json.Replace("ISSUEDESCRIPTION", _options.IssueDescription);
            json = json.Replace("ISSUETYPE", string.IsNullOrWhiteSpace(_options.IssueType) ? "Bug" : _options.IssueType);

            var organizationJiraUrl = string.IsNullOrWhiteSpace(_options.OrganizationJiraUrl)
                                                ? ConfigurationManager.AppSettings["OrganizationJiraUrl"]
                                                : _options.OrganizationJiraUrl;

            string jiraApiUrl;

            if (string.IsNullOrWhiteSpace(organizationJiraUrl))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the Jira Organization Url. You can specify it as a command line parameter or in the configuration file for this program. Please see --help for more details.");
                Console.ForegroundColor = _originalColor;
                return;
            }
            else
            {
                jiraApiUrl = $"{organizationJiraUrl}/rest/api/2/issue";
            }

            try
            {
                var response = MakeJiraRequest(jiraApiUrl, _options.UserName, _options.Password, json);
                ProcessJiraResponse(response);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to make a Jira request: {0}", ex.Message);
                Console.ForegroundColor = _originalColor;
            }
        }

        private void ProcessJiraResponse(string response)
        {
            // The response is something like this
            // "{"id":"13932","key":"DOT-1166","self":"https://dotalign.atlassian.net/rest/api/2/issue/13932"}"
            var match = Regex.Match(response, "key\":\"((\\s|\\S)*)\",\"self\":\"((\\s|\\S)*)\"", RegexOptions.IgnoreCase);

            var group = match.Groups[1];

            var issueKey = @group.Value;

            if (!string.IsNullOrWhiteSpace(issueKey))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Created issue: {issueKey}");
                Console.WriteLine($"Creating branch {issueKey} in repository {_options.RepositoryDirectory}");
                Console.ForegroundColor = _originalColor;

                var processInfo = new ProcessStartInfo
                {
                    FileName = "git.exe",
                    WorkingDirectory = _options.RepositoryDirectory,
                    Arguments = $"checkout -b {issueKey}"
                };

                var process = new Process {StartInfo = processInfo};

                process.Start();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not create a Jira issue. This response was not expected: {0}", response);
                Console.ForegroundColor = _originalColor;
            }
        }

        private void CurlJira(string jiraApiUrl, Options options, string data)
        {
            var dataFileFullName = Path.GetTempFileName();
            File.WriteAllText(dataFileFullName, data);

            var curlArgs = $"-D- -u {options.UserName}:{options.Password} -X POST -d @{dataFileFullName} -H \"Content-Type: application/json\" {jiraApiUrl}";

            // Run the curl command
            var process = new Process();

            var startInfo = new ProcessStartInfo
                                    {
                                        FileName = "curl.exe",
                                        Arguments = curlArgs,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true,
                                        RedirectStandardError = true
                                    };

            process.StartInfo = startInfo;
            process.Start();

            while (!process.HasExited)
            {
                Console.WriteLine(process.StandardOutput.ReadLine());
            }

            if (0 != process.ExitCode)
            {
                Console.WriteLine("Could not create issue with Jira");
            }
        }

        private string MakeJiraRequest(string url, string userName, string password, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var base64Credentials = GetEncodedCredentials(userName, password);
            request.Headers.Add("Authorization", "Basic " + base64Credentials);

            Console.WriteLine("Making request to Jira url: {0}", url);
            var response = request.GetResponse();

            string text;

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            return text;
        }

        private string GetEncodedCredentials(string userName, string password)
        {
            string mergedCredentials = $"{userName}:{password}";
            var byteCredentials = Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }
    }
}
