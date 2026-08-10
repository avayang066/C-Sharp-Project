import React, { useState } from "react";

const currentUrl = window.location.pathname;
const pageLinks = [
  { name: "首頁", url: "/user" },
  { name: "生產資料", url: "/dashboard" },
  { name: "警報", url: "/alarm" },
  { name: "機台", url: "/machine" },
  { name: "統計", url: "/statistic" },
  { name: "會員", url: "/user" }
];

// 樣式一律寫在 factory-dashboard.css 的 .fd-nav-* class，
// 不用 inline style —— inline style 優先權高於 @media，會讓 RWD 失效。
const PageLinks = () => {
  const [open, setOpen] = useState(false);
  const handleLogout = (e) => {
    e.preventDefault();
    localStorage.removeItem("token");
    window.location.href = "/user";
  };
  return (
    <div className="fd-page-link-row">
      <button
        className="fd-page-link-btn fd-nav-toggle"
        onClick={() => setOpen((o) => !o)}
      >
        <span className="fd-nav-toggle-label">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden="true">
            <rect x="3" y="6" width="18" height="2" rx="1" fill="#2b526e"/>
            <rect x="3" y="11" width="18" height="2" rx="1" fill="#2b526e"/>
            <rect x="3" y="16" width="18" height="2" rx="1" fill="#2b526e"/>
          </svg>
          頁面列表
        </span>
        <span className="fd-nav-toggle-caret">{open ? "▲" : "▼"}</span>
      </button>
      {open && (
        <div className="fd-nav-menu">
          {pageLinks.map((link) => (
            <a
              key={link.name}
              href={link.url}
              className={
                "fd-page-link-btn fd-nav-menu-item" +
                (currentUrl === link.url ? " fd-page-link-active" : "")
              }
              onClick={() => setOpen(false)}
            >
              {link.name}
            </a>
          ))}
          <a
            href="#logout"
            className="fd-page-link-btn fd-nav-menu-item fd-nav-menu-logout"
            onClick={handleLogout}
          >
            登出
          </a>
        </div>
      )}
    </div>
  );
};
export default PageLinks;
