# Scene Setup Checklist

## 1) MainMenu 场景
- Canvas
  - Button(Start) -> 绑定 `MainMenuController.startButton`
  - Button(Quit) -> 绑定 `MainMenuController.quitButton`
- 空对象 `MainMenuUI` 挂载 `MainMenuController`

## 2) Level_01 场景
- Camera(Main Camera)
- 背景对象 `BG_Starfield`
  - 挂 `ScrollingBackground`，绑定 Renderer
- 玩家对象 `Player`
  - `SpriteRenderer`, `Rigidbody2D(Kinematic)`, `Collider2D(isTrigger=true)`
  - `HealthComponent`
    - faction=Player
    - notifyPlayerHpEvents=true
    - notifyDeathAsPlayer=true
  - `KeyboardMouseInputProvider`
  - `PlayerController` (绑 PlayerConfig / InputProvider / WeaponController)
  - `WeaponController`
    - ownerFaction=Player
    - projectilePrefab=玩家子弹
    - firePoints=玩家炮口
- 刷怪对象 `Spawner`
  - `EnemySpawner` (绑 WaveConfig / SpawnPoints / BossPrefab / BossSpawnPoint)
- Canvas(HUD)
  - Text(score)
  - Text(hp)
  - Slider(boss hp)
  - PausePanel
  - `HUDController` 绑定以上引用

## 3) 敌机 Prefab
- `SpriteRenderer`, `Rigidbody2D(Kinematic)`, `Collider2D(isTrigger=true)`
- `HealthComponent` (faction=Enemy)
- `EnemyController` (绑 EnemyConfig)
- `RamDamage` (collisionDamage=1, faction=Enemy)
- 可选：`WeaponController` + 敌机 firePoint + 敌机子弹 prefab

## 4) Boss Prefab
- `SpriteRenderer`, `Rigidbody2D(Kinematic)`, `Collider2D(isTrigger=true)`
- `HealthComponent`
  - faction=Enemy
  - notifyBossHpEvents=true
  - notifyDeathAsBoss=true
- `BossController`
  - phases=3个 BossPhaseConfig（建议阈值 1.0 / 0.7 / 0.4）
  - projectilePrefab=Boss子弹
  - firePoint=Boss炮口

## 5) ProjectilePool
- 运行时会自动创建 `[Core] ProjectilePool`。
- 推荐在 `ProjectilePool` 的 entries 添加：
  - 玩家子弹 prefab, prewarm 120
  - 敌机子弹 prefab, prewarm 120
  - Boss子弹 prefab, prewarm 200

## 6) Result 场景
- Canvas
  - Text(title)
  - Text(score)
  - Button(Retry)
  - Button(Menu)
- 空对象 `ResultUI` 挂 `ResultController` 并绑定引用

## 7) 配置资产 (ScriptableObject)
创建以下资产并绑定到脚本：
- PlayerConfig
- EnemyConfig（基础/精英可各建一份）
- WeaponConfig（玩家/敌机）
- WaveConfig
- BossPhaseConfig (x3)

## 8) Build Settings
- Scenes In Build 顺序建议：
  1. MainMenu
  2. Level_01
  3. Result
