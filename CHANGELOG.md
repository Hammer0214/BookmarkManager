# 标签页收藏管理器 - 版本记录

## V0.0.1 (2026-07-04)

### 首次发布

#### 核心功能
- 分类管理：创建、编辑、删除分类，支持自定义图标
- 书签管理：添加、编辑、删除、一键打开、拖拽排序
- 标签系统：创建、删除标签，关联书签，按标签筛选
- 搜索功能：按标题、URL、描述搜索
- 导入/导出：JSON 和 HTML 书签格式

#### 界面特性
- 无边框自定义窗口（可拖拽、最小化、最大化/还原、关闭）
- 深色/浅色主题切换
- 自定义右键菜单样式
- 标题栏按钮悬停效果（最小化灰、最大化蓝、关闭红）
- 书签卡片式列表展示
- 状态栏显示操作结果

#### 技术实现
- C# WPF (.NET 10) + SQLite + Dapper + CommunityToolkit.Mvvm
- MVVM 架构
- WM_NCCALCSIZE 去除窗口边框
- WM_GETMINMAXINFO 最大化边距处理
- 全局异常处理
- 自包含单文件发布

#### 安装程序
- Inno Setup 安装向导
- 支持选择安装路径
- 可创建桌面快捷方式
- 自动注册卸载程序
- 安装完成后可选启动应用

#### Bug 修复
- 修复 FocusSearchCommand 缺失问题
- 修复右键菜单点击后不关闭问题
- 修复编辑分类后 SelectedCategory 引用失效
- 修复拖拽排序位置计算错误
- 修复最大化窗口超出屏幕边距
- 修复导入 JSON 重复数据问题
- 修复 N+1 标签查询性能问题
- 修复搜索空格触发不一致
- 修复 EmptyState 更新时机问题
- 修复 TagFilterIndicator 与 TitleText 重叠
- 修复 Tag 点击后 UpdateTitle 引用错误

### 备份文件
- `backups/V0.0.1/BookmarkManager-Setup-V0.0.1.exe` - 安装程序
- `backups/V0.0.1/BookmarkManager-V0.0.1.exe` - 独立运行版
- `backups/V0.0.1/Source/` - 源代码备份
