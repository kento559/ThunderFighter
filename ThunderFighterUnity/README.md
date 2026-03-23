# ThunderFighterUnity (雷霆战机 MVP)

Unity 2D 竖版卷轴射击游戏脚手架，已实现：
- 状态流程：Boot -> MainMenu -> Playing -> Paused -> GameOver -> Victory
- 玩家移动/射击、敌机波次、Boss 多阶段、计分与结算
- 伤害接口与事件总线：OnScoreChanged / OnPlayerHpChanged / OnGameStateChanged / OnBossHpChanged
- 子弹对象池（含回收逻辑）

## 目录
- `Assets/Scenes`: `MainMenu`, `Level_01`, `Result`
- `Assets/Scripts/Core`: 流程、事件、计分、启动
- `Assets/Scripts/Input`: 输入接口与键鼠实现
- `Assets/Scripts/Combat`: 伤害、血量、武器、子弹、对象池
- `Assets/Scripts/Player`: 玩家控制
- `Assets/Scripts/Enemy`: 敌机行为与碰撞伤害
- `Assets/Scripts/Boss`: Boss 逻辑与阶段配置
- `Assets/Scripts/Spawning`: 波次配置与刷怪器
- `Assets/Scripts/UI`: 主菜单、HUD、结算页
- `Assets/Scripts/Config`: ScriptableObject 配置

## 快速开始
1. 使用 Unity Hub 新建/打开 2D 项目，位置指向本文件夹 `ThunderFighterUnity`。
2. 创建 3 个场景并加入 Build Settings：
   - `MainMenu`
   - `Level_01`
   - `Result`
3. 按 `SCENE_SETUP.md` 挂载对象与脚本。
4. 运行后默认操作：
   - 移动：`WASD` / 方向键
   - 开火：`J` 或鼠标左键
   - 暂停：`Esc`

## 说明
- 美术和音频均按可替换占位策略设计，替换资源无需改脚本接口。
- 当前为 MVP 单关卡，后续可在 WaveConfig 中扩展更多波次与章节。
