import React, { useEffect, useState } from 'react';
import './factory-dashboard.css';

function FactoryDashboard() {
  const [machines, setMachines] = useState([]);
  const [alarms, setAlarms] = useState([]);
  const [logs, setLogs] = useState([]);

  // 封裝抓資料的函式
  const fetchAndRender = async () => {
    try {
      // 機台
      const machinesRes = await fetch('/api/machine');
      const machinesData = await machinesRes.json();
      setMachines(machinesData);

      // 警報
      const alarmsRes = await fetch('/api/machine/alarms/5');
      const alarmsData = await alarmsRes.json();
      setAlarms(alarmsData);

      // 產出
      const logsRes = await fetch('/api/productionlog');
      const logsData = await logsRes.json();
      setLogs(logsData);
    } catch (err) {
      console.error('Error fetching dashboard data', err);
    }
  };

  // componentDidMount + 每 5 秒更新
  useEffect(() => {
    fetchAndRender();                 // 先抓一次
    const timer = setInterval(fetchAndRender, 5000); // 每 5 秒

    // componentWillUnmount: 清掉 interval
    return () => clearInterval(timer);
  }, []);

  return (
    <div>
      <h2>機台資訊</h2>
      <table id="machinesTable" className="fd-table">
        <thead>
          <tr>
            <th>Id</th>
            <th>編號</th>
            <th>機台名稱</th>
            <th>啟用狀態</th>
          </tr>
        </thead>
        <tbody>
          {machines.map(m => (
            <tr key={m.id}>
              <td>{m.id}</td>
              <td>{m.machineCode}</td>
              <td>{m.machineName}</td>
              <td>{m.isActive ? '✔' : '✖'}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>最新警報</h2>
      <table id="alarmsTable" className="fd-table">
        <thead>
          <tr>
            <th>Id</th>
            <th>類型</th>
            <th>訊息</th>
            <th>時間</th>
          </tr>
        </thead>
        <tbody>
          {alarms.map(a => (
            <tr key={a.id}>
              <td>{a.id}</td>
              <td>{a.alarmType}</td>
              <td>{a.message}</td>
              <td>{a.createdAt}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>產出資料</h2>
      <table id="productionTable" className="fd-table">
        <thead>
          <tr>
            <th>Id</th>
            <th>機台編號</th>
            <th>狀態</th>
            <th>YieldRate</th>
            <th>OutputQty</th>
            <th>Timestamp</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(l => (
            <tr key={l.id}>
              <td>{l.id}</td>
              <td>{l.machineId}</td>
              <td>{l.status}</td>
              <td>{l.yieldRate}</td>
              <td>{l.outputQty}</td>
              <td>{l.timestamp}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default FactoryDashboard;