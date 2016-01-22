# CIC
Create a Jira Issue and a branch with the same name in the specified local repository

To get the application, you can build the code yourself, or from any release, download `cic.exe`, `commandline.dll` and (optionally) `cic.exe.config` and put them in some folder on your computer, preferably somewhere in the path.

Then when you run it without arguments or with the `--help` argument, you will be able to see how to run it. 

As an example, 

    cic -u jaspreet -p <Password> -k DS -r c:\code\myrepo -t "Focus being lost on click" -j http://dot.atlassian.net
