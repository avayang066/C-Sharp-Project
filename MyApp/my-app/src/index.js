import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './index.css';
import FactoryDashboard from './FactoryDashboard';
import User from './User';
import reportWebVitals from './reportWebVitals';

const root = createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        {/* 根路徑導向登入／會員頁 */}
        <Route path="/" element={<Navigate to="/user" replace />} />
        <Route path="/dashboard" element={<FactoryDashboard />} />
        <Route path="/user" element={<User />} />
        {/* 未實作的頁面（/alarm、/machine、/statistic）先一併導回首頁，避免空白畫面 */}
        <Route path="*" element={<Navigate to="/user" replace />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);

reportWebVitals();