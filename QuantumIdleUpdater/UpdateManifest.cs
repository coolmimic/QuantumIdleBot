using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleUpdater
{
    // ==========================================
    // 1. 数据模型 (放在同一个文件方便复制)
    // ==========================================
    public class UpdateManifest
    {
        public string Version { get; set; }
        public string BaseUrl { get; set; } // 文件下载基础URL
        public List<FileItem> Files { get; set; }
    }

    public class FileItem
    {
        public string Path { get; set; }       // 相对路径 "data/config.json"
        public bool SkipIfExists { get; set; } // 核心属性：是否跳过已存在的文件
    }
}
