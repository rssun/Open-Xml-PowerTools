﻿/***************************************************************************

Copyright (c) Microsoft Corporation 2012-2015.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

Published at http://OpenXmlDeveloper.org
Resource Center and Documentation: http://openxmldeveloper.org/wiki/w/wiki/powertools-for-open-xml.aspx

Developer: Eric White
Blog: http://www.ericwhite.com
Twitter: @EricWhiteDev
Email: eric@ericwhite.com

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenXmlPowerTools;

namespace OxPt
{
    public class TestUtil
    {
#if NETCOREAPP2_0
        // CoreFx uses an extra subdirectory in the path for generating DLL/exe
        // i.e. bin\Debug\netcoreapp2.0\
        // therefore an extra ../ is required to find the TestFiles dir
        public static DirectoryInfo SourceDir = new DirectoryInfo("../../../../TestFiles/");
#else
        public static DirectoryInfo SourceDir = new DirectoryInfo("../../../TestFiles/");
#endif
        private static bool? s_DeleteTempFiles = null;

        public static bool DeleteTempFiles
        {
            get
            {
                if (s_DeleteTempFiles != null)
                    return (bool)s_DeleteTempFiles;
                FileInfo donotdelete = new FileInfo("donotdelete.txt");
                s_DeleteTempFiles = !donotdelete.Exists;
                return (bool)s_DeleteTempFiles;
            }
        }

        private static DirectoryInfo s_TempDir = null;
        public static DirectoryInfo TempDir
        {
            get
            {
                if (s_TempDir != null)
                    return s_TempDir;
                else
                {
                    var now = DateTime.Now;
                    var tempDirName = String.Format("Test-{0:00}-{1:00}-{2:00}-{3:00}{4:00}{5:00}", now.Year - 2000, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    s_TempDir = new DirectoryInfo(Path.Combine(".", tempDirName));
                    s_TempDir.Create();
                    return s_TempDir;
                }
            }
        }

        public static void NotePad(string str)
        {
            var guidName = Guid.NewGuid().ToString().Replace("-", "") + ".txt";
            var fi = new FileInfo(Path.Combine(TempDir.FullName, guidName));
            File.WriteAllText(fi.FullName, str);
            var notepadExe = new FileInfo(@"C:\Program Files (x86)\Notepad++\notepad++.exe");
            if (!notepadExe.Exists)
                notepadExe = new FileInfo(@"C:\Program Files\Notepad++\notepad++.exe");
            if (!notepadExe.Exists)
                notepadExe = new FileInfo(@"C:\Windows\System32\notepad.exe");
            ExecutableRunner.RunExecutable(notepadExe.FullName, fi.FullName, TempDir.FullName);
        }

        public static void KDiff3(FileInfo oldFi, FileInfo newFi)
        {
            var kdiffExe = new FileInfo(@"C:\Program Files (x86)\KDiff3\kdiff3.exe");
            var result = ExecutableRunner.RunExecutable(kdiffExe.FullName, oldFi.FullName + " " + newFi.FullName, TempDir.FullName);
        }

        public static void Explorer(DirectoryInfo di)
        {
            ProcessStartInfo psi = new ProcessStartInfo("C:/Windows/Explorer.exe", di.FullName);
            Process.Start(psi);
        }

        public static string CreateTestDir(string testId, out DirectoryInfo thisTestTempDir)
        {
            var rootTempDir = TestUtil.TempDir;
            thisTestTempDir = new DirectoryInfo(Path.Combine(rootTempDir.FullName, testId));
            if (thisTestTempDir.Exists)
                Assert.True(false, "Duplicate test id: " + testId);
            else
                thisTestTempDir.Create();
            var tempDirFullName = thisTestTempDir.FullName;
            return tempDirFullName;
        }

        public static void AddToBatchFile(FileInfo testBaselineFi, FileInfo newlyCreatedFi)
        {
            while (true)
            {
                try
                {
                    ////////// CODE TO REPEAT UNTIL SUCCESS //////////
                    var batchFileName = "Copy-Gen-Files-To-TestFiles.bat";
                    var batchFi = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, batchFileName));
                    var batch = "copy " + newlyCreatedFi.FullName + " " + testBaselineFi.FullName + Environment.NewLine;
                    if (batchFi.Exists)
                        File.AppendAllText(batchFi.FullName, batch);
                    else
                        File.WriteAllText(batchFi.FullName, batch);
                    //////////////////////////////////////////////////
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        public static void OpenWindowsExplorer(DirectoryInfo thisTestTempDir)
        {
            while (true)
            {
                try
                {
                    ////////// CODE TO REPEAT UNTIL SUCCESS //////////
                    var semaphorFi = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, "z_ExplorerOpenedSemaphore.txt"));
                    if (!semaphorFi.Exists)
                    {
                        File.WriteAllText(semaphorFi.FullName, "");
                        TestUtil.Explorer(thisTestTempDir);
                    }
                    //////////////////////////////////////////////////
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        public static void NoRef(object o)
        {
            var z = o;
            o = z;
        }
    }
}
