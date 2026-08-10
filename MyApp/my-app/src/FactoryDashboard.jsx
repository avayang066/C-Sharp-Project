import React, { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import PageLinks from "./CommonPage";
import { getValidToken } from "./User";
import "./factory-dashboard.css";

const Table = ({ columns, data }) => (
  <div className="fd-table-wrapper">
    <table className="fd-table">
      <thead>
        <tr>
          {columns.map((col) => (
            <th key={col}>{col}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data && data.length > 0 ? (
          data.map((row, i) => (
            <tr key={i}>
              {Object.keys(row).map((key) => (
                <td key={key}>{row[key]}</td>
              ))}
            </tr>
          ))
        ) : (
          <tr>
            <td colSpan={columns.length} className="fd-no-data">
              無資料
            </td>
          </tr>
        )}
      </tbody>
    </table>
  </div>
);

const FactoryDashboard = () => {
  // ----------------------------------------
  // 狀態管理區
  // ----------------------------------------
  const [machines, setMachines] = useState([]);
  const [alarms, setAlarms] = useState([]);
  const [logs, setLogs] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;
  const [showForm, setShowForm] = useState(false);
  const [newMachine, setNewMachine] = useState({
    machineCode: "",
    machineName: "",
    isActive: true,
  });
  const [kpiTotalOutput, setKpiTotalOutput] = useState("-");
  const [kpiAverageYieldRate, setKpiAverageYieldRate] = useState("-");
  // 是否持有有效 token。為 false 時停止所有輪詢並導回登入頁
  const [isAuthed, setIsAuthed] = useState(() => !!getValidToken());


  // ----------------------------------------
  // 欄位定義區
  // ----------------------------------------
  const machineCols = ["機台編號", "機台代號", "機台名稱", "啟用狀態", "操作"];
  const alarmCols = ["類型", "訊息", "時間"];
  const logsCols = ["機台代號", "狀態", "良率", "產量", "產出時間"];

  // ----------------------------------------
  // API 與資料操作區
  // ----------------------------------------
  // 共用 fetchWithAuth 函式
  // token 不存在／已過期，或後端回 401 時，標記為未授權（畫面會導回 /user）。
  // 未授權時回傳一個 401 Response，讓呼叫端統一用 res.ok 判斷即可。
  const fetchWithAuth = async (url, options = {}) => {
    const token = getValidToken();
    if (!token) {
      setIsAuthed(false);
      return new Response(null, { status: 401 });
    }

    const headers = {
      ...(options.headers || {}),
      Authorization: "Bearer " + token
    };
    const res = await fetch(url, { ...options, headers });
    if (res.status === 401) {
      localStorage.removeItem("token");
      setIsAuthed(false);
    }
    return res;
  };

  // 機台啟用/停用
  const handleToggleActive = async (machine) => {
    try {
      const res = await fetchWithAuth(`/api/machine/${machine.id}/toggle`, {
        method: "PUT"
      });
      if (res.ok) {
        fetchMachines();
      } else {
        const errorData = await res.json();
        alert(errorData.message || "更新狀態失敗");
      }
    } catch (e) {
      console.error("Toggle error:", e);
      alert("連線伺服器失敗");
    }
  };

  // 抓取 KPI 以及平均良率
  const fetchKpiAndAvarage = async () => {
    try {
      const res1 = await fetchWithAuth("/api/productionlog/kpi-total-output");
      if (!res1.ok) return;
      setKpiTotalOutput(await res1.json());

      const res2 = await fetchWithAuth("/api/productionlog/kpi-average-yieldrate");
      if (!res2.ok) return;
      const avg = await res2.json();
      setKpiAverageYieldRate(avg.toFixed(2));
    } catch (e) {
      setKpiTotalOutput("-");
      setKpiAverageYieldRate("-");
    }
  };

  // 抓取機台資料
  const fetchMachines = async () => {
    try {
      const res = await fetchWithAuth("/api/machine");
      if (!res.ok) return;
      setMachines(await res.json());
    } catch (e) {}
  };

  // 抓取警報資料
  const fetchAlarms = async () => {
    try {
      const res = await fetchWithAuth("/api/machine/alarms/10");
      if (!res.ok) return;
      setAlarms(await res.json());
    } catch (e) {}
  };

  // 抓取產出資料
  const fetchLogs = async (page = 1) => {
    try {
      const pageNum = page || 1;
      const res = await fetchWithAuth(`/api/productionlog?page=${pageNum}&pageSize=10`);
      if (!res.ok) {
        // 401 等錯誤回應可能沒有 body，不能直接 res.json()
        console.error("取得生產資料失敗:", res.status, await res.text());
        return;
      }
      const data = await res.json();
      setLogs(data);
    } catch (e) {
      console.error("網路錯誤:", e);
    }
  };

  // 新增機台
  const handleAddMachine = async (e) => {
    e.preventDefault();
    try {
      const res = await fetchWithAuth("/api/machine", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newMachine)
      });
      if (res.ok) {
        setShowForm(false);
        setNewMachine({ machineCode: "", machineName: "", isActive: true });
        fetchMachines();
      }
    } catch (e) {
      alert("新增失敗");
    }
  };

  // 匯出產出資料
  const exportLogs = async () => {
    try {
      const res = await fetchWithAuth("/api/productionlog/export");
      if (!res.ok) {
        alert("匯出失敗");
        return;
      }
      const blob = await res.blob();
      const disposition = res.headers.get("Content-Disposition");
      let fileName = "export.xlsx";
      if (disposition) {
        const match = disposition.match(/filename\*=UTF-8''([^;\n]+)/);
        if (match) {
          let raw = match[1];
          const semiIdx = raw.indexOf(";");
          if (semiIdx !== -1) raw = raw.substring(0, semiIdx);
          fileName = decodeURIComponent(raw);
        } else if (disposition.includes("filename=")) {
          let raw = disposition.split("filename=")[1];
          const semiIdx = raw.indexOf(";");
          if (semiIdx !== -1) raw = raw.substring(0, semiIdx);
          fileName = decodeURIComponent(raw.replace(/\"/g, ""));
        }
        if (!fileName.endsWith(".xlsx")) fileName += ".xlsx";
      }
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch (e) {
      alert("匯出失敗");
    }
  };

  // ----------------------------------------
  // 資料渲染區
  // ----------------------------------------
  const renderMachines = machines.map((m) => ({
    Id: m.id,
    編號: m.machineCode,
    機台名稱: m.machineName,
    啟用狀態: (
      <span className={m.isActive ? "fd-icon-active" : "fd-icon-inactive"}>
        {m.isActive ? "✔" : "✖"}
      </span>
    ),
    操作: (
      <button
        className={m.isActive ? "fd-btn-stop" : "fd-btn-start"}
        onClick={() => handleToggleActive(m)}
      >
        {m.isActive ? "停用" : "啟用"}
      </button>
    ),
  }));

  const renderAlarms = alarms.slice(0, 50).map((a) => ({
    類型: a.alarmType,
    訊息: a.message,
    時間: new Date(a.createdAt).toLocaleString(),
  }));

  const renderLogs = Array.isArray(logs)
    ? logs.map((l) => ({
        機台代號: l.machineCode,
        狀態: (
          <span
            className={
              l.status === "Success" ? "fd-status-success" : "fd-status-error"
            }
          >
            {l.status}
          </span>
        ),
        YieldRate: `${(l.yieldRate * 100).toFixed(1)}%`,
        OutputQty: l.outputQty,
        Timestamp: new Date(l.timestamp).toLocaleString(),
      }))
    : [];

  // ----------------------------------------
  // 自動刷新區
  // ----------------------------------------
  // 未授權時不發任何請求，也不啟動輪詢（否則會每 10 秒重打一輪 401）
  useEffect(() => {
    if (!isAuthed) return;
    fetchLogs(currentPage);
    const timer = setInterval(() => {
      fetchLogs(currentPage);
    }, 10000);
    return () => clearInterval(timer);
  }, [currentPage, isAuthed]);

  useEffect(() => {
    if (!isAuthed) return;
    fetchMachines();
    const timer = setInterval(fetchMachines, 30000);
    return () => clearInterval(timer);
  }, [isAuthed]);

  useEffect(() => {
    if (!isAuthed) return;
    fetchAlarms();
    const timer = setInterval(fetchAlarms, 10000);
    return () => clearInterval(timer);
  }, [isAuthed]);

  useEffect(() => {
    if (!isAuthed) return;
    fetchKpiAndAvarage();
    const timer = setInterval(fetchKpiAndAvarage, 10000);
    return () => clearInterval(timer);
  }, [isAuthed]);

  // ----------------------------------------
  // HTML
  // ----------------------------------------
  // 沒有有效 token（未登入或 token 已過期）就導回登入頁
  if (!isAuthed) {
    return <Navigate to="/user" replace />;
  }

  return (
    <div className="factory-dashboard-container">
      {/* 分頁連結區塊 */}
      <PageLinks />

      {/* KPI和良率卡片 */}
      <div className="fd-kpi-row">
        <div className="fd-kpi-card">
          <div className="fd-kpi-label">近一個月總產量</div>
          <div className="fd-kpi-value">{kpiTotalOutput}</div>
        </div>
        <div className="fd-kpi-card">
          <div className="fd-kpi-label">近一個月平均良率</div>
          <div className="fd-kpi-value">{kpiAverageYieldRate}%</div>
        </div>
      </div>

      <div className="fd-header-row">
        <h2 className="fd-title" style={{ marginTop: 0 }}>
          機台資訊
        </h2>
        <button className="fd-add-btn" onClick={() => setShowForm(!showForm)}>
          {showForm ? "取消新增" : "+ 新增機台"}
        </button>
        <button
          className="fd-export-btn"
          style={{ marginLeft: 8 }}
          onClick={exportLogs}
        >
          匯出
        </button>
      </div>

      {/* 新增機台表單區塊 */}
      {showForm && (
        <form className="fd-add-form" onSubmit={handleAddMachine}>
          <div className="fd-input-group">
            <input
              placeholder="機台代號 (如: M001)"
              value={newMachine.machineCode}
              onChange={(e) =>
                setNewMachine({ ...newMachine, machineCode: e.target.value })
              }
              required
            />
            <input
              placeholder="機台名稱 (如: 沖壓機)"
              value={newMachine.machineName}
              onChange={(e) =>
                setNewMachine({ ...newMachine, machineName: e.target.value })
              }
              required
            />
            <label className="fd-checkbox-label">
              <input
                type="checkbox"
                checked={newMachine.isActive}
                onChange={(e) =>
                  setNewMachine({ ...newMachine, isActive: e.target.checked })
                }
              />{" "}
              啟用
            </label>
            <button type="submit" className="fd-submit-btn">
              儲存
            </button>
          </div>
        </form>
      )}

      <Table columns={machineCols} data={renderMachines} />

      <h2 className="fd-title">最新警報</h2>
      <Table columns={alarmCols} data={renderAlarms} />

      <h2 className="fd-title">產出資料</h2>
      <div className="fd-table-container-with-pager">
        {/* Table 內部已有 .fd-table-wrapper 負責橫向捲動，不再外包一層 */}
        <Table columns={logsCols} data={renderLogs} />

        {/* --- 整合型分頁條 --- */}
        <div className="fd-pagination-footer">
          <button
            className="fd-page-btn-sm"
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
          >
            ←
          </button>

          <span className="fd-page-info-sm">第 {currentPage} 頁</span>

          <button
            className="fd-page-btn-sm"
            disabled={logs.length < pageSize}
            onClick={() => setCurrentPage((prev) => prev + 1)}
          >
            →
          </button>
        </div>
      </div>
    </div>
  );
};

export default FactoryDashboard;
