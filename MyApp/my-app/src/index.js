import React from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import FactoryDashboard from './FactoryDashboard';
import reportWebVitals from './reportWebVitals';

const root = createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <FactoryDashboard />
  </React.StrictMode>
);

reportWebVitals();