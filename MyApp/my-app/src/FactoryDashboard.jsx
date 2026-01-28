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

  // 資料欄位 & 美化
  const machineCols = ["Id", "編號", "機台名稱", "啟用狀態"];
  const alarmCols = ["Id", "類型", "訊息", "時間"];
  const logsCols = [
    // "Id",
    "機台編號",
    "狀態",
    "YieldRate",
    "OutputQty",
    "Timestamp",
  ];

  // 美化呈現 values
  const renderMachines = machines.map((m) => ({
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

  const renderAlarms = alarms.slice(0, 50).map((a) => ({
    Id: a.id,
    類型: a.alarmType,
    訊息: a.message,
    時間: new Date(a.createdAt).toLocaleString(),
  }));

  const renderLogs = logs.slice(0, 50).map((l) => ({
    // Id: l.id,
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

  // 機台-每30秒抓一次
  useEffect(() => {
    let timer;
    const fetchMachines = async () => {
      try {
        const res = await fetch("/api/machine");
        setMachines(await res.json());
      } catch (e) {}
    };
    fetchMachines();
    timer = setInterval(fetchMachines, 30000); // 每30秒
    return () => clearInterval(timer);
  }, []);

  // 警報-每10秒抓一次
  useEffect(() => {
    let timer;
    const fetchAlarms = async () => {
      try {
        const res = await fetch("/api/machine/alarms/5");
        setAlarms(await res.json());
      } catch (e) {}
    };
    fetchAlarms();
    timer = setInterval(fetchAlarms, 10000); // 每10秒
    return () => clearInterval(timer);
  }, []);

  // 產出資料-每10秒抓一次
  useEffect(() => {
    let timer;
    const fetchLogs = async () => {
      try {
        const res = await fetch("/api/productionlog");
        setLogs(await res.json());
      } catch (e) {}
    };
    fetchLogs();
    timer = setInterval(fetchLogs, 10000); // 每10秒
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
