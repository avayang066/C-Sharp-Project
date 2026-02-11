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

  const fetchLogs = async () => {
    try {
      const res = await fetch("/api/productionlog");
      setLogs(await res.json());
    } catch (e) {}
  };

  // --- Effects ---
  useEffect(() => {
    fetchMachines();
    const timer = setInterval(fetchMachines, 30000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    fetchAlarms();
    const timer = setInterval(fetchAlarms, 10000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    fetchLogs();
    const timer = setInterval(fetchLogs, 10000);
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

  const renderLogs = logs.slice(0, 50).map((l) => ({
    機台編號: l.machineId,
    狀態: (
      <span
        className={
          l.status === "Success"
            ? "fd-status-success"
            : l.status === "Error"
              ? "fd-status-error"
              : "fd-status-normal"
        }
      >
        {l.status}
      </span>
    ),
    YieldRate: `${(l.yieldRate * 100).toFixed(1)}%`,
    OutputQty: l.outputQty,
    Timestamp: new Date(l.timestamp).toLocaleString(),
  }));

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
      <Table columns={logsCols} data={renderLogs} />
    </div>
  );
};

export default FactoryDashboard;
