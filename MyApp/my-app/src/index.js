import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';
import FactoryDashboard from './FactoryDashboard';
import User from './User';
import reportWebVitals from './reportWebVitals';

const root = createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/dashboard" element={<FactoryDashboard />} />
        <Route path="/user" element={<User />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);

reportWebVitals();