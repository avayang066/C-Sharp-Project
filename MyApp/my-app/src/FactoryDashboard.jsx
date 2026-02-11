import React, { useEffect, useState } from "react";
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
  const [machines, setMachines] = useState([]);
  const [alarms, setAlarms] = useState([]);
  const [logs, setLogs] = useState([]);

  // --- 新增分頁狀態 ---
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // 表單控制與資料狀態
  const [showForm, setShowForm] = useState(false);
  const [newMachine, setNewMachine] = useState({
    machineCode: "",
    machineName: "",
    isActive: true,
  });

  // 定義欄位名稱
  const machineCols = ["機台編號", "機台代號", "機台名稱", "啟用狀態", "操作"];
  const alarmCols = ["類型", "訊息", "時間"];
  const logsCols = ["機台編號", "狀態", "良率", "產量", "產出時間"];

  const handleToggleActive = async (machine) => {
    try {
      // 簡化參數：因為後端 ToggleMachineStatusAsync 不需要 Body，所以也拿掉 headers 和 body
      const res = await fetch(`/api/machine/${machine.id}/toggle`, {
        method: "PUT",
      });

      if (res.ok) {
        // 成功後刷新列表
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

  // --- API 抓取函式 ---
  const fetchMachines = async () => {
    try {
      const res = await fetch("/api/machine");
      setMachines(await res.json());
    } catch (e) {}
  };

  const fetchAlarms = async () => {
    try {
      const res = await fetch("/api/machine/alarms/10");
      setAlarms(await res.json());
    } catch (e) {}
  };

  const fetchLogs = async (page = 1) => {
    // 給予預設值 1
    try {
      // 加上防呆：如果 page 是 null 或 undefined，就用 1
      const pageNum = page || 1;
      const res = await fetch(`/api/productionlog?page=${pageNum}&pageSize=20`);

      if (!res.ok) {
        const errorData = await res.json();
        console.error("後端驗證失敗:", errorData);
        return;
      }

      const data = await res.json();
      setLogs(data);
    } catch (e) {
      console.error("網路錯誤:", e);
    }
  };

  // ---  Effects 區塊 ---
  // 1. 當頁碼改變時抓取，並設定定時刷新
  useEffect(() => {
    fetchLogs(currentPage); // 立即抓取

    const timer = setInterval(() => {
      fetchLogs(currentPage);
    }, 10000); // 每 10 秒刷新當前頁面資料

    return () => clearInterval(timer); // 清除舊的計時器
  }, [currentPage]); // 依賴項只留 currentPage

  // 2. 機台列表刷新
  useEffect(() => {
    fetchMachines();
    const timer = setInterval(fetchMachines, 30000);
    return () => clearInterval(timer);
  }, []);

  // 3. 警報列表刷新
  useEffect(() => {
    fetchAlarms();
    const timer = setInterval(fetchAlarms, 10000);
    return () => clearInterval(timer);
  }, []);

  // --- 處理新增機台 ---
  const handleAddMachine = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch("/api/machine", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newMachine),
      });
      if (res.ok) {
        setShowForm(false);
        setNewMachine({ machineCode: "", machineName: "", isActive: true });
        fetchMachines(); // 成功後重新整理列表
      }
    } catch (e) {
      alert("新增失敗");
    }
  };

  // --- 資料美化渲染 (只保留這一份宣告) ---
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
    // Id: a.id,
    類型: a.alarmType,
    訊息: a.message,
    時間: new Date(a.createdAt).toLocaleString(),
  }));

  const renderLogs = Array.isArray(logs)
    ? logs.map((l) => ({
        機台編號: l.machineId,
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
    : []; // 如果不是陣列，就給空陣列，避免 slice 報錯

  return (
    <div className="factory-dashboard-container">
      <div className="fd-header-row">
        <h2 className="fd-title" style={{ marginTop: 0 }}>
          機台資訊
        </h2>
        <button className="fd-add-btn" onClick={() => setShowForm(!showForm)}>
          {showForm ? "取消新增" : "+ 新增機台"}
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
        <div className="fd-table-wrapper" style={{ marginBottom: 0 }}>
          <Table columns={logsCols} data={renderLogs} />
        </div>

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
