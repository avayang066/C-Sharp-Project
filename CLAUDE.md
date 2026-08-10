# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案概述

工廠生產監控系統：ASP.NET Core 8 Web API（`MyApp/`）+ React 19 SPA（`MyApp/my-app/`）+ xUnit 測試（`MyApp.Tests/`）。
一個背景服務持續產生模擬產線資料（生產日誌、良率警報），前端儀表板讀取這些資料。註解與 UI 文字為繁體中文。

## 常用指令

```powershell
# 後端（在 MyApp/ 下）
dotnet run                      # http://localhost:5000 / https://localhost:5001，自動開 /swagger
dotnet build                    # 或在 repo 根目錄 dotnet build FirstDotNet.sln
dotnet format                   # CSharpier 風格已套用於現有程式碼

# 前端（務必在 MyApp/my-app/ 下，不是 MyApp/）
cd MyApp\my-app
npm start                       # http://localhost:3000，透過 package.json 的 proxy 轉發 /api 到 :5000
npm run build
# MyApp/package.json 沒有 scripts，在 MyApp/ 下跑 npm start 會噴 Missing script: "start"

# 測試（repo 根目錄）
dotnet test
dotnet test --filter "FullyQualifiedName~MachineServiceTests.GetAllMachinesAsync_ReturnsAllMachines"
dotnet test --filter "FullyQualifiedName~MachineServiceTests"

# EF Core migration（在 MyApp/ 下）—— 注意下方「資料庫雙重設定」陷阱
dotnet ef migrations add <Name>
dotnet ef database update
```

後端必須跑在 5000 埠，否則前端 proxy 失效。

## 架構要點

**分層**：Controller → `I*Service` 介面 → Service 實作 → `ApplicationDbContext`。
所有 service 在 [Program.cs](MyApp/Program.cs) 以 `AddScoped` 註冊；例外是 `ProductionLogController` 同時注入 service 與 DbContext。

**背景資料產生器** [Services/BackgroundService.cs](MyApp/Services/BackgroundService.cs)（`ProductionLogGeneratorService`，`AddHostedService` 註冊）每 30 秒一輪：
1. 從 `IsActive` 機台中隨機挑一台，產生一筆 `ProductionLog`（良率 > 0.90 → `Success`，< 0.80 → `Error`，其餘 `Normal`）
2. 良率 < 0.80 時附帶產生一筆 `AlarmEvent`
3. 每 10 輪清理一次舊資料：每台機台只保留最新 100 筆 `ProductionLog`（連同其 `AlarmEvent`）

所以資料庫是刻意保持小量的滾動視窗——歷史資料會消失是預期行為，不是 bug。沒有任何 Active 機台時不會產生資料。

**認證**：`UserController` 全開放（register/login/logout），`MachineController` 與 `ProductionLogController` 掛 `[Authorize]`。
JWT 由 [UserService.cs](MyApp/Services/UserService.cs) 簽發（claims：NameIdentifier、Name），前端存在 `localStorage.token`，
由 [FactoryDashboard.jsx](MyApp/my-app/src/FactoryDashboard.jsx) 內的 fetch 包裝統一加上 `Authorization: Bearer`。
`logout` 是純前端行為（清 localStorage），server 端不維護 token 黑名單。

**前端路由** [index.js](MyApp/my-app/src/index.js) 只有 `/dashboard` 與 `/user` 兩條。
但共用導覽 [CommonPage.jsx](MyApp/my-app/src/CommonPage.jsx) 列了五個連結，`/alarm`、`/machine`、`/statistic` 目前是死連結（尚未實作的頁面）。

**並行控制**：`MachineService` 用 `static SemaphoreSlim(5)` 限制新增/切換狀態的同時進入數，這是刻意的設計而非殘留碼。

## 陷阱與已知不一致

- **資料庫雙重設定**：[Program.cs](MyApp/Program.cs) 的 DI 指向 **SQL Server**（連線字串寫死：`localhost\SQLEXPRESS`、`Database=MyAppDB`），但 [ApplicationDbContext.OnConfiguring](MyApp/Data/ApplicationDbContext.cs) 的 fallback 是 **SQLite**（`MyAppDB.db`）。`dotnet ef` CLI 走無參數建構函式，會落到 SQLite 分支——改 schema 前務必確認實際對到哪個資料庫。repo 內的 `MyApp/MyAppDB.db` 是舊 SQLite 殘留。
- 啟動時會 `db.Database.CanConnect()`，連不上直接 throw，所以本機沒有 SQLEXPRESS 就跑不起來。
- KPI 端點命名與行為不符：`kpi-total-output` 註解寫「近一個月」，實作是**今日**總產量；`kpi-average-yieldrate` 才是近 30 天。
- Serilog 硬編碼 `MinimumLevel.Warning()`（[Program.cs](MyApp/Program.cs) 開頭），寫入 `MyApp/Logs/app<date>.log`；appsettings 的 Logging 區塊不影響它。想看 Information 需改該行（檔內有註解掉的替代版本）。
- `appsettings.json` 內含開發用 `jwtKey`；未設定時 Program.cs 會在啟動時產生亂數金鑰（重啟即失效所有 token）。
- CORS 政策 `AllowAll`（任何來源/方法/header）。
- `MyApp/package.json`（React 依賴、react-router-dom v6）與真正的前端 `MyApp/my-app/package.json`（v7）重複且版本不一致；前端只用後者。`MyApp/my-app/src/App.js` 是未使用的 CRA 樣板。
- **`MyApp/Services/TakeInventory/`**（`.cs` 與 `.py` 各一份）命名空間為 `Hsihung.Api.Inventory`，是另一個系統的盤點功能移植稿：無 controller、無 DI 註冊、全專案零引用。不屬於執行中的程式，改動 MyApp 時不需要考慮它。
- 另有一份舊 MVC 版儀表板 [Views/Home/FactoryDashboard.cshtml](MyApp/Views/Home/FactoryDashboard.cshtml) + `wwwroot/css/`，`/` 會 redirect 到它。新功能請做在 React 端。

## 慣例

- Commit 訊息為繁體中文，格式如 `feat: 新增使用者登入與註冊頁面；整合頁面連結元件`（分號分隔多項）。
- 測試用 `UseInMemoryDatabase`，且多個測試共用固定 `databaseName: "TestDb"`——新增測試時注意狀態互相污染。
