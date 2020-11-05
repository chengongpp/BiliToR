using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BiliToPNG
{
    internal class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("**主人好，欢迎使用B漫图片提取器0.01版");
            Console.WriteLine("**目前仅支持转换到webp格式。webp格式可以用画图或浏览器打开");
            Console.WriteLine("**B漫的漫画文件，离线缓存存储在安卓手机SD卡目录下的");
            Console.WriteLine("**data/bilibili/comic/down文件夹。");
            Console.WriteLine("**基本上是对应了\"漫画/话数/内容.jpg.view\"的形式来保存的。");
            Console.WriteLine("**所以请先将down文件夹下的东西拷到电脑上来吧！");
            while (true)
            {
                Console.WriteLine("**********************************************");
                Console.WriteLine("接下来，请把Bilibili漫画的.jpg.view文件，或包含了.view文件的文件夹，输入或拖入这个窗口，按下回车确认*");
                Console.WriteLine("将会在相同位置自动创建\"bmoutput-[日期]-\"格式的文件/文件夹");
                Console.WriteLine("并按照最后修改时间顺序重命名。如果有人知道怎么看出正经的页顺序，请在合适的场所探讨，不要联系作者）");
                string fromPath = Console.ReadLine();


                FileAttributes attr;
                try
                {
                    attr = File.GetAttributes(fromPath ?? throw new FileNotFoundException("找不到文件，请不要联系作者"));
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine("识别到{0}是文件夹", fromPath);

                    DateTime today = DateTime.Today;
                    DirectoryInfo fromDirectoryInfo = new DirectoryInfo(fromPath);
                    string directoryPath =
                        String.Format("{0}\\bmoutput-{1}{2}{3}", fromDirectoryInfo.Parent.FullName, today.Year, today.Month, today.Day);
                    if (!File.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    /*递归进行转换*/
                    ConvertDirectory(fromDirectoryInfo, new DirectoryInfo(directoryPath));
                    Console.WriteLine("全部转换成功！关闭窗口退出，按任意键进行下一轮转换");
                }
                else
                {
                    Console.WriteLine("识别到{0}是Bilibili漫画文件", fromPath);
                    FileInfo fromFileInfo = new FileInfo(fromPath);
                    var saveDirectoryName = fromFileInfo.DirectoryName;
                    DateTime today = DateTime.Today;
                    string savePath =
                        String.Format("{0}\\bmoutput-{1}{2}{3}.webp", saveDirectoryName, today.Year, today.Month,
                            today.Day);
                    Convert(fromFileInfo, new FileInfo(savePath));
                    Console.WriteLine("全部转换成功！关闭窗口退出，按任意键进行下一轮转换");
                }
                Console.ReadKey();
            }
        }
        
        public static void Convert(FileInfo srcFileInfo, FileInfo dstFileInfo)
        {
            Console.WriteLine("从这里{0}", srcFileInfo.FullName);
            Console.WriteLine("转换到{0}", dstFileInfo.FullName);
            byte[] buf = File.ReadAllBytes(srcFileInfo.FullName);
            byte[] cut = buf.Skip(9).ToArray();
            File.WriteAllBytes(dstFileInfo.FullName, cut);
            Console.WriteLine("转换完1个。");
        }

        public static void ConvertDirectory(DirectoryInfo srcDirectoryInfo, DirectoryInfo dstDirectoryInfo)
        {
            DirectoryInfo[] srcSubDirectories = srcDirectoryInfo.GetDirectories();
            DirectoryInfo[] dstSubDirectories = dstDirectoryInfo.GetDirectories();
            if (srcSubDirectories.Length != 0)
            {
                // 如不存在则创建同名文件夹
                var dstSubDirectoryNames = dstSubDirectories.Select(x => x.Name).ToList();
                foreach (var srcSubDirectoryInfo in srcSubDirectories)
                {
                    DirectoryInfo dstSubDirectoryInfo;
                    if (!dstSubDirectoryNames.Contains(srcSubDirectoryInfo.Name))
                    {
                        dstSubDirectoryInfo = dstDirectoryInfo.CreateSubdirectory(srcSubDirectoryInfo.Name);
                    }
                    else
                    {
                        dstSubDirectoryInfo = new DirectoryInfo($"{dstDirectoryInfo.FullName}\\{srcSubDirectoryInfo.Name}");
                    }
                    ConvertDirectory(srcSubDirectoryInfo, dstSubDirectoryInfo);
                }
            }

            var srcImgFiles = srcDirectoryInfo.GetFiles("*.jpg.view").OrderBy(x => x.CreationTime).ToList();
            Console.WriteLine("找到以下文件：");

            Parallel.ForEach(srcImgFiles,
                (srcImgFile, state, index) =>
                    Convert(srcImgFile, new FileInfo($"{dstDirectoryInfo.FullName}\\{index:000}.webp")));
        }
    }
}