import React, { useState } from "react";
import PageLinks from "./CommonPage";
import "./user.css";

const User = () => {
  const [mode, setMode] = useState("login"); // "login" or "register"
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem("token"));
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");
    try {
      const res = await fetch("/api/User/Login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });
      const data = await res.json();
      if (res.ok && data.token) {
        localStorage.setItem("token", data.token);
        setIsLoggedIn(true);
        setMessage("登入成功！");
      } else {
        setMessage(data.message || "登入失敗");
      }
    } catch (e) {
      setMessage("伺服器連線失敗");
    }
    setLoading(false);
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");
    try {
      const res = await fetch("/api/User/Register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });
      const data = await res.json();
      if (res.ok) {
        setMessage("註冊成功，請登入！");
        setMode("login");
      } else {
        setMessage(data.message || "註冊失敗");
      }
    } catch (e) {
      setMessage("伺服器連線失敗");
    }
    setLoading(false);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    setIsLoggedIn(false);
    setMessage("已登出");
  };

  return (
    <div className="factory-dashboard-container">
      <PageLinks />
      <div className="user-form-wrapper">
        <h2 className="fd-title">
          {isLoggedIn ? "會員中心" : mode === "login" ? "登入" : "註冊"}
        </h2>
        <div
          className="fd-header-row fd-btn-group"
          style={{ marginBottom: 24 }}
        >
          {!isLoggedIn && (
            <>
              <button
                className={
                  "fd-page-link-btn" +
                  (mode === "login" ? " fd-page-link-active" : "")
                }
                onClick={() => setMode("login")}
                disabled={mode === "login"}
              >
                登入
              </button>
              <button
                className={
                  "fd-page-link-btn" +
                  (mode === "register" ? " fd-page-link-active" : "")
                }
                onClick={() => setMode("register")}
                disabled={mode === "register"}
              >
                註冊
              </button>
            </>
          )}
        </div>
        {message && (
          <div
            style={{
              color: message.includes("成功") ? "#27ae60" : "#c0392b",
              marginBottom: 16,
              textAlign: "center",
            }}
          >
            {message}
          </div>
        )}
        {!isLoggedIn ? (
          <form
            className="fd-add-form"
            onSubmit={mode === "login" ? handleLogin : handleRegister}
            style={{ marginTop: 8 }}
          >
            <div
              className="fd-input-group"
              style={{ display: "flex", flexDirection: "column" }}
            >
              <input
                type="text"
                placeholder="帳號"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                autoFocus
              />
              <input
                type="password"
                placeholder="密碼"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <button
                type="submit"
                className="fd-submit-btn"
                style={{ marginTop: 12 }}
                disabled={loading}
              >
                {loading ? "處理中..." : mode === "login" ? "登入" : "註冊"}
              </button>
            </div>
          </form>
        ) : (
          <div style={{ textAlign: "center", marginTop: 32 }}>
            <button className="fd-add-btn" onClick={handleLogout}>
              登出
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default User;
