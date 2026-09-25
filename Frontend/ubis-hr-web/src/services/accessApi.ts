import axios from "axios";

const accessApi = axios.create({
  baseURL: import.meta.env.VITE_ACCESS_API_URL,
  timeout: 15000,
});

accessApi.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  config.headers["X-Api-Key"] = import.meta.env.VITE_ACCESS_API_KEY;

  return config;
});

accessApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // 401 จาก Endpoint Login เอง = "รหัสผ่านผิด" ไม่ใช่ "Token หมดอายุ"
      // ต้องปล่อยให้ LoginView แสดงข้อความ Error เอง ห้าม Redirect
      const url = error.config?.url ?? "";
      if (url.includes("/Auth")) {
        return Promise.reject(error);
      }

      localStorage.removeItem("token");
      localStorage.removeItem("email");
      localStorage.removeItem("displayName");
      localStorage.removeItem("employeeCode");
      localStorage.removeItem("permissions");

      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);

export default accessApi;
