import React, { useState } from "react";
// 動態插入 style 讓按鈕 hover 時字體變白
const pageLinkBtnHoverStyle = `
.fd-page-link-btn:hover {
  color: #fff !important;
  transition: color 0.2s, background 0.2s;
}
`;

const currentUrl = window.location.pathname;
const pageLinks = [
  { name: "首頁", url: "/user" },
  { name: "生產資料", url: "/dashboard" },
  { name: "警報", url: "/alarm" },
  { name: "機台", url: "/machine" },
  { name: "統計", url: "/statistic" },
  { name: "會員", url: "/user" }
];

const PageLinks = () => {
    // 確保 style 只插入一次
    if (!document.getElementById('page-link-btn-hover-style')) {
      const style = document.createElement('style');
      style.id = 'page-link-btn-hover-style';
      style.innerHTML = pageLinkBtnHoverStyle;
      document.head.appendChild(style);
    }
  const [open, setOpen] = useState(false);
  const handleLogout = (e) => {
    e.preventDefault();
    localStorage.removeItem("token");
    window.location.href = "/user";
  };
  return (
    <div className="fd-page-link-row" style={{ position: "relative"}}>
      <button
        className="fd-page-link-btn"
        style={{ minWidth: 180, fontSize: "26px", height: 54, color: "#465374", boxShadow: "0 10px 10px #0002", fontWeight: 700, padding: "32px 32px", display: "flex", alignItems: "center", justifyContent: "space-between" }}
        onClick={() => setOpen((o) => !o)}
      >
        <span style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" style={{ marginRight: 8 }}>
            <rect x="3" y="6" width="18" height="2" rx="1" fill="#2b526e"/>
            <rect x="3" y="11" width="18" height="2" rx="1" fill="#2b526e"/>
            <rect x="3" y="16" width="18" height="2" rx="1" fill="#2b526e"/>
          </svg>
          頁面列表
        </span>
        <span style={{ fontSize: "32px", marginLeft: 12 }}>
          {open ? "▲" : "▼"}
        </span>
      </button>
      {open && (
        <div
          style={{
            position: "absolute",
            top: 48,
            left: 0,
            background: "var(--white)",
            border: "1px solid #dcdfe6",
            borderRadius: 8,
            boxShadow: "0 15px 15px #0002",
            zIndex: 10,
            minWidth: 180,
            padding: 8
          }}
        >
          {pageLinks.map((link) => (
            <a
              key={link.name}
              href={link.url}
              className={
                "fd-page-link-btn" +
                (currentUrl === link.url ? " fd-page-link-active" : "")
              }
              style={{ display: "block", margin: 0, marginBottom: 8 }}
              onClick={() => setOpen(false)}
            >
              {link.name}
            </a>
          ))}
          <a
            href="#logout"
            className="fd-page-link-btn"
            style={{ display: "block", margin: 0, color: "var(--error-red)" }}
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