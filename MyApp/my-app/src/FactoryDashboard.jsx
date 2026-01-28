import React, { useEffect, useState } from "react";
import "./factory-dashboard.css";

const Table = ({ columns, data }) => (
  <div className="fd-table-wrapper">
    <table className="fd-table">
      <thead>
        <tr>
          {columns.map(col => (
            <th key={col}>{col}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data && data.length > 0 ? (
          data.map((row, i) => (
            <tr key={i}>
              {Object.keys(row).map(key => (
                <td key={key}>{row[key]}</td>
              ))}
            </tr>
          ))
        ) : (
          <tr>
            <td colSpan={columns.length} className="fd-no-data">無資料</td>
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

  // 資料欄位 & 美化
  const machineCols = ["Id", "編號", "機台名稱", "啟用狀態"];
  const alarmCols = ["Id", "類型", "訊息", "時間"];
  const logsCols = ["Id", "機台編號", "狀態", "YieldRate", "OutputQty", "Timestamp"];

  // 美化呈現 values
  const renderMachines = machines.map(m => ({
    Id: m.id,
    編號: m.machineCode,
    機台名稱: m.machineName,
    啟用狀態: (
      <span
        style={{
          color: m.isActive ? "#27ae60" : "#c0392b",
          fontWeight: 600,
          fontSize: "1.2em",
        }}
      >
        {m.isActive ? "✔" : "✖"}
      </span>
    ),
  }));

  const renderAlarms = alarms.map(a => ({
    Id: a.id,
    類型: a.alarmType,
    訊息: a.message,
    時間: new Date(a.createdAt).toLocaleString(),
  }));

  const renderLogs = logs.map(l => ({
    Id: l.id,
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

  // 資料抓取
  useEffect(() => {
    let timer;
    const fetchData = async () => {
      try {
        const [machinesRes, alarmsRes, logsRes] = await Promise.all([
          fetch("/api/machine"),
          fetch("/api/machine/alarms/5"),
          fetch("/api/productionlog"),
        ]);
        setMachines(await machinesRes.json());
        setAlarms(await alarmsRes.json());
        setLogs(await logsRes.json());
      } catch (e) {
        // Handle error
      }
    };

    fetchData();
    timer = setInterval(fetchData, 15000);

    return () => clearInterval(timer);
  }, []);

  return (
    <div className="factory-dashboard-container">
      <h2 className="fd-title">機台資訊</h2>
      <Table columns={machineCols} data={renderMachines} />

      <h2 className="fd-title">最新警報</h2>
      <Table columns={alarmCols} data={renderAlarms} />

      <h2 className="fd-title">產出資料</h2>
      <Table columns={logsCols} data={renderLogs} />
    </div>
  );
};

export default FactoryDashboard;