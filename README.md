标签页收藏管理器

一个桌面标签页/书签管理工具，帮你把散落各处的网页统一收藏、分类、查找。再也不用担心浏览器书签太多找不到、换浏览器要重新整理。

## 关于

你有没有遇到过这种情况？

- 浏览器收藏夹里攒了几百个链接，想找个之前看过的页面翻半天
- 换了浏览器或电脑，书签全没了
- 想给书签打标签分类，浏览器自带的功能太简陋

**标签页收藏管理器** 就是为解决这些问题而生的。它独立运行在 Windows 上，不依赖任何浏览器，所有数据本地存储，支持分类、标签、搜索、导入导出，让你真正管理好自己的收藏。

## 功能特性

- **分类管理** — 创建、编辑、删除分类，支持自定义图标
- **书签管理** — 添加、编辑、删除、一键在浏览器中打开
- **拖拽排序** — 书签列表支持拖拽重新排序
- **标签系统** — 创建标签、关联书签、按标签筛选
- **搜索功能** — 按标题、URL、描述搜索
- **导入/导出** — 支持 JSON 和 HTML 书签格式
- **深色/浅色主题** — 一键切换
- **键盘快捷键** — Ctrl+N 添加书签，Ctrl+F 搜索等
- **自定义标题栏** — 无边框窗口，支持拖拽和最大化/最小化/关闭

## 技术栈

- C# / .NET 10
- WPF (Windows Presentation Foundation)
- SQLite + Dapper
- CommunityToolkit.Mvvm
- Inno Setup (安装包)

## 系统要求

- Windows 10 或更高版本
- 支持 x64、x86、ARM64 架构

## 构建方式

```powershell
# 发布自包含单文件
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64

# 编译安装包（需要 Inno Setup 6）
& "C:\Program Files\Inno Setup 6\ISCC.exe" installer-arch.iss /DArch=x64 /DRid=win-x64
```

## 安装

从 [Releases](https://github.com/你的用户名/仓库名/releases) 下载对应架构的安装包，双击运行即可。

## 快捷键

| 快捷键 | 功能  |
| --- | --- |
| Ctrl+N | 添加书签 |
| Ctrl+F | 搜索  |
| Ctrl+Shift+N | 新建分类 |
| Ctrl+D | 切换深色模式 |
| Ctrl+E | 导出  |
| Ctrl+I | 导入  |

## 许可证

MIT
