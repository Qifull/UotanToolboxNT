﻿using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class MagiskHelper
    {
        public class PatchPlan
        {
            public string? MAGISK_VER { get; set; }
            public string? MAGISK_VER_CODE { get; set; }
            public bool IsVivoSuuPatch { get; set; }
        }
        public static bool Magisk_Validation(string MD5_in, string MAGISK_VER)
        {
            string MD5_out = null;
            string MD5;
            Dictionary<string, string> patchPlans = new Dictionary<string, string>
        {
            {"27.0" , "3b324a47607ae17ac0376c19043bb7b1"},
            {"26.4" , "3b324a47607ae17ac0376c19043bb7b1"},
            {"26.3" , "3b324a47607ae17ac0376c19043bb7b1"}
             /*下面的支持还没写，你要是看到这段文字可以考虑一下帮我写写然后PR到仓库。 -zicai
            {"26.2" , "daf3cffe200d4e492edd0ca3c676f07f"},
            {"26.1" , "0e8255080363ee0f895105cdc3dfa419"},
            {"26.0" , "3d2c5bcc43373eb17939f0592b2b40f9"},
            {"25.2" , "bf6ef4d02c48875ae3929d26899a868d"},
            {"25.1" , "c48a22c8ed43cd20fe406acccc600308"},
            {"25.0" , "7b40f9efd587b59bade9b9ec892e875e"},
            {"22.1" , "55285c3ad04cdf72e6e2be9d7ba4a333"}
             */
        };
            if (patchPlans.TryGetValue(MAGISK_VER, out MD5_out))
            {
                if (MD5_out == MD5_in)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到有效的" + MAGISK_VER + "面具安装包"));
                    return true;
                }
                SukiHost.ShowDialog(new ConnectionDialog("面具安装包可能失效，继续修补存在风险"));
                return false;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("面具安装包不被支持"));
                return false;
            }
        }
        public static bool CheckComponentFiles(string magisk_path, string arch)
        {
            string compPathBase = System.IO.Path.Combine(magisk_path, "lib");
            string archSubfolder = arch switch
            {
                "aarch64" => "arm64-v8a",
                "armeabi" => "armeabi-v7a",
                "X86" => "x86",
                "X86-64" => "x86_64",
                _ => throw new ArgumentException($"未知架构：{arch}")
            };
            string compPath = System.IO.Path.Combine(compPathBase, archSubfolder);
            string[] commonFiles = { "libmagiskpolicy.so", "libmagiskinit.so", "libmagiskboot.so", "libbusybox.so" };
            string specificFiles = arch switch
            {
                "aarch64" => "libmagisk64.so",
                "armeabi" => "libmagisk32.so",
                "X86" => "libmagisk32.so",
                "X86-64" => "libmagisk64.so",
                _ => throw new InvalidOperationException() // 前面不出问题，这个东西应该不会被抛出
            };
            commonFiles = commonFiles.Concat(new[] { specificFiles }).ToArray();
            var results = FileHelper.CheckFilesExistInDirectory(compPath, commonFiles);
            bool allFilesExist = results.Values.All(result => result);
            return allFilesExist;
        }
        public static bool boot_img_pre(string boot_path)
        {
            try
            {
                File.Copy(boot_path, System.IO.Path.Combine(Global.boot_tmp, "stock_boot.img"), true);
                File.Copy(System.IO.Path.Combine(Global.boot_tmp, "ramdisk.cpio"), System.IO.Path.Combine(Global.boot_tmp, "ramdisk.cpio.orig"), true);
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式0预处理出错 {e.Message}"));
                return false;
            }
        }
        public static bool patched_img_pre(string boot_path)
        {
            try
            {
                File.Copy(System.IO.Path.Combine(Global.boot_tmp, "ramdisk", ".backup", ".magisk", "config.orig"), System.IO.Path.Combine(Global.boot_tmp, "config.orig"));
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式1预处理出错 {e.Message}"));
                return false;
            }
        }
        public static bool comp_copy(string compPath)
        {
            try
            {
                File.Copy(System.IO.Path.Combine(Global.magisk_tmp, "assets", "stub.xz"), System.IO.Path.Combine(Global.boot_tmp, "stub.xz"), true);
                File.Copy(System.IO.Path.Combine(compPath, "libmagiskinit.so"), System.IO.Path.Combine(Global.boot_tmp, "magiskinit"), true);
                File.Copy(System.IO.Path.Combine(compPath, "magisk32.xz"), System.IO.Path.Combine(Global.boot_tmp, "magisk32.xz"), true);
                if (File.Exists(System.IO.Path.Combine((compPath), "magisk64.xz")))
                {
                    File.Copy(System.IO.Path.Combine(compPath, "magisk64.xz"), System.IO.Path.Combine(Global.boot_tmp, "magisk64.xz"), true);
                }
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog("Failed to copy components to boot partition" + e));
                return false;
            }
        }
        public static string dtb_detect(string path)
        {
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "dtb")))
            {
                return "dtb";
            }
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "kernel_dtb")))
            {
                return "kernel_dtb";
            }
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "extra")))
            {
                return "extra";
            }
            return null;
        }
        public static bool CleanBoot(string path)
        {
            string[] filesToDelete =
                {
                "magisk64.xz",
                "magisk32.xz",
                "magiskinit",
                "stub.xz",
                "ramdisk.cpio.orig",
                "config",
                "stock_boot.img",
                "cpio",
                "init",
                "init.xz",
                ".magisk",
                ".rmlist"
                };
            try
            {
                foreach (string file in filesToDelete)

                {
                    string filePath = System.IO.Path.Combine(path, file);
                    if (File.Exists(filePath))
                    {
                        FileHelper.WipeFile(filePath);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        }
    }